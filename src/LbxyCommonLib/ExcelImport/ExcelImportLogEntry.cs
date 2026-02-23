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

    public sealed class ExcelImportLogEntry
    {
        /// <summary>
        /// 初始化 <see cref="ExcelImportLogEntry"/> 类型的新实例。
        /// </summary>
        /// <param name="rowIndex">相关单元格的行索引（从 0 开始）。</param>
        /// <param name="columnIndex">相关单元格的列索引（从 0 开始）。</param>
        /// <param name="columnName">相关列的名称。</param>
        /// <param name="message">日志消息内容。</param>
        /// <param name="rawValue">导入前单元格的原始文本值。</param>
        public ExcelImportLogEntry(int rowIndex, int columnIndex, string columnName, string message, string rawValue)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnName = columnName;
            Message = message;
            RawValue = rawValue;
        }

        /// <summary>
        /// 获取相关单元格的行索引（从 0 开始）。
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// 获取相关单元格的列索引（从 0 开始）。
        /// </summary>
        public int ColumnIndex { get; }

        /// <summary>
        /// 获取相关列的名称。
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// 获取日志消息内容。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 获取导入前单元格的原始文本值。
        /// </summary>
        public string RawValue { get; }
    }
}
