# Excel 文件处理模块

目标
- 支持对 .xlsx/.xls/.xlsm 三种格式的统一处理
- 自动识别文件格式（扩展名 + 文件头），并选择最优解析器
- 提供统一接口隐藏底层差异，支持工作表列表、单元格读取与公式计算

核心类型
- ExcelFileFormat: Unknown/Xlsx/Xls/Xlsm
- ExcelFileInfo: 封装检测结果 (Path, Format, ExtensionFormat)
- ExcelProcessingException: 模块级异常
- ExcelFileDetector: 负责格式识别与结果缓存
- ExcelWorkbookFactory: 根据检测结果创建 IExcelWorkbook
- IExcelWorkbook/IExcelWorksheet: 统一工作簿/工作表接口
- IExcelWorkbookProvider/ExcelWorkbookProvider: 适配层，隐藏底层 NPOI/xlsx 依赖，支持可插拔实现（默认 XlsxAdapter）

格式识别
- 扩展名:
  - .xlsx → Xlsx
  - .xls → Xls
  - .xlsm → Xlsm
- 文件头 (Magic Number):
  - D0 CF 11 E0 A1 B1 1A E1 → 传统 OLE（二进制），识别为 Xls
  - 50 4B 03 04 → ZIP 容器，识别为 Open XML
- 组合策略:
  - 先用文件头判断大类（OLE/ZIP/Unknown）
  - 若为 ZIP 且扩展名是 .xlsm → Xlsm，否则 → Xlsx
  - 若文件头 Unknown 且扩展名可识别 → 回退为扩展名
  - 无扩展名时完全依赖文件头

解析器选择
- Xls → NPOI HSSF (HSSFWorkbook)
- Xlsx → NPOI XSSF (XSSFWorkbook)
- Xlsm → NPOI XSSF 读取单元格，同时可通过 GetVbaProjectBytes 访问宏二进制

统一接口
- IExcelWorkbook:
  - ExcelFileFormat Format { get; }
  - string Path { get; }
  - IReadOnlyList<string> GetSheetNames();
  - IExcelWorksheet GetWorksheet(int index);
  - IExcelWorksheet GetWorksheet(string name);
  - byte[] GetVbaProjectBytes(); // Xlsm 返回宏二进制，其他为空数组
- IExcelWorksheet:
  - string Name { get; }
  - int RowCount { get; }
  - int ColumnCount { get; }
  - object GetCellValue(int rowIndex, int columnIndex, bool evaluateFormula);

性能与懒加载
- 使用 NPOI 按需访问工作表；仅在显式调用 GetWorksheet 时构造包装对象
- 单元格值按需计算公式 (evaluateFormula=true 时使用 IFormulaEvaluator)
- ExcelFileDetector 使用基于路径+长度+时间戳的缓存，避免重复读取文件头

示例
```csharp
var info = ExcelFileDetector.Detect(path);
using var workbook = ExcelWorkbookFactory.Open(path);
var names = workbook.GetSheetNames();
var sheet = workbook.GetWorksheet(names[0]);
var value = sheet.GetCellValue(0, 0, evaluateFormula: true);
```

适配层与禁止直接使用底层库
- 上层业务（例如 ExcelImporter）只依赖 IExcelWorkbook/IExcelWorksheet 和 IExcelWorkbookProvider，不直接引用 NPOI 或 xlsx 等第三方库
- 默认适配器 XlsxAdapter 通过 ExcelWorkbookProvider.Current 暴露，内部使用 NPOI 实现解析与格式识别
- 任何新接入的解析方案（内存数据源、CSV、其他 Excel 引擎）应实现 IExcelWorkbookProvider 后，通过设置 ExcelWorkbookProvider.Current 进行切换

接入自定义适配器的三步示例
1. 实现 IExcelWorkbookProvider 和对应的 IExcelWorkbook/IExcelWorksheet 包装类型
2. 在应用启动或测试初始化阶段设置 ExcelWorkbookProvider.Current = new CustomWorkbookProvider();
3. 继续使用 ExcelImporter 或 ExcelProcessing API，无需修改调用代码

性能基准
- 单文件大小 ≤10 MB 时，解析耗时目标 <500 ms（普通桌面/服务器环境）
- 单文件大小 ≤10 MB 时，内存峰值目标 <80 MB（包含工作簿对象与中间缓冲区）
