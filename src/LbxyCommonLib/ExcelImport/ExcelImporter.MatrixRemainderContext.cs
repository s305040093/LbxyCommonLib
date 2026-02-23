#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1518
#pragma warning disable SA1201
#pragma warning disable SA1602
#pragma warning disable SA1601

namespace LbxyCommonLib.ExcelImport
{
    public sealed partial class ExcelImporter
    {
        public sealed class MatrixRemainderContext
        {
            public int TotalRowCount { get; set; }

            public int TotalColumnCount { get; set; }

            public int BlockRowSize { get; set; }

            public int BlockColumnSize { get; set; }

            public int RowRemainder { get; set; }

            public int ColumnRemainder { get; set; }

            public MatrixRemainderMode Mode { get; set; }

#if NET45
            public MatrixExportOptions Options { get; set; }
#else
            public MatrixExportOptions? Options { get; set; }
#endif
        }
    }
}
