#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 提供公共实例属性的统一访问入口（读取、写入、元数据）。
    /// 默认按显示名称（DisplayName）匹配属性；显示名称解析优先级为 XafDisplayName &gt; DisplayName &gt; Display &gt; 属性名。
    /// </summary>
    /// <remarks>
    /// <para>线程安全：每个类型的属性元数据在首次访问时构建并缓存，后续调用为只读访问。</para>
    /// <para>性能：字符串比较规则为 Ordinal/OrdinalIgnoreCase 时，名称匹配使用字典查找；其余比较模式使用线性扫描。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 模型：DisplayName = "Display_Name"
    /// // public sealed class Model { [DisplayName("Display_Name")] public string Name { get; set; } }
    ///
    /// var model = new Model();
    ///
    /// // 1. 默认：按显示名称匹配（useDisplayName = true）
    /// var v1 = PropertyAccessor.GetValue(model, "Display_Name");
    /// PropertyAccessor.SetValue(model, "Display_Name", "NewValue");
    ///
    /// // 2. 按属性名匹配：显式 useDisplayName = false
    /// var v2 = PropertyAccessor.GetValue(model, nameof(Model.Name), useDisplayName: false);
    /// PropertyAccessor.SetValue(model, nameof(Model.Name), "NewValue", useDisplayName: false);
    /// </code>
    /// </example>
    public static class PropertyAccessor
    {
        /// <summary>
        /// 按显示名称匹配属性并返回其显示名称。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="propertyName">属性显示名称（DisplayName）。</param>
        /// <returns>匹配成功返回该属性的显示名称；未匹配返回 <paramref name="propertyName"/> 原值。</returns>
        /// <remarks>
        /// 该重载固定使用 <c>useDisplayName: true</c> 进行匹配。
        /// </remarks>
        public static string GetDisplayName<T>(string propertyName)
            where T : class
        {
            return PropertyAccessor<T>.GetDisplayName(propertyName, useDisplayName: true);
        }

        /// <summary>
        /// 获取指定属性的显示名称或属性名称。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="propertyName">用于匹配的键，含义由 <paramref name="useDisplayName"/> 决定。</param>
        /// <param name="useDisplayName">
        /// 为 <c>true</c> 时返回属性的显示名称（DisplayName）；
        /// 为 <c>false</c> 时返回属性的原始名称（PropertyName）。
        /// </param>
        /// <param name="comparison">字符串比较规则。</param>
        /// <returns>匹配成功返回对应的名称；未匹配返回 <paramref name="propertyName"/> 原值。</returns>
        /// <remarks>
        /// 该方法不会抛出“未找到属性”的异常；未匹配时直接返回输入值。
        /// </remarks>
        public static string GetDisplayName<T>(string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return PropertyAccessor<T>.GetDisplayName(propertyName, useDisplayName, comparison);
        }

        /// <summary>
        /// 按显示名称匹配属性并获取其值。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="instance">对象实例，不允许为 <c>null</c>。</param>
        /// <param name="propertyName">属性显示名称（DisplayName），不允许为 <c>null</c> 或空字符串。</param>
        /// <returns>属性值。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">未找到匹配的属性显示名称，或 <paramref name="propertyName"/> 为 <c>null</c>/空字符串。</exception>
        /// <exception cref="InvalidOperationException">目标属性不可读。</exception>
        /// <remarks>
        /// 该重载固定使用 <c>useDisplayName: true</c> 进行匹配。
        /// </remarks>
        public static object GetValue<T>(T instance, string propertyName)
            where T : class
        {
            return PropertyAccessor<T>.GetValue(instance, propertyName, useDisplayName: true);
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="instance">对象实例，不允许为 <c>null</c>。</param>
        /// <param name="propertyName">用于匹配的键，含义由 <paramref name="useDisplayName"/> 决定，不允许为 <c>null</c> 或空字符串。</param>
        /// <param name="useDisplayName">
        /// 为 <c>true</c> 时按显示名称（DisplayName）匹配；为 <c>false</c> 时先按属性名（PropertyName）匹配，未命中再按显示名称匹配。
        /// </param>
        /// <param name="comparison">字符串比较规则。</param>
        /// <returns>属性值。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">未找到匹配属性，或 <paramref name="propertyName"/> 为 <c>null</c>/空字符串。</exception>
        /// <exception cref="InvalidOperationException">目标属性不可读。</exception>
        public static object GetValue<T>(T instance, string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return PropertyAccessor<T>.GetValue(instance, propertyName, useDisplayName, comparison);
        }

        /// <summary>
        /// 按显示名称匹配属性并设置其值。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="instance">对象实例，不允许为 <c>null</c>。</param>
        /// <param name="propertyName">属性显示名称（DisplayName），不允许为 <c>null</c> 或空字符串。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">未找到匹配的属性显示名称，或 <paramref name="propertyName"/> 为 <c>null</c>/空字符串。</exception>
        /// <exception cref="InvalidOperationException">目标属性不可写。</exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/> 不能赋值给目标属性类型。</exception>
        /// <remarks>
        /// 该重载固定使用 <c>useDisplayName: true</c> 进行匹配。
        /// </remarks>
        public static void SetValue<T>(T instance, string propertyName, object value)
            where T : class
        {
            PropertyAccessor<T>.SetValue(instance, propertyName, value, useDisplayName: true);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <param name="instance">对象实例，不允许为 <c>null</c>。</param>
        /// <param name="propertyName">用于匹配的键，含义由 <paramref name="useDisplayName"/> 决定，不允许为 <c>null</c> 或空字符串。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="useDisplayName">
        /// 为 <c>true</c> 时按显示名称（DisplayName）匹配；为 <c>false</c> 时先按属性名（PropertyName）匹配，未命中再按显示名称匹配。
        /// </param>
        /// <param name="comparison">字符串比较规则。</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException">未找到匹配属性，或 <paramref name="propertyName"/> 为 <c>null</c>/空字符串。</exception>
        /// <exception cref="InvalidOperationException">目标属性不可写。</exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/> 不能赋值给目标属性类型。</exception>
        public static void SetValue<T>(T instance, string propertyName, object value, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            PropertyAccessor<T>.SetValue(instance, propertyName, value, useDisplayName, comparison);
        }

        /// <summary>
        /// 获取类型的所有属性元数据。
        /// </summary>
        /// <typeparam name="T">对象类型（仅支持引用类型）。</typeparam>
        /// <returns>属性元数据列表。</returns>
        /// <remarks>
        /// 返回值为缓存的只读列表；列表顺序为反射返回顺序，未定义且不应依赖其顺序进行业务逻辑处理。
        /// </remarks>
        public static IReadOnlyList<PropertyMetadata> GetProperties<T>()
            where T : class
        {
            return PropertyAccessor<T>.Properties;
        }
    }
}
