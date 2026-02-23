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
        public sealed class MatrixExportOptions
        {
            public int StartRowIndex { get; set; }

            public int StartColumnIndex { get; set; }

            public int? RowCount { get; set; }

            public int? ColumnCount { get; set; }

            public int? BlockRowCount { get; set; }

            public int? BlockColumnCount { get; set; }

            public MatrixRemainderMode? RemainderMode { get; set; }

            public MatrixBlockTraversalOrder BlockTraversalOrder { get; set; } = MatrixBlockTraversalOrder.TopDownLeftRight;

            public long? MaxEstimatedBytesPerBlock { get; set; }
        }
    }
}
