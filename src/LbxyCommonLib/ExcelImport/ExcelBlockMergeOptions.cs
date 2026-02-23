#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1518
#pragma warning disable SA1201
#pragma warning disable SA1602

namespace LbxyCommonLib.ExcelImport
{
    public sealed class ExcelBlockMergeOptions
    {
        public ExcelBlockMergeConflictStrategy ConflictStrategy { get; set; } = ExcelBlockMergeConflictStrategy.Append;

#if NET45
        public string[] DuplicateKeyColumnNames { get; set; }
#else
        public string[]? DuplicateKeyColumnNames { get; set; }
#endif
    }
}
