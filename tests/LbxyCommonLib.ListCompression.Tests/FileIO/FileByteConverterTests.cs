namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class FileByteConverterTests
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
        public void WriteAndRead_WorksForNormalData()
        {
            var path = Path.Combine(tempDir, "normal.bin");
            var input = new byte[] { 1, 2, 3, 4, 5 };

            FileByteConverter.Write(path, input, overwrite: true);
            var output = FileByteConverter.Read(path);

            Assert.That(output, Is.EqualTo(input));
        }

        [Test]
        public void WriteAndRead_EmptyArray()
        {
            var path = Path.Combine(tempDir, "empty.bin");
            var input = new byte[0];

            FileByteConverter.Write(path, input, overwrite: true);
            var output = FileByteConverter.Read(path);

            Assert.That(output.Length, Is.EqualTo(0));
        }

        [Test]
        public void Write_NoOverwrite_ThrowsWhenFileExists()
        {
            var path = Path.Combine(tempDir, "exists.bin");
            File.WriteAllBytes(path, new byte[] { 9, 9 });

            Assert.Throws<IOException>(() =>
            {
                FileByteConverter.Write(path, new byte[] { 1 }, overwrite: false);
            });
        }

        [Test]
        public void Read_FileNotFound_Throws()
        {
            var path = Path.Combine(tempDir, "missing.bin");
            Assert.Throws<FileNotFoundException>(() =>
            {
                FileByteConverter.Read(path);
            });
        }

        [Test]
        public void TryWrite_ReturnsFalseOnError()
        {
            var path = tempDir; // directory path will cause UnauthorizedAccessException on CreateNew
            string errorMessage;
            var ok = FileByteConverter.TryWrite(path, new byte[] { 1 }, overwrite: false, out errorMessage);

            Assert.That(ok, Is.False);
            Assert.That(errorMessage, Is.Not.Null);
            Assert.That(errorMessage.Length, Is.GreaterThan(0));
        }

        [Test]
        public void TryRead_ReturnsFalseOnError()
        {
            var path = Path.Combine(tempDir, "missing.bin");
            byte[] data;
            string errorMessage;
            var ok = FileByteConverter.TryRead(path, out data, out errorMessage);

            Assert.That(ok, Is.False);
            Assert.That(errorMessage, Is.Not.Null);
            Assert.That(errorMessage.Length, Is.GreaterThan(0));
            Assert.That(data.Length, Is.EqualTo(0));
        }
    }
}
