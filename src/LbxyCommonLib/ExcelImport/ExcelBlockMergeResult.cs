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
    using System;
    using System.Collections.Generic;
    using System.Data;

    public sealed class ExcelBlockMergeResult
    {
        public ExcelBlockMergeResult(DataTable table, IReadOnlyList<ExcelImportLogEntry> logs, ExcelBlockMergeStatistics statistics)
        {
            Table = table;
            Logs = logs;

            if (statistics == null)
            {
                statistics = new ExcelBlockMergeStatistics();
            }

            if (logs != null)
            {
                var count = 0;
                for (var i = 0; i < logs.Count; i++)
                {
                    var message = logs[i].Message;
                    if (!string.IsNullOrEmpty(message) && message.StartsWith("数据类型转换失败:", StringComparison.Ordinal))
                    {
                        count++;
                    }
                }

                statistics.TypeConversionFailureCount = count;
            }

            Statistics = statistics;
        }

        public DataTable Table { get; }

        public IReadOnlyList<ExcelImportLogEntry> Logs { get; }

        public ExcelBlockMergeStatistics Statistics { get; }
    }
}
