#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Internal helper for recursively replacing placeholders in an object graph.
    /// </summary>
    internal class ObjectReplacer
    {
        private readonly Dictionary<string, string> _lookup;
        private readonly HashSet<object> _visited;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _isAsync;

        // Cache for generic method infos to avoid repeated reflection
        private static readonly MethodInfo _taskFromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult));

        // High-performance Type Accessor Cache (Expression Trees)
        private static readonly ConcurrentDictionary<Type, TypeAccessor> _typeAccessorCache = new ConcurrentDictionary<Type, TypeAccessor>();

        public ObjectReplacer(Dictionary<string, string> lookup, CancellationToken cancellationToken, bool isAsync)
        {
            _lookup = lookup;
            // Use ReferenceEqualityComparer to track object identity for cycle detection
            _visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            _cancellationToken = cancellationToken;
            _isAsync = isAsync;
        }

        #region Sync Implementation

        public void Process(object obj)
        {
            if (obj == null) return;

            // Check for cycles
            if (!_visited.Add(obj)) return;

            var type = obj.GetType();

            // Skip strings (leaves)
            if (type == typeof(string)) return;

            // Skip primitive types and common value types that are leaves
            if (IsLeafType(type)) return;

            // Handle Collections
            if (obj is IList list)
            {
                ProcessList(list);
                return;
            }
            else if (obj is IDictionary dictionary)
            {
                ProcessDictionary(dictionary);
                return;
            }
            else if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    Process(item);
                }
                return;
            }

            // Handle Regular Object Properties via Optimized Accessor
            var accessor = GetTypeAccessor(type);

            foreach (var prop in accessor.Properties)
            {
                if (!prop.CanRead) continue;

                try
                {
                    object currentValue = prop.Getter(obj);
                    if (currentValue == null) continue;

                    bool replaced = false;

                    // 1. Try String Replacement
                    if (currentValue is string strVal)
                    {
                        if (_lookup.TryGetValue(strVal, out var replacement))
                        {
                            if (prop.CanWrite)
                            {
                                // Only replace if the property can accept a string (e.g., string or object)
                                if (prop.PropertyType.IsAssignableFrom(typeof(string)))
                                {
                                    prop.Setter(obj, replacement);
                                    replaced = true;
                                }
                            }
                        }
                    }
                    // 2. Try ValueType Replacement (convert string replacement to target type)
                    else if (currentValue.GetType().IsValueType)
                    {
                        string s = currentValue.ToString();
                        if (!string.IsNullOrEmpty(s) && _lookup.TryGetValue(s, out var replacement))
                        {
                            if (prop.CanWrite)
                            {
                                try
                                {
                                    Type targetType = prop.PropertyType;
                                    if (targetType == typeof(object)) targetType = currentValue.GetType();

                                    var newValue = ClassExtensions.ConvertValue(replacement, targetType);

                                    bool canAssign = false;
                                    if (newValue == null)
                                    {
                                        if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                                            canAssign = true;
                                    }
                                    else
                                    {
                                        var newType = newValue.GetType();
                                        if (targetType.IsAssignableFrom(newType))
                                            canAssign = true;
                                        else
                                        {
                                            var underlying = Nullable.GetUnderlyingType(targetType);
                                            if (underlying != null && underlying.IsAssignableFrom(newType))
                                                canAssign = true;
                                        }
                                    }

                                    if (canAssign)
                                    {
                                        prop.Setter(obj, newValue);
                                        replaced = true;
                                    }
                                }
                                catch
                                {
                                    // Conversion failed, skip replacement
                                }
                            }
                        }
                    }

                    // 3. Recurse if not replaced
                    if (!replaced)
                    {
                        Process(currentValue);
                    }
                }
                catch
                {
                    // Ignore errors accessing properties
                }
            }
        }

        private void ProcessList(IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                object item = list[i];
                if (item == null) continue;

                bool replaced = false;

                if (item is string strVal)
                {
                    if (_lookup.TryGetValue(strVal, out var replacement))
                    {
                        if (!list.IsReadOnly)
                        {
                            list[i] = replacement;
                            replaced = true;
                        }
                    }
                }
                else if (item.GetType().IsValueType)
                {
                    string s = item.ToString();
                    if (!string.IsNullOrEmpty(s) && _lookup.TryGetValue(s, out var replacement))
                    {
                        if (!list.IsReadOnly)
                        {
                            try
                            {
                                var newValue = ClassExtensions.ConvertValue(replacement, item.GetType());
                                list[i] = newValue;
                                replaced = true;
                            }
                            catch { }
                        }
                    }
                }

                if (!replaced)
                {
                    Process(item);
                }
            }
        }

        private void ProcessDictionary(IDictionary dict)
        {
            var updates = new List<DictionaryEntry>();

            foreach (DictionaryEntry entry in dict)
            {
                bool replaced = false;

                if (entry.Value is string sVal && _lookup.TryGetValue(sVal, out var replacement))
                {
                    updates.Add(new DictionaryEntry(entry.Key, replacement));
                    replaced = true;
                }
                else if (entry.Value != null && entry.Value.GetType().IsValueType)
                {
                    string vs = entry.Value.ToString();
                    if (!string.IsNullOrEmpty(vs) && _lookup.TryGetValue(vs, out var r))
                    {
                        try
                        {
                            var newValue = ClassExtensions.ConvertValue(r, entry.Value.GetType());
                            updates.Add(new DictionaryEntry(entry.Key, newValue));
                            replaced = true;
                        }
                        catch { }
                    }
                }

                if (!replaced && entry.Value != null)
                {
                    Process(entry.Value);
                }
            }

            foreach (var update in updates)
            {
                dict[update.Key] = update.Value;
            }
        }

        #endregion

        #region Async Implementation

        public async Task ProcessAsync(object obj)
        {
            if (obj == null) return;

            // Check for cycles
            if (!_visited.Add(obj)) return;

            _cancellationToken.ThrowIfCancellationRequested();

            var type = obj.GetType();

            if (type == typeof(string)) return;
            if (IsLeafType(type)) return;

            if (obj is IList list)
            {
                await ProcessListAsync(list);
                return;
            }
            else if (obj is IDictionary dictionary)
            {
                await ProcessDictionaryAsync(dictionary);
                return;
            }
            else if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    await ProcessAsync(item);
                }
                return;
            }

            var accessor = GetTypeAccessor(type);

            foreach (var prop in accessor.Properties)
            {
                if (!prop.CanRead) continue;

                try
                {
                    object currentValue = prop.Getter(obj);
                    if (currentValue == null) continue;

                    // Async Property Handling
                    if (_isAsync && IsTask(prop.PropertyType))
                    {
                        await HandleAsyncPropertyAsync(obj, prop, currentValue);
                        continue;
                    }

                    bool replaced = false;

                    if (currentValue is string strVal)
                    {
                        if (_lookup.TryGetValue(strVal, out var replacement))
                        {
                            if (prop.CanWrite && prop.PropertyType.IsAssignableFrom(typeof(string)))
                            {
                                prop.Setter(obj, replacement);
                                replaced = true;
                            }
                        }
                    }
                    else if (currentValue.GetType().IsValueType)
                    {
                        string s = currentValue.ToString();
                        if (!string.IsNullOrEmpty(s) && _lookup.TryGetValue(s, out var replacement))
                        {
                            if (prop.CanWrite)
                            {
                                try
                                {
                                    Type targetType = prop.PropertyType;
                                    if (targetType == typeof(object)) targetType = currentValue.GetType();

                                    var newValue = ClassExtensions.ConvertValue(replacement, targetType);

                                    bool canAssign = false;
                                    if (newValue == null)
                                    {
                                        if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                                            canAssign = true;
                                    }
                                    else
                                    {
                                        var newType = newValue.GetType();
                                        if (targetType.IsAssignableFrom(newType))
                                            canAssign = true;
                                        else
                                        {
                                            var underlying = Nullable.GetUnderlyingType(targetType);
                                            if (underlying != null && underlying.IsAssignableFrom(newType))
                                                canAssign = true;
                                        }
                                    }

                                    if (canAssign)
                                    {
                                        prop.Setter(obj, newValue);
                                        replaced = true;
                                    }
                                }
                                catch { }
                            }
                        }
                    }

                    if (!replaced)
                    {
                        await ProcessAsync(currentValue);
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        private async Task ProcessListAsync(IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                object item = list[i];
                if (item == null) continue;

                // Handle Task<T> items in list
                if (_isAsync && item is Task taskItem && IsTask(item.GetType()))
                {
                    await taskItem.ConfigureAwait(false);
                    var resultProp = item.GetType().GetProperty("Result");
                    if (resultProp != null)
                    {
                        var result = resultProp.GetValue(item);
                        if (result != null)
                        {
                            bool taskReplaced = false;
                            string replacementVal = null;

                            if (result is string s && _lookup.TryGetValue(s, out var rep))
                            {
                                replacementVal = rep;
                                taskReplaced = true;
                            }
                            else if (result.GetType().IsValueType)
                            {
                                string sVal = result.ToString();
                                if (!string.IsNullOrEmpty(sVal) && _lookup.TryGetValue(sVal, out var r))
                                {
                                    replacementVal = r;
                                    taskReplaced = true;
                                }
                            }

                            if (taskReplaced && !list.IsReadOnly)
                            {
                                try
                                {
                                    var newTask = CreateTaskFromResult(item.GetType(), replacementVal);
                                    list[i] = newTask;
                                }
                                catch { }
                            }
                            else if (!taskReplaced)
                            {
                                // Recurse into the result if it's a complex object
                                await ProcessAsync(result);
                            }
                        }
                    }
                    continue;
                }

                bool replaced = false;

                if (item is string strVal)
                {
                    if (_lookup.TryGetValue(strVal, out var replacement))
                    {
                        if (!list.IsReadOnly)
                        {
                            list[i] = replacement;
                            replaced = true;
                        }
                    }
                }
                else if (item.GetType().IsValueType)
                {
                    string s = item.ToString();
                    if (!string.IsNullOrEmpty(s) && _lookup.TryGetValue(s, out var replacement))
                    {
                        if (!list.IsReadOnly)
                        {
                            try
                            {
                                var newValue = ClassExtensions.ConvertValue(replacement, item.GetType());
                                list[i] = newValue;
                                replaced = true;
                            }
                            catch { }
                        }
                    }
                }

                if (!replaced)
                {
                    await ProcessAsync(item);
                }
            }
        }

        private async Task ProcessDictionaryAsync(IDictionary dict)
        {
            var updates = new List<DictionaryEntry>();

            foreach (DictionaryEntry entry in dict)
            {
                bool replaced = false;

                // Handle Task<T> values in dictionary
                if (_isAsync && entry.Value is Task taskItem && IsTask(entry.Value.GetType()))
                {
                    await taskItem.ConfigureAwait(false);
                    var resultProp = entry.Value.GetType().GetProperty("Result");
                    if (resultProp != null)
                    {
                        var result = resultProp.GetValue(entry.Value);
                        if (result != null)
                        {
                            bool taskReplaced = false;
                            string replacementVal = null;

                            if (result is string s && _lookup.TryGetValue(s, out var rep))
                            {
                                replacementVal = rep;
                                taskReplaced = true;
                            }
                            else if (result.GetType().IsValueType)
                            {
                                string sVal = result.ToString();
                                if (!string.IsNullOrEmpty(sVal) && _lookup.TryGetValue(sVal, out var r))
                                {
                                    replacementVal = r;
                                    taskReplaced = true;
                                }
                            }

                            if (taskReplaced)
                            {
                                try
                                {
                                    var newTask = CreateTaskFromResult(entry.Value.GetType(), replacementVal);
                                    updates.Add(new DictionaryEntry(entry.Key, newTask));
                                    replaced = true;
                                }
                                catch { }
                            }
                            else
                            {
                                await ProcessAsync(result);
                            }
                        }
                    }
                }
                else if (entry.Value is string sVal && _lookup.TryGetValue(sVal, out var replacement))
                {
                    updates.Add(new DictionaryEntry(entry.Key, replacement));
                    replaced = true;
                }
                else if (entry.Value != null && entry.Value.GetType().IsValueType)
                {
                    string vs = entry.Value.ToString();
                    if (!string.IsNullOrEmpty(vs) && _lookup.TryGetValue(vs, out var r))
                    {
                        try
                        {
                            var newValue = ClassExtensions.ConvertValue(r, entry.Value.GetType());
                            updates.Add(new DictionaryEntry(entry.Key, newValue));
                            replaced = true;
                        }
                        catch { }
                    }
                }

                if (!replaced && entry.Value != null && !(entry.Value is Task)) // Skip recursing into Task again
                {
                    await ProcessAsync(entry.Value);
                }
            }

            foreach (var update in updates)
            {
                dict[update.Key] = update.Value;
            }
        }

        private async Task HandleAsyncPropertyAsync(object obj, FastProperty prop, object taskObj)
        {
            if (taskObj is Task task)
            {
                await task.ConfigureAwait(false);

                var resultProp = task.GetType().GetProperty("Result");
                if (resultProp != null)
                {
                    var result = resultProp.GetValue(task);
                    if (result != null)
                    {
                        bool replaced = false;
                        string replacementVal = null;

                        if (result is string s && _lookup.TryGetValue(s, out var replacement))
                        {
                            replacementVal = replacement;
                            replaced = true;
                        }
                        else if (result.GetType().IsValueType)
                        {
                            string vs = result.ToString();
                            if (!string.IsNullOrEmpty(vs) && _lookup.TryGetValue(vs, out var r))
                            {
                                replacementVal = r;
                                replaced = true;
                            }
                        }

                        if (replaced)
                        {
                            if (prop.CanWrite)
                            {
                                var newTask = CreateTaskFromResult(prop.PropertyType, replacementVal);
                                prop.Setter(obj, newTask);
                            }
                        }
                        else
                        {
                            // Recurse into the result if it's a complex object
                            // This modifies the object in-place if it's a reference type
                            await ProcessAsync(result);
                        }
                    }
                }
            }
        }

        #endregion

        private object CreateTaskFromResult(Type taskType, object result)
        {
            var genericArg = taskType.GetGenericArguments()[0];
            var typedResult = ClassExtensions.ConvertValue(result.ToString(), genericArg);
            var method = _taskFromResultMethod.MakeGenericMethod(genericArg);
            return method.Invoke(null, new[] { typedResult });
        }

        private bool IsTask(Type type)
        {
            return typeof(Task).IsAssignableFrom(type) && type.IsGenericType;
        }

        private bool IsLeafType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(Guid) ||
                   type == typeof(DateTime) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(decimal);
        }

        private TypeAccessor GetTypeAccessor(Type type)
        {
            return _typeAccessorCache.GetOrAdd(type, t => new TypeAccessor(t));
        }

        // --- Helper Classes for Expression Trees ---

        private class TypeAccessor
        {
            public readonly FastProperty[] Properties;

            public TypeAccessor(Type type)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var list = new List<FastProperty>(props.Length);
                foreach (var p in props)
                {
                    if (p.GetIndexParameters().Length > 0) continue;
                    list.Add(new FastProperty(p));
                }
                Properties = list.ToArray();
            }
        }

        private class FastProperty
        {
            public readonly Type PropertyType;
            public readonly bool CanRead;
            public readonly bool CanWrite;
            public readonly Func<object, object> Getter;
            public readonly Action<object, object> Setter;

            public FastProperty(PropertyInfo prop)
            {
                PropertyType = prop.PropertyType;
                CanRead = prop.CanRead;
                CanWrite = prop.CanWrite;

                // Build Getter
                if (CanRead)
                {
                    var instance = Expression.Parameter(typeof(object), "instance");
                    var castInstance = Expression.Convert(instance, prop.DeclaringType);
                    var propertyAccess = Expression.Property(castInstance, prop);
                    var castResult = Expression.Convert(propertyAccess, typeof(object));
                    Getter = Expression.Lambda<Func<object, object>>(castResult, instance).Compile();
                }

                // Build Setter
                if (CanWrite)
                {
                    var instance = Expression.Parameter(typeof(object), "instance");
                    var value = Expression.Parameter(typeof(object), "value");
                    var castInstance = Expression.Convert(instance, prop.DeclaringType);
                    var castValue = Expression.Convert(value, prop.PropertyType);
                    var propertyAccess = Expression.Property(castInstance, prop);
                    var assign = Expression.Assign(propertyAccess, castValue);
                    Setter = Expression.Lambda<Action<object, object>>(assign, instance, value).Compile();
                }
            }
        }
    }

    internal class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
