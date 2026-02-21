# LbxyCommonLib

[![CI](https://github.com/lbxy/LbxyCommonLib/actions/workflows/ci.yml/badge.svg)](https://github.com/lbxy/LbxyCommonLib/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/LbxyCommonLib.ListCompression.svg)](https://www.nuget.org/packages/LbxyCommonLib.ListCompression)

跨版本兼容的通用帮助类库，面向 .NET Framework 4.5 与现代 .NET（通过 netstandard2.0）。包含高性能、可扩展的“列表压缩”模块，支持基于 Equals 的比较、默认全局压缩、可定制规则，以及自动累加。

## 安装

### 从 NuGet 安装

```bash
dotnet add package LbxyCommonLib.ListCompression
```

### 从源码构建本地包

```bash
dotnet pack -c Release
```

## 快速开始

```csharp
using System.Collections.Generic;
using LbxyCommonLib.ListCompression;
using LbxyCommonLib.ListCompression.Interfaces;

public sealed class OrderItem : ISummable<OrderItem>
{
    public OrderItem(string sku, double qty) { Sku = sku; Qty = qty; }
    public string Sku { get; }
    public double Qty { get; }
    public double GetSummableValue() => Qty;
    public OrderItem WithUpdatedSummableValue(double v) => new OrderItem(Sku, v);
    public override bool Equals(object o) => o is OrderItem other && Sku == other.Sku;
    public override int GetHashCode() => Sku?.GetHashCode() ?? 0;
}

var input = new List<OrderItem>
{
  new("A", 1.0), new("A", 2.0), new("B", 3.0), new("B", 4.0)
}.AsReadOnly();

// 全局压缩（默认，按键汇总，保留首次出现顺序）
var compressedGlobal = ListCompressor<OrderItem>.Compress(input);

// 相邻压缩（仅压缩相邻元素）
var adjacentRule = new CompressionRule<OrderItem> { AdjacentOnly = true };
var compressedAdjacent = ListCompressor<OrderItem>.Compress(input, adjacentRule);
```

## 自定义压缩规则

```csharp
var rule = new CompressionRule<MyDto>
{
  AreEqual = (a, b) => a.Code == b.Code,
  SumSelector = x => x.Count,
  UpdateSum = (x, sum) => new MyDto(x.Code, (int)sum),
};
var result = ListCompressor<MyDto>.Compress(list, rule);
```

## 设计原则

- O(n) 时间复杂度，O(n) 空间复杂度
- 输入为 IReadOnlyList<T>，返回新 List<T>，不修改原列表
- 同步与异步 API（Task）双模式
- 面向 net45 与 netstandard2.0，使用条件属性保证兼容性

## 测试与基准

- 单元测试位于 `tests/LbxyCommonLib.ListCompression.Tests`
- 基准测试位于 `tests/Benchmarks`，使用 BenchmarkDotNet

运行：

```bash
dotnet test
dotnet run --project tests/Benchmarks
```

## 贡献指南

- 使用最新的 .NET SDK（项目通过 `global.json` 固定了版本）
- 开发前请先运行 `dotnet test` 确认当前分支通过
- 提交代码前确保本地通过 `dotnet build -c Release` 与 `dotnet test -c Release`
- 提交 Pull Request 时：
  - 使用英文或中英文混合的标题简要说明变更
  - 在描述中列出关键修改点、潜在破坏性变更与测试情况
  - 如为新特性，请视情况更新 README 或 CHANGELOG

## 许可协议

MIT，见 LICENSE。
