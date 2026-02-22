namespace LbxyCommonLib.FolderLauncher.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.FolderLauncher;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DirectoryLauncherTests
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
        public void ValidatePath_Throws_OnNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => DirectoryLauncher.ValidatePath(null));
            Assert.Throws<ArgumentException>(() => DirectoryLauncher.ValidatePath(string.Empty));
        }

        [Test]
        public void ValidatePath_Throws_OnRelative()
        {
            Assert.Throws<ArgumentException>(() => DirectoryLauncher.ValidatePath("relative\\path"));
        }

        [Test]
        public void ValidatePath_Throws_OnMissingDirectory()
        {
            var missing = Path.Combine(tempDir, "not-exist");
            Assert.Throws<DirectoryNotFoundException>(() => DirectoryLauncher.ValidatePath(missing));
        }

        [Test]
        public void ValidatePath_Succeeds_OnExistingAbsoluteDirectory()
        {
            DirectoryLauncher.ValidatePath(tempDir);
        }

        [Test]
        public void HasReadAccess_ReturnsTrue_OnTempDirectory()
        {
            var canRead = DirectoryLauncher.HasReadAccess(tempDir);
            Assert.That(canRead, Is.True);
        }

        [Test]
        public void TryOpen_ReturnsFalse_OnInvalidPath()
        {
            var ok = DirectoryLauncher.TryOpen("Z:\\this\\path\\is\\unlikely\\valid", out var msg);
            Assert.That(ok, Is.False);
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg.Length, Is.GreaterThan(0));
        }
    }
}

