# 性能基准

运行：

```bash
dotnet run --project tests/Benchmarks -c Release
```

输出示例（不同环境会有差异）：

```
| Method              | Mean      | Error    | StdDev   |
|-------------------- |----------:|---------:|---------:|
| AdjacentCompression |  1.23 ms  | 0.05 ms  | 0.04 ms  |
| GlobalCompression   |  2.87 ms  | 0.09 ms  | 0.08 ms  |
```

## PropertyAccessor

运行：

```bash
dotnet run --project tests/Benchmarks -c Release -- --filter *PropertyAccessor*
```

本机一次运行结果（.NET 8, Core2 Duo T7700）：

```
| Method                                | Mean        | Ratio | Allocated |
|-------------------------------------- |------------:|------:|----------:|
| GetDisplayName_Reflection             | 1,047.07 ns |  1.00 |     248 B |
| GetDisplayName_PropertyAccessor       |    34.11 ns |  0.03 |         - |
| GetValue_Reflection                   |    14.27 ns |  0.01 |         - |
| GetValue_PropertyAccessor             |    43.04 ns |  0.04 |         - |
| ToPropertyDictionary_PropertyAccessor |   343.38 ns |  0.33 |     320 B |
| ToPropertyDictionary_Reflection       |   451.66 ns |  0.43 |     376 B |
```

完整报告：
- [Benchmarks.Ext.PropertyAccessorBenchmarks-report-github.md](file:///c:/Users/netc/Documents/trae_projects/LbxyCommonLib/BenchmarkDotNet.Artifacts/results/Benchmarks.Ext.PropertyAccessorBenchmarks-report-github.md)
- [Benchmarks.Ext.PropertyAccessorBenchmarks-report.html](file:///c:/Users/netc/Documents/trae_projects/LbxyCommonLib/BenchmarkDotNet.Artifacts/results/Benchmarks.Ext.PropertyAccessorBenchmarks-report.html)
