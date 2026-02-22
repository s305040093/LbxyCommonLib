using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Ext.StringExtensions;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class StringExtensionsBenchmarks
    {
        private readonly string sample = "file10";
        private readonly string sample2 = "file2";
        private readonly string csv = "a,\"b,c\",\"d\"\"e\"";
        private readonly List<string> lines;

        public StringExtensionsBenchmarks()
        {
            lines = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                lines.Add("line" + i);
            }
        }

        [Benchmark]
        public int CompareNatural_Ext() => sample.CompareNatural(sample2);

        [Benchmark]
        public int CompareNatural_Native() => string.CompareOrdinal(sample, sample2);

        [Benchmark]
        public string[] SplitCsv_Ext() => csv.SplitCsv();

        [Benchmark]
        public string SplitJoin_Ext() => lines.JoinWith(",");
    }
}
