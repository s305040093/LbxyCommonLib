using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using LbxyCommonLib.FileFinder;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class FileNameLauncherBenchmarks
    {
        private string root;

        [GlobalSetup]
        public void Setup()
        {
            root = Path.Combine(Path.GetTempPath(), "LbxyCommonLibBench_FileFinder", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            for (int i = 0; i < 5000; i++)
            {
                var name = i % 4 == 0 ? $"report_{i}.txt" : i % 4 == 1 ? $"Report_{i}.docx" : i % 4 == 2 ? $"budget_{i}.xlsx" : $"manual_{i}.pdf";
                File.WriteAllText(Path.Combine(root, name), "x");
            }
        }

        [GlobalCleanup]
        public void Cleanup()
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

        [Benchmark]
        public object SearchFuzzy_Reports()
        {
            return FileNameLauncher.SearchFiles(root, "report", MatchMode.Fuzzy, null, 100);
        }

        [Benchmark]
        public object SearchExact_One()
        {
            return FileNameLauncher.SearchFiles(root, "report_1234.txt", MatchMode.Exact, null, 1);
        }
    }
}
