using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using LbxyCommonLib.Ext;

namespace Benchmarks.Ext
{
    [MemoryDiagnoser]
    public class ClassExtensionsBenchmarks
    {
        private SimpleClass _obj;
        private Dictionary<string, string> _target;

        [GlobalSetup]
        public void Setup()
        {
            _obj = new SimpleClass
            {
                Name = "Benchmark",
                Value = 12345,
                Date = DateTime.Now
            };
            _target = new Dictionary<string, string>();

            // Warmup cache
            _obj.ToPropertyDictionary();
        }

        [Benchmark(OperationsPerInvoke = 1000)]
        public void Map_1000_Times()
        {
            for (int i = 0; i < 1000; i++)
            {
                _obj.ToPropertyDictionary();
            }
        }

        private class SimpleClass
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
