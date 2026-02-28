#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1200
#pragma warning disable CS1591
#pragma warning disable SA1309
using System.Collections.Generic;
using System.Linq;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// Aggregates keywords from multiple <see cref="ICableKeywordProvider"/> sources.
    /// </summary>
    public class CompositeCableKeywordProvider : ICableKeywordProvider
    {
        private readonly IEnumerable<ICableKeywordProvider> _providers;

        public CompositeCableKeywordProvider(IEnumerable<ICableKeywordProvider> providers)
        {
            _providers = providers ?? Enumerable.Empty<ICableKeywordProvider>();
        }

        public CompositeCableKeywordProvider(params ICableKeywordProvider[] providers)
        {
            _providers = providers ?? Enumerable.Empty<ICableKeywordProvider>();
        }

        public IEnumerable<string> GetPowerKeywords()
        {
            return _providers.SelectMany(p => p.GetPowerKeywords()).Distinct();
        }

        public IEnumerable<string> GetControlKeywords()
        {
            return _providers.SelectMany(p => p.GetControlKeywords()).Distinct();
        }
    }
}
