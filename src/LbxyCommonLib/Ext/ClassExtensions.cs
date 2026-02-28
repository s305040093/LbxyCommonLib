#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 提供对象属性提取与转换的扩展方法。
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// 属性名称提取选项。
        /// </summary>
        public enum PropertyNameOptions
        {
            /// <summary>
            /// 仅使用属性名称（默认）。
            /// </summary>
            Default = 0,

            /// <summary>
            /// 优先使用显示名称特性 (XafDisplayName > DisplayName > Display)。
            /// </summary>
            UseDisplayAttributes = 1,
        }

        /// <summary>
        /// 将对象的公共实例属性转换为字典。
        /// </summary>
        /// <typeparam name="T">对象类型（必须为类）。</typeparam>
        /// <param name="source">源对象实例。</param>
        /// <param name="target">目标字典（可选）。若提供则进行覆盖更新。</param>
        /// <param name="options">属性名称提取选项。</param>
        /// <param name="onlyUpdateExisting">仅更新现有键（可选）。默认值为 <c>false</c>（允许新增属性到字典）；若设为 <c>true</c>，则仅更新字典中已存在的键，忽略源对象中的其他属性。</param>
        /// <returns>包含属性名和值的字典。</returns>
        public static Dictionary<string, string> ToPropertyDictionary<T>(this T source, Dictionary<string, string> target = null, PropertyNameOptions options = PropertyNameOptions.Default, bool onlyUpdateExisting = false)
            where T : class
        {
            if (target == null)
            {
                target = new Dictionary<string, string>();
            }

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

                    var key = (options == PropertyNameOptions.UseDisplayAttributes) ? prop.DisplayName : prop.Name;

                    if (onlyUpdateExisting && !target.ContainsKey(key))
                    {
                        continue;
                    }

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
    }
}
