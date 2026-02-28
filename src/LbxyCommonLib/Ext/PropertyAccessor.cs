#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 提供统一的属性访问与元数据获取功能。
    /// 支持显示名称获取（XafDisplayName > DisplayName > Display）、高性能属性读写。
    /// </summary>
    /// <example>
    /// <code>
    /// // 1. 默认模式：使用属性名称 (PropertyName)
    /// var value = PropertyAccessor.GetValue(model, "Name");
    /// PropertyAccessor.SetValue(model, "Name", "NewValue");
    ///
    /// // 2. 显示名称模式：使用显示名称 (DisplayName)
    /// // 需设置 useDisplayName: true
    /// var value = PropertyAccessor.GetValue(model, "姓名", useDisplayName: true);
    /// PropertyAccessor.SetValue(model, "姓名", "NewValue", useDisplayName: true);
    /// </code>
    /// </example>
    public static class PropertyAccessor
    {
        /// <summary>
        /// 获取指定属性的显示名称。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>显示名称，若未找到则返回属性名称。</returns>
        public static string GetDisplayName<T>(string propertyName)
            where T : class
        {
            return PropertyAccessor<T>.GetDisplayName(propertyName, useDisplayName: true);
        }

        /// <summary>
        /// 获取指定属性的显示名称。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        /// <returns>显示名称，若未找到则返回属性名称。</returns>
        public static string GetDisplayName<T>(string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return PropertyAccessor<T>.GetDisplayName(propertyName, useDisplayName, comparison);
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>属性值。</returns>
        public static object GetValue<T>(T instance, string propertyName)
            where T : class
        {
            return PropertyAccessor<T>.GetValue(instance, propertyName, useDisplayName: true);
        }

        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称或显示名称。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        /// <returns>属性值。</returns>
        public static object GetValue<T>(T instance, string propertyName, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            return PropertyAccessor<T>.GetValue(instance, propertyName, useDisplayName, comparison);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="value">要设置的值。</param>
        public static void SetValue<T>(T instance, string propertyName, object value)
            where T : class
        {
            PropertyAccessor<T>.SetValue(instance, propertyName, value, useDisplayName: false);
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">属性名称或显示名称。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="useDisplayName">是否强制使用显示名称匹配（默认 true）。</param>
        /// <param name="comparison">字符串比较规则（默认 OrdinalIgnoreCase）。</param>
        public static void SetValue<T>(T instance, string propertyName, object value, bool useDisplayName = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            where T : class
        {
            PropertyAccessor<T>.SetValue(instance, propertyName, value, useDisplayName, comparison);
        }

        /// <summary>
        /// 获取类型的所有属性元数据。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>属性元数据列表。</returns>
        public static IReadOnlyList<PropertyMetadata> GetProperties<T>()
            where T : class
        {
            return PropertyAccessor<T>.Properties;
        }
    }
}
