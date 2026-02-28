using System.Collections.Generic;
using LbxyCommonLib.Cable;
using NUnit.Framework;

namespace LbxyCommonLib.ListCompression.Tests.Cable
{
    [TestFixture]
    public class CableParserFactoryTests
    {
        [Test]
        public void Create_NullConfig_UsesDefault()
        {
            var parser = CableParserFactory.Create(null);
            var result = parser.Parse("YJV", "3x10");
            Assert.That(result.Category, Is.EqualTo("动力电缆"));
        }

        [Test]
        public void Create_DisableBuiltIn_IgnoresDefaultKeywords()
        {
            var config = new CableParserConfiguration { EnableBuiltInKeywords = false };
            var parser = CableParserFactory.Create(config);
            var result = parser.Parse("YJV", "3x10");
            // Should be null/empty because no keywords match "YJV" if built-in are disabled
            // Wait, Parse logic might still parse spec, but category will be null/empty if keywords don't match.
            // If category is null, it's not set.
            Assert.That(result.Category, Is.Null.Or.Empty);
        }

        [Test]
        public void Create_JsonSource_LoadsKeywords()
        {
            var jsonPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "factory_test.json");
            System.IO.File.WriteAllText(jsonPath, "{\"PowerKeywords\":[\"TEST-POWER\"],\"ControlKeywords\":[\"TEST-CONTROL\"]}");

            try
            {
                var config = new CableParserConfiguration { EnableBuiltInKeywords = false };
                config.JsonFileSources.Add(jsonPath);

                var parser = CableParserFactory.Create(config);
                var result = parser.Parse("TEST-POWER", "3x10");
                Assert.That(result.Category, Is.EqualTo("动力电缆"));

                var result2 = parser.Parse("TEST-CONTROL", "3x10");
                Assert.That(result2.Category, Is.EqualTo("控制电缆"));
            }
            finally
            {
                if (System.IO.File.Exists(jsonPath)) System.IO.File.Delete(jsonPath);
            }
        }
    }
}
