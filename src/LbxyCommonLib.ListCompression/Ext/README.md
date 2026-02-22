# String Extensions (Ext.StringExtensions)

功能清单
- 比较：EqualsOrdinal、EqualsCulture、CompareNatural（数字段自然排序）
- 分割：SplitBy、SplitLines（支持 \r/\n/\r\n）、SplitCsv（兼容双引号转义）
- 合并：JoinWith、ConcatWith、MergeLines（自动去除空行）
- 前缀/后缀：EnsurePrefix、EnsureSuffix、RemovePrefix、RemoveSuffix、HasPrefixIgnoreCase、HasSuffixIgnoreCase
- 替换：ReplaceIgnoreCase、ReplaceFirst、ReplaceLast、ReplaceRegex

技术规范
- 以静态类 Ext.StringExtensions.StringExt 提供零侵入扩展
- 所有方法标注 [MethodImpl(MethodImplOptions.AggressiveInlining)]
- 对 this string 为 null 返回默认值（string.Empty / false / -1）
- 无第三方依赖，仅使用 System 与 Regex
- 提供完整 XML 注释与示例

兼容性
- 目标框架：.NET Framework 4.5、.NET Standard 2.0、.NET 6.0
- StyleCop 已处理规则，编译无阻塞

使用示例
```csharp
using Ext.StringExtensions;

var ok = "Hello".EqualsOrdinal("hello", ignoreCase: true);
var parts = "a--b--c".SplitBy("--");
var csv = "a,\"b,c\"".SplitCsv();
var s = "world".EnsurePrefix("hello ");
var t = "a-b-b".ReplaceFirst("b", "X");
var cmp = "file10".CompareNatural("file2"); // > 0
```

性能对比
- BenchmarkDotNet 用例位于 tests/Benchmarks/StringExtensions.Benchmark.cs
- 对比 CompareNatural 与 String.CompareOrdinal、SplitCsv 与原生分割

注意事项
- ReplaceIgnoreCase 采用 Regex.Escape(oldValue) 保证安全
- SplitLines 对混合换行友好；MergeLines 默认移除空字符串行
