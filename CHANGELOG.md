# 变更日志（Changelog）

本项目遵循「保持简洁、面向使用者」的变更记录风格。  
版本号由 MinVer 根据 Git 标签自动生成。

## [Unreleased]

## [0.3.1] - 2026-02-28

### 变更

- 降级 `Newtonsoft.Json` 依赖版本至 13.0.3，以增强与其他库的兼容性。

## [0.3.0] - 2026-02-28

### 新增

- 新增 `MergeOrReplace` 扩展方法，支持将对象转换为属性字典后，与外部字典进行合并或替换。
- 新增 `LbxyCommonLib.Collections` 命名空间及 `DictionaryOperations` 模块。
- 提供 `Merge` 方法，支持字典的浅层合并与深度合并（递归合并嵌套字典）。
- 提供 `Replace` 方法，支持仅替换目标字典中已存在键的值。
- 支持自定义合并冲突策略：`Overwrite`（覆盖）、`KeepTarget`（保留）、`Throw`（报错）。

### 移除

- 移除 `ToPropertyDictionaryWithOverride` 扩展方法。请使用 `MergeOrReplace` 代替。
- 移除 `OverrideStrategy` 枚举。请使用 `LbxyCommonLib.Collections.DictionaryConflictStrategy` 代替。

## [0.2.2] - 2026-02-28

### 变更

- `ClassExtensions.ToPropertyDictionary` 的 `useDisplayName` 参数默认值由 `false` 更改为 `true`。默认情况下将使用属性显示名称作为字典键。

### 新增

- `ClassExtensions.ToPropertyDictionary` 增加 `useDisplayName` 可选参数，允许使用属性显示名称作为字典键。

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
