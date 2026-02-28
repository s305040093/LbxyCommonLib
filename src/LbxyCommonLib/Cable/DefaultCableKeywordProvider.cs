#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1200
#pragma warning disable CS1591
using System.Collections.Generic;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// Default implementation of <see cref="ICableKeywordProvider"/> using built-in keywords.
    /// </summary>
    public class DefaultCableKeywordProvider : ICableKeywordProvider
    {
        private static readonly string[] PowerKeywords = { "YJV", "VV", "YJLV", "VLV", "YJV22", "VV22", "YJLV22", "VLV22", "电力", "ZR-YJV", "NH-YJV", "WDZ-YJV" };
        private static readonly string[] ControlKeywords = { "KVV", "KVVP", "KYJV", "KYJVP", "KVV22", "KVVP2", "控制", "ZR-KVV", "NH-KVV" };

        public IEnumerable<string> GetPowerKeywords()
        {
            return PowerKeywords;
        }

        public IEnumerable<string> GetControlKeywords()
        {
            return ControlKeywords;
        }
    }
}
