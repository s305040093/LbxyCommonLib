#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// 泛型缓存类，用于存储特定类型的属性元数据。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public static class PropertyAccessor<T>
        where T : class
    {
        /// <summary>
        /// 获取所有公共实例属性的元数据列表。
        /// </summary>
        public static readonly IReadOnlyList<PropertyMetadata> Properties;

        private static readonly Dictionary<string, PropertyMetadata> NameMap;
        private static readonly Dictionary<string, PropertyMetadata> DisplayNameMap;

        static PropertyAccessor()
        {
            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<PropertyMetadata>();

            NameMap = new Dictionary<string, PropertyMetadata>(StringComparer.OrdinalIgnoreCase);
            DisplayNameMap = new Dictionary<string, PropertyMetadata>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in props)
            {
                if (p.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                try
                {
                    var metadata = new PropertyMetadata(p);
                    list.Add(metadata);

                    if (!NameMap.ContainsKey(metadata.Name))
                    {
                        NameMap[metadata.Name] = metadata;
                    }

                    if (!DisplayNameMap.ContainsKey(metadata.DisplayName))
                    {
                        DisplayNameMap[metadata.DisplayName] = metadata;
                    }
                }
                catch
                {
                    // 忽略处理失败的属性
                }
            }

            Properties = list.AsReadOnly();
        }

        /// <summary>
        /// 根据属性名称获取显示名称。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>显示名称。</returns>
        public static string GetDisplayName(string propertyName)
        {
            return GetDisplayName(propertyName, useDisplayName: false);
        }

        /// <summary>
        /// 根据属性名称或显示名称获取显示名称。
        /// </summary>
        /// <param name="propertyName">属性名称或显示名称。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        /// <returns>显示名称。</returns>
        public static string GetDisplayName(string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var meta = GetMetadata(propertyName, useDisplayName, comparison);
            return meta != null ? meta.DisplayName : propertyName;
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>属性值。</returns>
        public static object GetValue(T instance, string propertyName)
        {
            return GetValue(instance, propertyName, useDisplayName: false);
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称或显示名称。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        /// <returns>属性值。</returns>
        public static object GetValue(T instance, string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var meta = GetMetadata(propertyName, useDisplayName, comparison);
            if (meta == null)
            {
                if (useDisplayName)
                {
                    var availableNames = string.Join(", ", DisplayNameMap.Keys);
                    throw new ArgumentException($"Property with DisplayName '{propertyName}' not found on type '{typeof(T).Name}'. Available DisplayNames: {availableNames}", nameof(propertyName));
                }

                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.", nameof(propertyName));
            }

            if (!meta.CanRead)
            {
                throw new InvalidOperationException($"Property '{meta.Name}' is not readable.");
            }

            return meta.Getter(instance);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="value">要设置的值。</param>
        public static void SetValue(T instance, string propertyName, object value)
        {
            SetValue(instance, propertyName, value, useDisplayName: true);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称或显示名称。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        public static void SetValue(T instance, string propertyName, object value, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var meta = GetMetadata(propertyName, useDisplayName, comparison);
            if (meta == null)
            {
                if (useDisplayName)
                {
                    var availableNames = string.Join(", ", DisplayNameMap.Keys);
                    throw new ArgumentException($"Property with DisplayName '{propertyName}' not found on type '{typeof(T).Name}'. Available DisplayNames: {availableNames}", nameof(propertyName));
                }

                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.", nameof(propertyName));
            }

            if (!meta.CanWrite)
            {
                throw new InvalidOperationException($"Property '{meta.Name}' is not writable.");
            }

            meta.Setter(instance, value);
        }

        private static PropertyMetadata GetMetadata(string key, bool useDisplayName, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            // 优化：针对 Ordinal/OrdinalIgnoreCase 使用字典查找
            var canUseMap = comparison == StringComparison.Ordinal || comparison == StringComparison.OrdinalIgnoreCase;

            if (useDisplayName)
            {
                if (canUseMap && DisplayNameMap.TryGetValue(key, out var meta))
                {
                    if (comparison == StringComparison.Ordinal && !string.Equals(meta.DisplayName, key, comparison))
                    {
                        return null;
                    }

                    return meta;
                }

                if (!canUseMap)
                {
                    foreach (var prop in Properties)
                    {
                        if (string.Equals(prop.DisplayName, key, comparison))
                        {
                            return prop;
                        }
                    }
                }

                return null;
            }
            else
            {
                // 兼容旧行为：先尝试 Name，再尝试 DisplayName
                if (canUseMap)
                {
                    if (NameMap.TryGetValue(key, out var meta))
                    {
                        if (comparison == StringComparison.Ordinal && !string.Equals(meta.Name, key, comparison))
                        {
                            // 名称匹配但大小写不符，尝试 DisplayName 匹配（为了保持逻辑一致性）
                            goto TryDisplayName;
                        }

                        return meta;
                    }

                TryDisplayName:
                    if (DisplayNameMap.TryGetValue(key, out meta))
                    {
                        if (comparison == StringComparison.Ordinal && !string.Equals(meta.DisplayName, key, comparison))
                        {
                            return null;
                        }

                        return meta;
                    }

                    return null;
                }
                else
                {
                    foreach (var prop in Properties)
                    {
                        if (string.Equals(prop.Name, key, comparison))
                        {
                            return prop;
                        }
                    }

                    foreach (var prop in Properties)
                    {
                        if (string.Equals(prop.DisplayName, key, comparison))
                        {
                            return prop;
                        }
                    }

                    return null;
                }
            }
        }
    }
}
