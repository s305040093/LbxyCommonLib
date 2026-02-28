#pragma warning disable SA1600
#pragma warning disable SA1128
#pragma warning disable SA1200
#pragma warning disable CS1591
using System;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// 当电缆型号同时包含动力电缆与控制电缆特征关键词时抛出此异常。
    /// </summary>
    public class AmbiguousCategoryException : Exception
    {
        public AmbiguousCategoryException(string message) : base(message)
        {
        }

        public AmbiguousCategoryException(string message, string originalModel)
            : base($"{message} (Original Model: {originalModel})")
        {
        }
    }
}
