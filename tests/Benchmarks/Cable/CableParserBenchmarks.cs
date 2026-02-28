using BenchmarkDotNet.Attributes;
using LbxyCommonLib.Cable;

namespace LbxyCommonLib.Benchmarks.Cable
{
    [MemoryDiagnoser]
    public class CableParserBenchmarks
    {
        private CableParser _parser;
        private const string Model = "BPYJV";

        [GlobalSetup]
        public void Setup()
        {
            _parser = new CableParser();
        }

        [Benchmark]
        public CableSpec Parse_Standard() => _parser.Parse("YJV", "3x50+1x25");

        [Benchmark]
        public CableSpec Parse_VariableFrequency() => _parser.Parse("BPYJV", "3x50+3x25");

        [Benchmark]
        public CableSpec Parse_TwistedPair() => _parser.Parse("DJYPV", "6x2x1.5");

        [Benchmark]
        public CableSpec Parse_MultiBundle_VF() => _parser.Parse("BPYJV", "2(3x35+3x6)");

        [Benchmark]
        public CableSpec Parse_MixedSymbols() => _parser.Parse("BPYJV", "3*50+3×25");
        
        // Simulating the user's specific request "10万次循环"
        // This measures the total time to run 100,000 parses.
        [Benchmark]
        public void Parse_Loop_100k()
        {
            for (int i = 0; i < 100_000; i++)
            {
                _parser.Parse(Model, "3x50+3x25");
            }
        }
    }
}
