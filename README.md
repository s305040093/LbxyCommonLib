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
using LbxyCommonLib.Numerics;

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

// 数值相等性辅助：相对/绝对误差控制与链式调用
double price = 100.000000001;
double baseline = 100.0;
bool roughlyEqual = price
    .EqualsRelatively(baseline, relativeError: 1e-8)
    && price.EqualsAbsolutely(baseline, absoluteError: 1e-10);
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

## ExcelImporter 高级配置

### 自定义标题行与数据起始行（ImportAdvanced）

在某些复杂报表场景下，标题行与数据行之间可能存在若干说明行或占位行，此时可以通过 `ImportAdvanced` 接口结合 `ExcelImportSettings` 精确控制：

- `HeaderRowIndex`：标题行索引（从 0 开始），例如业务上“第 3 行标题”应设置为 `2`。
- `DataRowIndex`：数据起始行索引（从 0 开始），例如业务上“第 4 行数据起始行”应设置为 `3`。
- `HeaderReadMode`：表头读取模式，常见为 `HeaderStartIndex`（从指定列起读取连续表头）或 `HeaderByName`（按列名匹配）。
- 高级模式始终视为“存在表头”，调用时会强制 `HasHeader = true`。

约束条件：

- 必须满足 `HeaderRowIndex < DataRowIndex`，否则将抛出 `ArgumentException`。
- `HeaderRowIndex` 与 `DataRowIndex` 不能超出工作表实际行数，否则同样抛出 `ArgumentException`。

典型用法（从文件路径导入到预定义 DataTable）：

```csharp
using System;
using System.Data;
using LbxyCommonLib.ExcelImport;

var settings = new ExcelImportSettings
{
    HasHeader = true,
    HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
    HeaderStartColumnIndex = 0,
    HeaderRowIndex = 0, // 第 1 行为表头
    DataRowIndex = 1,   // 第 2 行开始为数据
};

var table = new DataTable("Orders");
table.Columns.Add("Name", typeof(string));
table.Columns.Add("Amount", typeof(decimal));
table.Columns.Add("Note", typeof(string));

var importer = new ExcelImporter();
var result = importer.ImportAdvanced("orders.xlsx", settings, table);
```

如果需要从流读取（例如来自 HTTP 上传或内存流），可以使用流重载：

```csharp
using (var stream = File.OpenRead("orders.xlsx"))
{
    var settings = new ExcelImportSettings
    {
        HasHeader = true,
        HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
        HeaderRowIndex = 2, // 业务上第 3 行
        DataRowIndex = 3,   // 业务上第 4 行
    };

    var table = new DataTable("Orders");
    table.Columns.Add("CustomerName", typeof(string));
    table.Columns.Add("Amount", typeof(decimal));

    var importer = new ExcelImporter();
    var result = importer.ImportAdvanced(stream, settings, table);
}
```

#### 常见场景示例

1. **标题前有说明行**

   结构示意：

   | 行号 | 内容         |
   | ---- | ------------ |
   | 1    | 报表标题     |
   | 2    | 生成时间说明 |
   | 3    | Name,Amount  |
   | 4    | Alice,10     |
   | 5    | Bob,20       |

   配置：

   ```csharp
   var settings = new ExcelImportSettings
   {
       HasHeader = true,
       HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
       HeaderStartColumnIndex = 0,
       HeaderRowIndex = 2, // 业务上第 3 行
       DataRowIndex = 3,   // 业务上第 4 行
   };
   ```

2. **标题与目标列名不一致，通过 HeaderByName 与别名映射**

   Excel 表头：`Name,Amount,Note`  
   目标 DataTable：`CustomerName,Amount`

   ```csharp
   var settings = new ExcelImportSettings
   {
       HasHeader = true,
       HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
       HeaderRowIndex = 0,
       DataRowIndex = 1,
   };

   // Name ↔ CustomerName 作为别名
   settings.HeaderRenameMapByName["Name"] = "CustomerName";

   var table = new DataTable("AdvancedByNameAlias");
   table.Columns.Add("CustomerName", typeof(string));
   table.Columns.Add("Amount", typeof(decimal));

   var importer = new ExcelImporter();
   var result = importer.ImportAdvanced("orders.xlsx", settings, table);
   ```

3. **只关注部分离散列**

   如果业务只关心某几个列，可以通过 `HeaderIndexList` 模式精确指定：

   ```csharp
   var settings = new ExcelImportSettings
   {
       HasHeader = true,
       HeaderReadMode = ExcelHeaderReadMode.HeaderIndexList,
       HeaderRowIndex = 0,
       DataRowIndex = 1,
   };

   // 只读取第 2、3 列（0 基索引 1、2）
   settings.HeaderIndexList.Add(1);
   settings.HeaderIndexList.Add(2);
   ```

#### 与 FillPredefinedDataTable 的关系与迁移建议

- `FillPredefinedDataTable`：适合标题在第一行、数据紧随其后且结构相对固定的场景。
- `ImportAdvanced`：在其基础上增加了对标题行与数据起始行的显式控制，更适合：
  - 上方有说明行或空行的 Excel 报表；
  - 一个工作簿中包含多个区块，仅需导入某一块数据；
  - 需要通过 `HeaderByName`/别名映射，解耦列顺序与列名的场景。

如果你已有使用 `FillPredefinedDataTable` 的代码，并希望迁移到高级模式，推荐步骤：

1. 保持现有 `DataTable` 结构不变。
2. 根据实际 Excel 结构在 `ExcelImportSettings` 中设置：
   - `HeaderRowIndex`（0 基）
   - `DataRowIndex`（0 基）
   - `HeaderReadMode` 以及必要的 `HeaderStartColumnIndex` / `HeaderIndexList` / `HeaderRenameMapByName`。
3. 将调用从：

   ```csharp
   var result = importer.FillPredefinedDataTable(path, settings, table);
   ```

   替换为：

   ```csharp
   var result = importer.ImportAdvanced(path, settings, table);
   ```

在默认情况下（`HeaderRowIndex = 0`, `DataRowIndex = 1`），两者的行为是一致的。

性能与兼容性说明：

- `ImportAdvanced` 在内部复用与 `FillPredefinedDataTable` 相同的数据读取与类型转换流水线，整体复杂度为 O(行数 × 列数)。
- 对于 10 万行以内的数据集，在常规桌面/服务器配置（如 i5 10 代、16 GB 内存）下，内存占用与耗时与原有导入接口等同，不会引入额外的性能损失。
- 支持 `.xls` 与 `.xlsx` 格式，并通过多目标编译支持 .NET Framework 4.5 与 netstandard2.0，从而可运行于 .NET 6 / .NET 8 等现代运行时。

### acceptNumericAsDate

`ExcelImportSettings.AcceptNumericAsDate` 用于控制“看起来像日期的纯数字”是否按日期解析：

- 默认值为 `false`，此时 20260101 等纯数字会被当作普通数字处理。
- 当设置为 `true` 且目标列类型为 `DateTime` 时，形如 `yyyyMMdd` 的 8 位纯数字会被解析为对应日期，例如 `20260101` 解析为 `2026-01-01`。
- 每次发生此类识别时，都会在 `ExcelImportFillResult.Logs` 中追加一条带有 i18n 键的提示消息，键为 `ExcelImporter.NumericAsDateWarning`。

风险提示：

- 启用 `acceptNumericAsDate` 后，类似 `20260101` 这样的业务编号有可能被误判为日期。
- 建议仅在目标列确实是日期列、且源数据约定使用 `yyyyMMdd` 纯数字格式时开启该选项。

示例代码：

```csharp
using System;
using System.Data;
using LbxyCommonLib.ExcelImport;

var settings = new ExcelImportSettings
{
    HasHeader = true,
    HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
    HeaderStartColumnIndex = 0,
    AcceptNumericAsDate = true,
};

var table = new DataTable("NumericDate");
table.Columns.Add("DateValue", typeof(DateTime));

var importer = new ExcelImporter();
var result = importer.FillPredefinedDataTable("path-to-file.xlsx", settings, table);
```

### ImportExcel 矩阵导出与分块导出

在需要将 Excel 数据导出为 `object[][]` 矩阵并进行更精细控制时，可以使用 `ImportExcel` 的高级重载与 `MatrixExportOptions`：

- `StartRowIndex` / `StartColumnIndex`：控制导出区域的起始行/列（0 基索引）。
- `RowCount` / `ColumnCount`：限制导出区域的最大行数与列数，为 null 或 ≤0 时表示“直到表尾”。
- `BlockRowCount` / `BlockColumnCount`：将区域拆分为多个固定大小的块；为 null 或 ≤0 时表示“不分块”。
- `BlockTraversalOrder`：
  - `TopDownLeftRight`：块按“先块行后块列”（上→下，再左→右）的顺序遍历。
  - `LeftRightTopDown`：块按“先块列后块行”（左→右，再上→下）的顺序遍历。

常用方法：

- `ImportExcel(path, settings)`：整张表导出为一个矩阵。
- `ImportExcel(path, settings, options)`：按给定区域导出为一个矩阵。
- `ImportExcelBlocks(path, settings, options)`：按给定区域和块配置导出为多个矩阵块。

示例代码：

```csharp
using System;
using System.Data;
using LbxyCommonLib.ExcelImport;

var settings = new ExcelImportSettings
{
    HasHeader = true,
};

var importer = new ExcelImporter();

// 1. 整张表导出为矩阵
object[][] allRows = importer.ImportExcel("orders.xlsx", settings);

// 2. 仅导出从第 101 行开始的 1000 行、前 5 列
var regionOptions = new ExcelImporter.MatrixExportOptions
{
    StartRowIndex = 100,   // 业务上的第 101 行（0 基索引）
    StartColumnIndex = 0,
    RowCount = 1000,
    ColumnCount = 5,
};

object[][] region = importer.ImportExcel("orders.xlsx", settings, regionOptions);

// 3. 在上述区域基础上按块导出，每块 200 行 × 2 列，块顺序为“先上后下，再左后右”
var blockOptions = new ExcelImporter.MatrixExportOptions
{
    StartRowIndex = 100,
    StartColumnIndex = 0,
    RowCount = 1000,
    ColumnCount = 5,
    BlockRowCount = 200,
    BlockColumnCount = 2,
    BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.TopDownLeftRight,
};

var blocks = importer.ImportExcelBlocks("orders.xlsx", settings, blockOptions);

// blocks[i] 即第 i 个块，每个块本身是一个 object[][] 子矩阵
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
