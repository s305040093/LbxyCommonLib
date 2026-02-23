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
    public sealed class ExcelBlockMergeStatistics
    {
        public int TotalBlocks { get; set; }

        public int SuccessfulBlocks { get; set; }

        public long MergeElapsedMilliseconds { get; set; }

        public int DuplicateRowCount { get; set; }

        public int TypeConversionFailureCount { get; set; }
    }
}
