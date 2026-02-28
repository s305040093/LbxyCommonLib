# 变更日志（Changelog）

本项目遵循「保持简洁、面向使用者」的变更记录风格。  
版本号由 MinVer 根据 Git 标签自动生成。

## [Unreleased]

## [0.2.1] - 2026-02-28

### 新增

- PropertyAccessor 增加 `useDisplayName` 参数，支持强制使用显示名称进行属性查找与读写。
- PropertyAccessor 增加 `comparison` 参数，支持自定义属性名匹配的大小写规则。
- 优化属性查找性能：针对 `Ordinal` 和 `OrdinalIgnoreCase` 使用字典查找。
- 增加属性查找失败时的详细异常信息（列出可用显示名称）。

### 修复

- 修复 `PropertyAccessor.GetDisplayName` 在 `useDisplayName: false` 时错误返回显示名称的问题。

## [0.1.0] - 2026-02-22

Commit: 299ee1d7ab93e041959ad394c4638295ec14f417

### 新增

- 建立跨版本兼容的帮助类库结构：支持 .NET Framework 4.5 与 .NET（通过 netstandard2.0）。
- 实现高性能列表压缩模块：
  - 支持相邻压缩与全局压缩两种模式。
  - 支持基于 `Equals` 的比较、可选自定义规则与自动累加。
  - 提供同步与异步 API。
- 提供单元测试与 BenchmarkDotNet 基准测试。
- 增强 ExcelImporter：支持共享模式读文件与文件锁重试逻辑。
- 为 ExcelImportSettings、ExcelImportException、FileIO、StringProcessing 等核心 API 补全文档注释。
- 完善 NuGet 打包元数据：补充描述、标签、符号包与 README 显示。

### 构建与发布

- 引入 MinVer 进行语义化版本号管理（基于 Git 标签）。
- 配置基础 CI（编译 + 测试）与打包流程雏形。

> 未来版本发布时，请在合并 PR 时同步更新本文件，并在 `PackageReleaseNotes` 中引用相应条目。
