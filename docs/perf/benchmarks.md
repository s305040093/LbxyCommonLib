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
