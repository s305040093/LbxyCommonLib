namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ResumeFileIOTests
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
        public void Append_And_ReadRange()
        {
            var path = Path.Combine(tempDir, "resume.bin");
            ResumeFileIO.Append(path, new byte[] { 1, 2, 3 }, createIfMissing: true);
            ResumeFileIO.Append(path, new byte[] { 4, 5 }, createIfMissing: true);

            var part = ResumeFileIO.ReadRange(path, 2, 3);
            Assert.That(part, Is.EqualTo(new byte[] { 3, 4, 5 }));
        }

        [Test]
        public void WriteAtOffset_CreatesGap()
        {
            var path = Path.Combine(tempDir, "gap.bin");
            ResumeFileIO.WriteAtOffset(path, new byte[] { 9 }, 10, createIfMissing: true);
            var bytes = File.ReadAllBytes(path);
            Assert.That(bytes.Length, Is.EqualTo(11));
            Assert.That(bytes[10], Is.EqualTo(9));
        }
    }
}

