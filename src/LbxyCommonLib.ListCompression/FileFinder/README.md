# File Finder: FileNameLauncher

功能
- 基于文件名的搜索与打开：支持 Exact 与 Fuzzy 两种模式
- 支持常见格式：.txt、.doc/.docx、.pdf、.xls/.xlsx、.csv、.md（可扩展）
- 预览信息（可选）：文件大小、修改日期、类型
- 异常与权限处理：返回明确提示，不抛异常

API
- SearchFiles(root, query, mode, allowedExtensions = null, maxResults = 0)
- SearchFilesAsync(root, query, mode, allowedExtensions = null, maxResults, ct)
- TryOpenByName(root, query, mode, allowedExtensions = null, out errorMessage, actuallyOpen = true)
- GetPreview(path, out errorMessage)

性能说明
- 使用 Directory.EnumerateFiles 进行流式遍历，避免一次性载入
- Fuzzy 模式采用低成本 Token 包含匹配；对 10 万文件目录具备线性伸缩性
- 基准用例位于 tests/Benchmarks/FileNameLauncherBenchmarks.cs，可用于度量搜索延迟

使用示例
```csharp
using LbxyCommonLib.FileFinder;
var hits = FileNameLauncher.SearchFiles(root, "report 2025", MatchMode.Fuzzy, null, 100);
var ok = FileNameLauncher.TryOpenByName(root, "budget-2025.xlsx", MatchMode.Exact, null, out var error);
var info = FileNameLauncher.GetPreview(hits[0], out var previewError);
```

注意事项
- Windows 上通过 Shell 打开文件，依赖系统默认关联的应用
- 测试场景可将 actuallyOpen = false，避免启动外部程序
