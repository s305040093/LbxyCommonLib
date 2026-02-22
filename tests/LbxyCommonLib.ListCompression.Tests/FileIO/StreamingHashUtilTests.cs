namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class StreamingHashUtilTests
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
        public void StreamedHash_Equals_WholeFileHash()
        {
            var path = Path.Combine(tempDir, "streamed.bin");
            var data = new byte[200_000];
            new Random(42).NextBytes(data);
            File.WriteAllBytes(path, data);

            var h1 = HashUtil.ComputeFileHash(path, "SHA256");
            var h2 = StreamingHashUtil.ComputeFileHashStreamed(path, "SHA256", 4096);

            Assert.That(HashUtil.ToBase64(h1), Is.EqualTo(HashUtil.ToBase64(h2)));
        }
    }
}

