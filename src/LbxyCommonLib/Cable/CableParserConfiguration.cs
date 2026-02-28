#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1200
#pragma warning disable CS1591
namespace LbxyCommonLib.Cable
{
    using System.Collections.Generic;

    /// <summary>
    /// Configuration options for CableParser keyword sources.
    /// </summary>
    public class CableParserConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable built-in default keywords.
        /// Default is true.
        /// </summary>
        public bool EnableBuiltInKeywords { get; set; } = true;

        /// <summary>
        /// Gets the list of JSON file paths to load keywords from.
        /// </summary>
        public List<string> JsonFileSources { get; } = new List<string>();

        // Future extension points:
        // public string DatabaseConnectionString { get; set; }
        // public string ApiUrl { get; set; }
    }
}
