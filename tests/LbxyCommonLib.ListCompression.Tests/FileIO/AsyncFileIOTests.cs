namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class AsyncFileIOTests
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
        public async Task WriteReadAsync_Works()
        {
            var path = Path.Combine(tempDir, "async.bin");
            var input = new byte[100_000];
            new Random(5678).NextBytes(input);

            await AsyncFileIO.WriteAsync(path, input, overwrite: true, CancellationToken.None);
            var output = await AsyncFileIO.ReadAsync(path, CancellationToken.None);

            Assert.That(output, Is.EqualTo(input));
        }

        [Test]
        public void WriteAsync_ThrowsOnInvalidPath()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await AsyncFileIO.WriteAsync(string.Empty, new byte[] { 1 }, overwrite: true, CancellationToken.None);
            });
        }
    }
}

