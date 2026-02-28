using System.Collections.Generic;
using System.IO;
using LbxyCommonLib.Cable;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Cable
{
    [TestFixture]
    public class CableParserKeywordTests
    {
        [Test]
        public void DefaultConstructor_UsesDefaultKeywords()
        {
            var parser = new CableParser();
            var spec = parser.Parse("YJV", "3x10");
            Assert.That(spec.Category, Is.EqualTo("动力电缆"));
            
            var specControl = parser.Parse("KVV", "3x10");
            Assert.That(specControl.Category, Is.EqualTo("控制电缆"));
        }

        [Test]
        public void CustomProvider_OverridesKeywords()
        {
            var customProvider = new CustomKeywordProvider(
                new[] { "TEST-POWER" },
                new[] { "TEST-CONTROL" }
            );

            var parser = new CableParser(customProvider);
            
            // Should match custom
            var specPower = parser.Parse("TEST-POWER", "3x10");
            Assert.That(specPower.Category, Is.EqualTo("动力电缆"));

            var specControl = parser.Parse("TEST-CONTROL", "3x10");
            Assert.That(specControl.Category, Is.EqualTo("控制电缆"));

            // Should NOT match default if not included (YJV is not in customProvider)
            var specDefault = parser.Parse("YJV", "3x10");
            Assert.That(specDefault.Category, Is.Null);
        }

        [Test]
        public void CompositeProvider_CombinesKeywords()
        {
            var defaultProvider = new DefaultCableKeywordProvider();
            var customProvider = new CustomKeywordProvider(
                new[] { "EXTRA-POWER" },
                new[] { "EXTRA-CONTROL" }
            );

            var composite = new CompositeCableKeywordProvider(defaultProvider, customProvider);
            var parser = new CableParser(composite);

            // Match default
            Assert.That(parser.Parse("YJV", "3x10").Category, Is.EqualTo("动力电缆"));
            // Match custom
            Assert.That(parser.Parse("EXTRA-POWER", "3x10").Category, Is.EqualTo("动力电缆"));
        }

        [Test]
        public void JsonFileProvider_LoadsKeywords()
        {
            string tempFile = Path.GetTempFileName();
            string json = @"{ ""PowerKeywords"": [""JSON-POWER""], ""ControlKeywords"": [""JSON-CONTROL""] }";
            File.WriteAllText(tempFile, json);

            try
            {
                var provider = new JsonFileKeywordProvider(tempFile);
                var parser = new CableParser(provider);

                Assert.That(parser.Parse("JSON-POWER", "3x10").Category, Is.EqualTo("动力电缆"));
                Assert.That(parser.Parse("JSON-CONTROL", "3x10").Category, Is.EqualTo("控制电缆"));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
        
        [Test]
        public void JsonFileProvider_HandlesMissingFile_Gracefully()
        {
            var provider = new JsonFileKeywordProvider("non_existent_file.json");
            var parser = new CableParser(provider);
            
            // Should not throw, but return empty keywords
            var spec = parser.Parse("YJV", "3x10");
            Assert.That(spec.Category, Is.Null);
        }

        private class CustomKeywordProvider : ICableKeywordProvider
        {
            private readonly IEnumerable<string> _power;
            private readonly IEnumerable<string> _control;

            public CustomKeywordProvider(IEnumerable<string> power, IEnumerable<string> control)
            {
                _power = power;
                _control = control;
            }

            public IEnumerable<string> GetPowerKeywords() => _power;
            public IEnumerable<string> GetControlKeywords() => _control;
        }
    }
}
