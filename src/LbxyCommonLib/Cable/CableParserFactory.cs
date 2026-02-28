#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1200
#pragma warning disable SA1512
#pragma warning disable SA1028
#pragma warning disable CS1591
namespace LbxyCommonLib.Cable
{
    using System.Collections.Generic;

    /// <summary>
    /// Factory for creating configured CableParser instances.
    /// </summary>
    public static class CableParserFactory
    {
        /// <summary>
        /// Creates a new CableParser with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <returns>A configured CableParser instance.</returns>
        public static CableParser Create(CableParserConfiguration config)
        {
            if (config == null)
            {
                config = new CableParserConfiguration();
            }

            var providers = new List<ICableKeywordProvider>();

            if (config.EnableBuiltInKeywords)
            {
                providers.Add(new DefaultCableKeywordProvider());
            }

            foreach (var path in config.JsonFileSources)
            {
                providers.Add(new JsonFileKeywordProvider(path));
            }

            // If no providers are configured, we should probably warn or add empty one.
            // But CompositeCableKeywordProvider handles empty list gracefully (returns empty keywords).

            var composite = new CompositeCableKeywordProvider(providers);
            return new CableParser(composite);
        }
    }
}
