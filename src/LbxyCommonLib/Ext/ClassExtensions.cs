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

            var properties = PropertyAccessor<T>.Properties;

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
    }
}
