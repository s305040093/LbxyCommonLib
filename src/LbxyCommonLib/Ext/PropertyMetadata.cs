#if !NET45
#nullable disable
#endif

namespace LbxyCommonLib.Ext
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// 属性元数据，包含名称、显示名称及读写委托。
    /// </summary>
    public class PropertyMetadata
    {
        /// <summary>
        /// 初始化 <see cref="PropertyMetadata"/> 类的新实例。
        /// </summary>
        /// <param name="info">属性信息。</param>
        public PropertyMetadata(PropertyInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.Name = info.Name;
            this.PropertyType = info.PropertyType;
            this.CanRead = info.CanRead;
            this.CanWrite = info.CanWrite;
            this.DisplayName = ResolveDisplayName(info);

            if (this.CanRead)
            {
                this.Getter = BuildGetter(info);
                this.StringGetter = BuildStringGetter(info);
            }

            if (this.CanWrite)
            {
                this.Setter = BuildSetter(info);
            }
        }

        /// <summary>
        /// 获取属性名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取显示名称。
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 获取属性类型。
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// 获取一个值，该值指示属性是否可读。
        /// </summary>
        public bool CanRead { get; }

        /// <summary>
        /// 获取一个值，该值指示属性是否可写。
        /// </summary>
        public bool CanWrite { get; }

        /// <summary>
        /// 获取属性读取委托。
        /// </summary>
        public Func<object, object> Getter { get; }

        /// <summary>
        /// 获取属性设置委托。
        /// </summary>
        public Action<object, object> Setter { get; }

        /// <summary>
        /// 获取属性字符串读取委托（已优化）。
        /// </summary>
        public Func<object, string> StringGetter { get; }

        private static string ResolveDisplayName(PropertyInfo p)
        {
            string displayName = p.Name;
            var attrs = p.GetCustomAttributes(false);

            // 优先级: XafDisplayName -> DisplayName -> Display
            string xafName = GetAttributeValue(attrs, "XafDisplayNameAttribute", "DisplayName");
            string displayNameAttr = GetAttributeValue(attrs, "DisplayNameAttribute", "DisplayName");
            string displayAttr = GetAttributeValue(attrs, "DisplayAttribute", "Name");

            if (!string.IsNullOrEmpty(xafName))
            {
                displayName = xafName;
            }
            else if (!string.IsNullOrEmpty(displayNameAttr))
            {
                displayName = displayNameAttr;
            }
            else if (!string.IsNullOrEmpty(displayAttr))
            {
                displayName = displayAttr;
            }

            return displayName;
        }

        private static string GetAttributeValue(object[] attrs, string attrTypeName, string propertyName)
        {
            foreach (var attr in attrs)
            {
                if (attr == null)
                {
                    continue;
                }

                var type = attr.GetType();
                if (type.Name == attrTypeName || (type.FullName != null && type.FullName.EndsWith(attrTypeName)))
                {
                    try
                    {
                        var prop = type.GetProperty(propertyName);
                        if (prop != null)
                        {
                            return prop.GetValue(attr, null) as string;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            return null;
        }

        private static Func<object, object> BuildGetter(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var instanceCast = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            var propertyAccess = Expression.Property(instanceCast, propertyInfo);
            var castPropertyValue = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castPropertyValue, instanceParam);
            return lambda.Compile();
        }

        private static Func<object, string> BuildStringGetter(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var instanceCast = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            var propertyAccess = Expression.Property(instanceCast, propertyInfo);

            Expression body;
            if (propertyInfo.PropertyType.IsValueType)
            {
                // 值类型（含 Nullable<T>）直接调用 ToString，无需判空
                body = Expression.Call(propertyAccess, "ToString", null);
            }
            else
            {
                // 引用类型需判空
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                var callToString = Expression.Call(propertyAccess, "ToString", null);
                body = Expression.Condition(nullCheck, callToString, Expression.Constant(string.Empty));
            }

            var lambda = Expression.Lambda<Func<object, string>>(body, instanceParam);
            return lambda.Compile();
        }

        private static Action<object, object> BuildSetter(PropertyInfo propertyInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var instanceCast = Expression.Convert(instanceParam, propertyInfo.DeclaringType);
            var valueCast = Expression.Convert(valueParam, propertyInfo.PropertyType);
            var propertyAccess = Expression.Property(instanceCast, propertyInfo);
            var assign = Expression.Assign(propertyAccess, valueCast);
            var lambda = Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam);
            return lambda.Compile();
        }
    }
}
