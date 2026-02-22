using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LbxyCommonLib.ListCompression;
using LbxyCommonLib.Numerics;
using LbxyCommonLib.ExcelImport;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.IO;

namespace Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[] { typeof(ListCompressionBenchmarks), typeof(NumericalEqualityBenchmarks), typeof(DirectoryLauncherBenchmarks), typeof(StringExtensionsBenchmarks), typeof(FileNameLauncherBenchmarks), typeof(ExcelImporterBenchmarks) });
            switcher.Run(args);
        }
    }

    public class ListCompressionBenchmarks
    {
        private IReadOnlyList<Item> adjacentInput;
        private IReadOnlyList<Item> mixedInput;
        private CompressionRule<Item> adjacentRule;
        private CompressionRule<Item> globalRule;

        [GlobalSetup]
        public void Setup()
        {
            var rnd = new Random(42);
            var list1 = new List<Item>(10000);
            for (int i = 0; i < 10000; i++)
            {
                list1.Add(new Item("A", rnd.Next(1, 5)));
                list1.Add(new Item("A", rnd.Next(1, 5)));
                list1.Add(new Item("B", rnd.Next(1, 5)));
            }
            adjacentInput = list1.AsReadOnly();

            var list2 = new List<Item>(10000);
            for (int i = 0; i < 10000; i++)
            {
                var key = (i % 50).ToString();
                list2.Add(new Item(key, rnd.Next(1, 5)));
            }
            mixedInput = list2.AsReadOnly();
            adjacentRule = new CompressionRule<Item> { AdjacentOnly = true };
            globalRule = new CompressionRule<Item>();
        }

        [Benchmark]
        public List<Item> GlobalCompressionOnAdjacentData() => ListCompressor<Item>.Compress(adjacentInput);

        [Benchmark]
        public List<Item> AdjacentCompressionOnAdjacentData() => ListCompressor<Item>.Compress(adjacentInput, adjacentRule);

        [Benchmark]
        public List<Item> GlobalCompressionOnMixedData() => ListCompressor<Item>.Compress(mixedInput, globalRule);

        [Benchmark]
        public List<Item> AdjacentCompressionOnMixedData() => ListCompressor<Item>.Compress(mixedInput, adjacentRule);
    }

    public sealed class Item : LbxyCommonLib.ListCompression.Interfaces.ISummable<Item>, IEquatable<Item>
    {
        public Item(string key, int count)
        {
            Key = key;
            Count = count;
        }

        public string Key { get; }
        public int Count { get; }

        public bool Equals(Item other) => other != null && string.Equals(Key, other.Key, StringComparison.Ordinal);
        public override bool Equals(object obj) => Equals(obj as Item);
        public override int GetHashCode() => Key == null ? 0 : Key.GetHashCode();

        public double GetSummableValue() => Count;
        public Item WithUpdatedSummableValue(double newValue) => new Item(Key, (int)newValue);
    }

    public class NumericalEqualityBenchmarks
    {
        private readonly double[] doubleValues1 = new double[100000];
        private readonly double[] doubleValues2 = new double[100000];

        [GlobalSetup]
        public void Setup()
        {
            var rnd = new Random(123);
            for (int i = 0; i < doubleValues1.Length; i++)
            {
                var v = rnd.NextDouble();
                doubleValues1[i] = v;
                doubleValues2[i] = v + (rnd.NextDouble() - 0.5d) * 1e-10d;
            }
        }

        [Benchmark]
        public int NativeDoubleComparison()
        {
            var count = 0;
            for (int i = 0; i < doubleValues1.Length; i++)
            {
                if (doubleValues1[i] == doubleValues2[i])
                {
                    count++;
                }
            }

            return count;
        }

        [Benchmark]
        public int UtilDoubleComparison()
        {
            var count = 0;
            for (int i = 0; i < doubleValues1.Length; i++)
            {
                if (NumericalEqualityUtil.AreEqual(doubleValues1[i], doubleValues2[i]))
                {
                    count++;
                }
            }

            return count;
        }
    }

    [MemoryDiagnoser]
    public class ExcelImporterBenchmarks
    {
        private string path;
        private ExcelImporter importer;
        private ExcelImportSettings settings;

        [GlobalSetup]
        public void Setup()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibBench_ExcelImporter");
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Data");

                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Col1");
                header.CreateCell(1).SetCellValue("Col2");
                header.CreateCell(2).SetCellValue("Col3");
                header.CreateCell(3).SetCellValue("Col4");
                header.CreateCell(4).SetCellValue("Col5");

                for (int i = 1; i <= 50000; i++)
                {
                    var row = sheet.CreateRow(i);
                    row.CreateCell(0).SetCellValue("Row" + i.ToString());
                    row.CreateCell(1).SetCellValue(i);
                    row.CreateCell(2).SetCellValue(i * 0.1d);
                    row.CreateCell(3).SetCellValue(DateTime.UtcNow);
                    row.CreateCell(4).SetCellValue(i % 2 == 0);
                }

                wb.Write(fs);
            }

            importer = new ExcelImporter();
            settings = new ExcelImportSettings
            {
                HasHeader = true,
            };
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var dir = Path.GetDirectoryName(path);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch
            {
            }
        }

        [Benchmark]
        public DataTable ReadLargeXlsx()
        {
            return importer.ReadToDataTable(path, settings);
        }
    }
}
