#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1602

namespace LbxyCommonLib.ExcelImport
{
    using System;

    public enum ExcelImportErrorCode
    {
        Unknown,
        FileNotFound,
        UnsupportedFormat,
        ParseFailed,
        SheetNotFound,
        EmptyFile,
        PasswordProtected,
    }

    public sealed class ExcelImportException : Exception
    {
        public ExcelImportException(string message, Exception inner, int rowIndex, int columnIndex, string valueSnapshot)
            : base(message, inner)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ValueSnapshot = valueSnapshot ?? string.Empty;
            ErrorCode = ExcelImportErrorCode.Unknown;
        }

        public ExcelImportException(string message, Exception inner, int rowIndex, int columnIndex, string valueSnapshot, ExcelImportErrorCode errorCode)
            : base(message, inner)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ValueSnapshot = valueSnapshot ?? string.Empty;
            ErrorCode = errorCode;
        }

        public int RowIndex { get; }

        public int ColumnIndex { get; }

        public string ValueSnapshot { get; }

        public ExcelImportErrorCode ErrorCode { get; }
    }
}

#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1649
#pragma warning restore SA1600
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore CS1591
