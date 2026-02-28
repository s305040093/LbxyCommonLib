#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1615
#pragma warning disable SA1200
#pragma warning disable CS1591
using System.Collections.Generic;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// Provides keywords for identifying cable categories.
    /// </summary>
    public interface ICableKeywordProvider
    {
        /// <summary>
        /// Gets the keywords for power cables.
        /// </summary>
        IEnumerable<string> GetPowerKeywords();

        /// <summary>
        /// Gets the keywords for control cables.
        /// </summary>
        IEnumerable<string> GetControlKeywords();
    }
}
