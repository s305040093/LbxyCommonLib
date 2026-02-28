#pragma warning disable SA1600
#pragma warning disable SA1128
#pragma warning disable SA1028
#pragma warning disable SA1200
#pragma warning disable CS1591
using System;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// 当截面数值无法解析为合法数字时抛出此异常。
    /// </summary>
    public class InvalidSectionException : Exception
    {
        public InvalidSectionException(string message) : base(message)
        {
        }

        public InvalidSectionException(string message, string invalidSegment)
            : base($"{message} (Invalid Segment: {invalidSegment})")
        {
        }
    }
}
