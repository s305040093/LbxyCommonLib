#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 提供对象属性提取与转换的扩展方法。
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// 将对象的公共实例属性转换为字典。
        /// </summary>
        /// <typeparam name="T">对象类型（必须为类）。</typeparam>
        /// <param name="source">源对象实例。</param>
        /// <param name="useDisplayName">是否使用属性显示名称作为键。默认为 <c>true</c>（使用显示名称）。</param>
        /// <returns>包含属性名和值的字典。</returns>
        public static Dictionary<string, string> ToPropertyDictionary<T>(this T source, bool useDisplayName = true)
            where T : class
        {
            var target = new Dictionary<string, string>();

            if (source == null)
            {
                return target;
            }

            var properties = PropertyAccessor.GetProperties<T>();

            foreach (var prop in properties)
            {
                try
                {
                    if (!prop.CanRead)
                    {
                        continue;
                    }

                    var key = useDisplayName ? prop.DisplayName : prop.Name;
                    var valStr = prop.StringGetter(source);
                    target[key] = valStr;
                }
                catch
                {
                    // 忽略异常
                }
            }

            return target;
        }

        /// <summary>
        /// 将当前对象转换为属性字典，并与外部字典合并或替换。
        /// </summary>
        /// <typeparam name="T">当前对象类型（必须为类）。</typeparam>
        /// <param name="source">当前对象实例。</param>
        /// <param name="src">需要合并或替换的源字典。</param>
        /// <param name="strategy">合并冲突策略。默认为 <c>Overwrite</c>（覆盖目标值）。</param>
        /// <param name="isReplace">是否为替换模式。默认为 <c>false</c>（合并模式）。
        /// <para>若为 <c>true</c>，则仅替换已存在的键，忽略源字典中新增的键。</para>
        /// <para>若为 <c>false</c>，则执行合并操作，新增不存在的键，并根据策略处理冲突。</para>
        /// </param>
        /// <param name="useDisplayName">是否使用属性显示名称作为键。默认为 <c>true</c>。</param>
        /// <returns>合并后的新字典实例。原对象和输入字典均不会被修改。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="source"/> 为 null 时抛出。</exception>
        /// <remarks>
        /// <para>此方法首先调用 <see cref="ToPropertyDictionary{T}"/> 将当前对象转换为字典。</para>
        /// <para>然后调用 <see cref="Collections.DictionaryOperations"/> 模块的功能，将 <paramref name="src"/> 合并到该字典中。</para>
        /// <para>如果 <paramref name="isReplace"/> 为 true，则仅更新已存在的键；否则将新增不存在的键。</para>
        /// </remarks>
        public static Dictionary<string, string> MergeOrReplace<T>(
            this T source,
            Dictionary<string, string> src,
            Collections.DictionaryConflictStrategy strategy = Collections.DictionaryConflictStrategy.Overwrite,
            bool isReplace = false,
            bool useDisplayName = true)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // 1. 将当前对象转换为属性字典（作为基准字典）
            // ToPropertyDictionary 返回的是新创建的字典，因此可以直接作为 result 使用，无需再次复制，满足不可变语义（针对 source）
            var result = source.ToPropertyDictionary(useDisplayName);

            // 2. 如果源字典为 null 或空，直接返回当前对象的属性字典
            if (src == null || src.Count == 0)
            {
                return result;
            }

            // 3. 根据模式调用 DictionaryOperations
            if (isReplace)
            {
                // 替换模式：仅替换已存在的键
                // 注意：Replace 目前不支持 ConflictStrategy，它总是覆盖。
                // 如果需要支持 ConflictStrategy，DictionaryOperations.Replace 可能需要扩展，或者在这里自行处理。
                // 根据 DictionaryOperations.Replace 的定义，它只是简单的赋值覆盖。
                Collections.DictionaryOperations.Replace(result, src);
            }
            else
            {
                // 合并模式：默认覆盖冲突键
                // 使用 Shallow 模式，因为 TValue 是 string，不需要深度合并
                Collections.DictionaryOperations.Merge(result, src, Collections.DictionaryMergeMode.Shallow, strategy);
            }

            return result;
        }

        /// <summary>
        /// 使用字典中的替换值更新对象的公共实例属性值（占位符替换）。
        /// </summary>
        /// <typeparam name="T">对象类型（必须为类）。</typeparam>
        /// <param name="source">源对象实例。</param>
        /// <param name="replacementMap">包含占位符和替换值的字典（Key: 占位符, Value: 替换值）。</param>
        /// <param name="comparer">字典键匹配的比较器。默认为 <c>null</c>（使用字典自身的比较规则或默认规则）。</param>
        /// <returns>已完成替换的源对象实例（支持链式调用）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="replacementMap"/> 为 null。</exception>
        /// <remarks>
        /// <para>处理逻辑：</para>
        /// <list type="number">
        /// <item>递归遍历对象图（包括嵌套对象、集合、字典）。</item>
        /// <item>若属性或集合元素的可读字符串值存在于 <paramref name="replacementMap"/> 的键中，则视为占位符。</item>
        /// <item>将对应的替换值（Value）转换为属性的目标类型并赋值。</item>
        /// </list>
        /// <para>高级特性：</para>
        /// <list type="bullet">
        /// <item>支持深度嵌套对象和循环引用检测。</item>
        /// <item>支持列表（IList）、字典（IDictionary）及任意 IEnumerable 集合的遍历与替换。</item>
        /// </list>
        /// <para>性能优化：</para>
        /// <item>使用表达式树（Expression Tree）缓存属性访问委托，性能接近原生代码。</item>
        /// </remarks>
        /// <example>
        /// <code>
        /// var user = new User { Name = "${Name}", Role = "Admin", Address = new Address { City = "${City}" } };
        /// var dict = new Dictionary&lt;string, string&gt; { { "${Name}", "Alice" }, { "${City}", "New York" } };
        /// user.ReplacePlaceholdersFromDictionary(dict);
        /// // user.Name becomes "Alice", user.Address.City becomes "New York"
        /// </code>
        /// </example>
        public static T ReplacePlaceholdersFromDictionary<T>(
            this T source,
            Dictionary<string, string> replacementMap,
            StringComparer comparer = null)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (replacementMap == null)
            {
                throw new ArgumentNullException(nameof(replacementMap));
            }

            if (replacementMap.Count == 0)
            {
                return source;
            }

            var lookup = (comparer == null)
                ? replacementMap
                : new Dictionary<string, string>(replacementMap, comparer);

            var replacer = new ObjectReplacer(lookup, CancellationToken.None, isAsync: false);
            replacer.Process(source);

            return source;
        }

        /// <summary>
        /// 将对象转换为属性字典，并使用替换映射表对字典值中的占位符进行替换。
        /// </summary>
        /// <typeparam name="T">对象类型（必须为类）。</typeparam>
        /// <param name="source">源对象实例。</param>
        /// <param name="replacementMap">包含占位符和替换值的字典。</param>
        /// <param name="useDisplayName">是否使用属性显示名称作为键。默认为 <c>true</c>。</param>
        /// <param name="comparer">字典键匹配的比较器。默认为 <c>null</c>。</param>
        /// <returns>包含已替换值的全新属性字典。源对象不会被修改。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> 为 null。</exception>
        /// <remarks>
        /// <para>此方法执行流程：</para>
        /// <list type="number">
        /// <item>调用 <see cref="ToPropertyDictionary{T}"/> 将对象转换为 <see cref="Dictionary{TKey, TValue}"/>。</item>
        /// <item>使用 <paramref name="replacementMap"/> 对生成的字典中的值（Value）进行批量替换。</item>
        /// </list>
        /// <para>注意：此操作不会修改 <paramref name="source"/> 对象的任何属性，仅返回处理后的字典。</para>
        /// </remarks>
        public static Dictionary<string, string> ToPropertyDictionaryWithReplacement<T>(
            this T source,
            Dictionary<string, string> replacementMap,
            bool useDisplayName = true,
            StringComparer comparer = null)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // 1. 先转换为属性字典（这是全新的字典对象，修改它不会影响源对象）
            var dict = source.ToPropertyDictionary(useDisplayName);

            // 2. 如果没有替换规则，直接返回
            if (replacementMap == null || replacementMap.Count == 0)
            {
                return dict;
            }

            // 3. 准备替换器
            var lookup = (comparer == null)
                ? replacementMap
                : new Dictionary<string, string>(replacementMap, comparer);

            // 使用 ObjectReplacer 对字典进行原地替换
            // ObjectReplacer 支持 IDictionary 处理，会遍历并替换匹配的值
            var replacer = new ObjectReplacer(lookup, CancellationToken.None, isAsync: false);
            replacer.Process(dict);

            return dict;
        }

        /// <summary>
        /// 异步将对象属性提取与转换的扩展方法。
        /// </summary>
        /// <remarks>
        /// <para>处理逻辑：</para>
        /// <list type="number">
        /// <item>递归遍历对象图（包括嵌套对象、集合、字典）。</item>
        /// <item>若属性或集合元素的可读字符串值存在于 <paramref name="replacementMap"/> 的键中，则视为占位符。</item>
        /// <item>将对应的替换值（Value）转换为属性的目标类型并赋值。</item>
        /// </list>
        /// <para>高级特性：</para>
        /// <list type="bullet">
        /// <item>支持深度嵌套对象和循环引用检测。</item>
        /// <item>支持列表（IList）、字典（IDictionary）及任意 IEnumerable 集合的遍历与替换。</item>
        /// <item>支持异步属性（Task&lt;T&gt;）的等待与结果替换（仅异步方法）。</item>
        /// </list>
        /// <para>性能优化：</para>
        /// <item>使用表达式树（Expression Tree）缓存属性访问委托，性能接近原生代码。</item>
        /// </remarks>
        /// <example>
        /// <code>
        /// await user.ReplacePlaceholdersFromDictionaryAsync(dict, cancellationToken: token);
        /// </code>
        /// </example>
        public static Task<T> ReplacePlaceholdersFromDictionaryAsync<T>(
            this T source,
            Dictionary<string, string> replacementMap,
            StringComparer comparer = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // 如果字典为空，直接返回完成的任务
            if (replacementMap == null || replacementMap.Count == 0)
            {
                return Task.FromResult(source);
            }

            return Task.Run(
                async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var lookup = (comparer == null)
                        ? replacementMap
                        : new Dictionary<string, string>(replacementMap, comparer);

                    var replacer = new ObjectReplacer(lookup, cancellationToken, isAsync: true);
                    await replacer.ProcessAsync(source);

                    return source;
                },
                cancellationToken);
        }

        internal static object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return value;
            }

            // Handle Nullable
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                targetType = underlyingType;
            }

            // Handle Enum
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value);
            }

            // Handle Guid
            if (targetType == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            // Handle TimeSpan
            if (targetType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(value);
            }

            // Handle IConvertible (int, double, bool, etc.)
            return Convert.ChangeType(value, targetType);
        }
    }
}
