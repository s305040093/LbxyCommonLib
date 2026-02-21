using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LbxyCommonLib.ListCompression;

namespace Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<ListCompressionBenchmarks>();
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
}
