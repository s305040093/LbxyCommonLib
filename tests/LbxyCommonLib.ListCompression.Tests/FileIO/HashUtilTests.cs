namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class HashUtilTests
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
        public void ComputeAndVerify_Sha256_File()
        {
            var path = Path.Combine(tempDir, "hash.bin");
            var data = new byte[] { 10, 20, 30, 40, 50 };
            File.WriteAllBytes(path, data);

            var hash = HashUtil.ComputeFileHash(path, "SHA256");
            var ok = HashUtil.VerifyFileHash(path, hash, "SHA256");

            Assert.That(ok, Is.True);
            Assert.That(hash.Length, Is.EqualTo(32)); // SHA256 length
        }

        [Test]
        public void ComputeHash_Unsupported_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                HashUtil.ComputeHash(new byte[] { 1, 2 }, "SHA1024");
            });
        }
    }
}

