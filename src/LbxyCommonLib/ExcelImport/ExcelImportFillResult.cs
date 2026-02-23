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
    using System.Collections.Generic;
    using System.Data;

    public sealed class ExcelImportFillResult
    {
        public ExcelImportFillResult(DataTable table, IReadOnlyList<ExcelImportLogEntry> logs)
        {
            Table = table;
            Logs = logs;
        }

        public DataTable Table { get; }

        public IReadOnlyList<ExcelImportLogEntry> Logs { get; }
    }
}
