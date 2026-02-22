namespace LbxyCommonLib.FileFinder.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LbxyCommonLib.FileFinder;
    using NUnit.Framework;

    [TestFixture]
    public sealed class FileNameLauncherTests
    {
        private string root;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_FileFinder", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "readme.txt"), "hello");
            File.WriteAllText(Path.Combine(root, "Report_2025_Q1.docx"), "docx");
            File.WriteAllText(Path.Combine(root, "budget-2025.xlsx"), "xlsx");
            File.WriteAllText(Path.Combine(root, "manual.pdf"), "pdf");
            File.WriteAllText(Path.Combine(root, "report-2025-overview.txt"), "txt");
            var sub = Path.Combine(root, "sub");
            Directory.CreateDirectory(sub);
            File.WriteAllText(Path.Combine(sub, "report-summary.txt"), "summary");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
            catch
            {
            }
        }

        [Test]
        public void Search_Exact_Match()
        {
            var hits = FileNameLauncher.SearchFiles(root, "readme.txt", MatchMode.Exact, null, 10);
            Assert.That(hits.Count, Is.EqualTo(1));
            Assert.That(Path.GetFileName(hits[0]), Is.EqualTo("readme.txt"));
        }

        [Test]
        public void Search_Fuzzy_Tokens()
        {
            var hits = FileNameLauncher.SearchFiles(root, "report 2025", MatchMode.Fuzzy, null, 10);
            Assert.That(hits.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task Search_Async_Cancellation()
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                var hits = await FileNameLauncher.SearchFilesAsync(root, "report", MatchMode.Fuzzy, null, 10, cts.Token);
                Assert.That(hits, Is.Not.Null);
                Assert.That(hits.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void TryOpen_Unsupported_Extension()
        {
            var path = Path.Combine(root, "image.bmp");
            File.WriteAllText(path, "bmp");
            var ok = FileNameLauncher.TryOpenByName(root, "image.bmp", MatchMode.Exact, null, out var error, false);
            Assert.That(ok, Is.False);
            Assert.That(error, Is.EqualTo("未找到匹配的文件。"));
        }

        [Test]
        public void TryOpen_ValidateOnly_Success()
        {
            var ok = FileNameLauncher.TryOpenByName(root, "manual.pdf", MatchMode.Exact, null, out var error, false);
            Assert.That(ok, Is.True);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void Preview_Info()
        {
            var path = Path.Combine(root, "readme.txt");
            var info = FileNameLauncher.GetPreview(path, out var error);
            Assert.That(info, Is.Not.Null);
            Assert.That(info.Name, Is.EqualTo("readme.txt"));
            Assert.That(info.SizeBytes, Is.GreaterThan(0));
            Assert.That(error, Is.Empty);
        }
    }
}
