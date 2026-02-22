namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ChunkedFileIOTests
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
        public void WriteAndReadChunks_LargeData()
        {
            var path = Path.Combine(tempDir, "large.bin");
            var total = 1024 * 1024; // 1MB
            var chunkSize = 64 * 1024;
            var rnd = new Random(1234);
            var data = new byte[total];
            rnd.NextBytes(data);

            var chunks = new List<ArraySegment<byte>>();
            for (var offset = 0; offset < data.Length; offset += chunkSize)
            {
                var size = Math.Min(chunkSize, data.Length - offset);
                chunks.Add(new ArraySegment<byte>(data, offset, size));
            }

            ChunkedFileIO.WriteChunks(path, chunks, overwrite: true);

            var readBack = new List<byte>();
            foreach (var chunk in ChunkedFileIO.ReadChunks(path, chunkSize))
            {
                readBack.AddRange(chunk);
            }

            Assert.That(readBack.ToArray(), Is.EqualTo(data));
        }
    }
}

