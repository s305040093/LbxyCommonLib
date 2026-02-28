#pragma warning disable SA1101
#pragma warning disable SA1600
#pragma warning disable SA1200
#pragma warning disable CS1591
#pragma warning disable SA1309
#pragma warning disable SA1214
#pragma warning disable SA1516
#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LbxyCommonLib.Cable
{
    /// <summary>
    /// Loads cable keywords from a JSON file.
    /// </summary>
    public class JsonFileKeywordProvider : ICableKeywordProvider
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private KeywordData _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFileKeywordProvider"/> class.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        public JsonFileKeywordProvider(string filePath)
        {
            _filePath = filePath;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetPowerKeywords()
        {
            LoadData();
            return _data?.PowerKeywords ?? new List<string>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetControlKeywords()
        {
            LoadData();
            return _data?.ControlKeywords ?? new List<string>();
        }

        private void LoadData()
        {
            if (_data != null)
            {
                return;
            }

            lock (_lock)
            {
                if (_data != null)
                {
                    return;
                }

                if (!File.Exists(_filePath))
                {
                    _data = new KeywordData();
                    return;
                }

                try
                {
                    string json = File.ReadAllText(_filePath);
                    _data = JsonConvert.DeserializeObject<KeywordData>(json) ?? new KeywordData();
                }
                catch
                {
                    // If parsing fails, return empty data to avoid crashing the application.
                    // In a real application, you might want to log this error.
                    _data = new KeywordData();
                }
            }
        }

        private class KeywordData
        {
            public List<string> PowerKeywords { get; set; } = new List<string>();
            public List<string> ControlKeywords { get; set; } = new List<string>();
        }
    }
}
