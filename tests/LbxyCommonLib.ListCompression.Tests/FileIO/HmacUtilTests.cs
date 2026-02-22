namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class HmacUtilTests
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
        public void ComputeAndVerify_HmacSha256_File()
        {
            var path = Path.Combine(tempDir, "hmac.bin");
            var data = new byte[] { 1, 2, 3, 4 };
            File.WriteAllBytes(path, data);

            var key = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2 };
            var hmac = HmacUtil.ComputeFileHmac(path, key, "HMACSHA256");
            var ok = HmacUtil.VerifyFileHmac(path, hmac, key, "HMACSHA256");

            Assert.That(ok, Is.True);
        }
    }
}

