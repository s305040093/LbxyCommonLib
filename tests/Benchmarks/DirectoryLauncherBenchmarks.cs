using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using LbxyCommonLib.FolderLauncher;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class DirectoryLauncherBenchmarks
    {
        private string tempDir;
        private string missingDir;

        [GlobalSetup]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibBench", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            missingDir = Path.Combine(tempDir, "missing");
        }

        [GlobalCleanup]
        public void Cleanup()
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

        [Benchmark]
        public void Validate_Existing()
        {
            DirectoryLauncher.ValidatePath(tempDir);
        }

        [Benchmark]
        public bool HasReadAccess_Existing()
        {
            return DirectoryLauncher.HasReadAccess(tempDir);
        }

        [Benchmark]
        public bool TryOpen_Missing()
        {
            return DirectoryLauncher.TryOpen(missingDir, out _);
        }
    }
}
