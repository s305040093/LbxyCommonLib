namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ChunkManifestTests
    {
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch
            {
            }
        }

        [Test]
        public void BuildAndVerifyManifest_Succeeds()
        {
            var path = Path.Combine(tempDir, "manifest.bin");
            var data = new byte[128 * 1024];
            new Random(7).NextBytes(data);
            File.WriteAllBytes(path, data);

            var manifest = ChunkManifestUtil.BuildFileManifest(path, 8192, "SHA256");
            var ok = ChunkManifestUtil.VerifyFileAgainstManifest(path, manifest);
            Assert.That(ok, Is.True);
        }
    }
}

