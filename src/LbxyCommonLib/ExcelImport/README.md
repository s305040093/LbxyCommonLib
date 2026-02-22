# Excel Import Module

目标
- 从 .xlsx/.xls/.xlsm 三种 Excel 文件读取指定工作表内容到 DataTable
- 表头模式：有表头/无表头
- 列映射：连续区域与分散字典映射（索引/列字母）
- 负数格式：支持 "-123.45" 与 "(123.45)"，可配置关闭括号识别或自定义正则
- 异常：统一 ExcelImportException，包含行号、列号、值快照

API
- ExcelImporter.ReadToDataTable(path, settings) / ReadToDataTableAsync(path, settings, ct)
- ExcelImporter.ReadToDataTable(stream, settings) / ReadToDataTableAsync(stream, settings, ct)
- ExcelImporter.ImportExcel(path, settings) / ImportExcel(stream, settings) → object[][]（可直接序列化为 JSON 数组）
- ExcelImportSettings：SheetName/SheetIndex、HasHeader、HeaderRenameMapByIndex/Name、StartColumnIndex/ColumnCount、DispersedMapByIndex/Letter、EnableBracketNegative、BracketAsNumeric、CustomNegativeRegex

配置与热重载
- 建议通过代码注入强类型设置（满足现有 DI 体系）
- 后续可拓展为从 app.config/appsettings.json 读取并通过 FileSystemWatcher 3 秒内热重载

性能
- 基于 ExcelProcessing 模块统一识别格式（扩展名 + 文件头），内部使用 NPOI 针对不同格式选择 HSSF/XSSF 解析器
- 支持从文件路径和 Stream 两种模式读取；对 10MB 以内单文件，设计目标为解析耗时 ≤ 500ms、额外内存占用不超过文件大小的 3 倍（实际表现取决于运行环境）

共享模式使用说明
- 从文件路径读取时（ReadToDataTable/ImportExcel 等），内部通过 ExcelProcessing.NpoiWorkbook 以 FileAccess.Read + FileShare.ReadWrite 方式打开文件，避免对已由 Excel 打开的工作簿施加独占锁
- 当检测到“文件被另一进程占用”的 IO 异常时，会按“首次尝试 + 最多 3 次重试”的策略重新打开文件，每次重试间隔约 200 ms
- 若所有尝试仍失败，将记录包含完整路径、尝试次数和耗时的 Trace 日志，并抛出 ExcelImportException（ErrorCode=FileLocked），其 ValueSnapshot 中包含 Path、Attempts、ElapsedMs 等诊断信息
- 对调用方而言，通常建议在捕获 ExcelImportException 后，根据 ErrorCode 区分 FileNotFound、UnsupportedFormat、FileLocked 等场景，并在 FileLocked 分支中提示用户关闭占用该文件的程序或稍后重试

支持的文件格式
- .xlsx
  - 基于 Open XML 格式，使用 XSSF 解析
  - 文件大小建议 ≤ 10MB
  - 不包含宏内容，仅读取单元格数据与公式结果
- .xls
  - 传统二进制格式，使用 HSSF 解析
  - 文件大小建议 ≤ 10MB
  - 不解析宏内容，仅读取单元格数据
- .xlsm
  - 启用宏的 Open XML 格式，使用 XSSF 解析单元格
  - 通过 ExcelProcessing 的 GetVbaProjectBytes 可额外访问宏二进制，但 ExcelImporter 仅处理数据区域

示例
```csharp
var settings = new ExcelImportSettings { HasHeader = true, SheetName = "Sheet1" };
var importer = new ExcelImporter();
var table = importer.ReadToDataTable("data.xlsx", settings);
```
