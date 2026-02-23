#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1602
#pragma warning disable SA1629
#pragma warning disable SA1642

namespace LbxyCommonLib.ExcelImport
{
    using System;

    /// <summary>
    /// Excel 导入错误代码，用于对导入异常进行分类，便于上层应用进行差异化处理。
    /// </summary>
    public enum ExcelImportErrorCode
    {
        /// <summary>
        /// 未知错误类型，表示未能归类的异常情况。
        /// </summary>
        Unknown,

        /// <summary>
        /// 文件未找到，例如路径不存在或无法访问。
        /// </summary>
        FileNotFound,

        /// <summary>
        /// 不受支持的 Excel 文件格式。
        /// </summary>
        UnsupportedFormat,

        /// <summary>
        /// Excel 内容解析失败，例如 XML 结构损坏或单元格值不符合预期格式。
        /// </summary>
        ParseFailed,

        /// <summary>
        /// 指定的工作表不存在或未能找到。
        /// </summary>
        SheetNotFound,

        /// <summary>
        /// 文件为空或不包含任何数据行。
        /// </summary>
        EmptyFile,

        /// <summary>
        /// Excel 文件受到密码保护，无法在当前上下文中解密读取。
        /// </summary>
        PasswordProtected,

        /// <summary>
        /// Excel 文件被其他进程独占锁定，无法在共享读取模式下成功打开。
        /// </summary>
        FileLocked,

        /// <summary>
        /// 指定的分块行列数无法整除数据区域的总行列数。
        /// </summary>
        BlockRemainderNotDivisible,
    }

    /// <summary>
    /// 在 Excel 导入过程中发生错误时抛出的异常类型，携带行列索引、错误快照值及错误代码等上下文信息。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class ExcelImportException : Exception
    {
        /// <summary>
        /// 使用指定的消息、内部异常、行列索引及原始值快照初始化 <see cref="ExcelImportException"/> 类的新实例。
        /// </summary>
        /// <param name="message">描述错误原因的消息。</param>
        /// <param name="inner">导致当前异常的内部异常实例。</param>
        /// <param name="rowIndex">发生错误的 Excel 行索引（从 0 开始）；未知时为 -1。</param>
        /// <param name="columnIndex">发生错误的 Excel 列索引（从 0 开始）；未知时为 -1。</param>
        /// <param name="valueSnapshot">发生错误时单元格的原始文本快照。</param>
        public ExcelImportException(string message, Exception inner, int rowIndex, int columnIndex, string valueSnapshot)
            : base(message, inner)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ValueSnapshot = valueSnapshot ?? string.Empty;
            ErrorCode = ExcelImportErrorCode.Unknown;
        }

        /// <summary>
        /// 使用指定的消息、内部异常、行列索引、原始值快照及错误代码初始化 <see cref="ExcelImportException"/> 类的新实例。
        /// </summary>
        /// <param name="message">描述错误原因的消息。</param>
        /// <param name="inner">导致当前异常的内部异常实例。</param>
        /// <param name="rowIndex">发生错误的 Excel 行索引（从 0 开始）；未知时为 -1。</param>
        /// <param name="columnIndex">发生错误的 Excel 列索引（从 0 开始）；未知时为 -1。</param>
        /// <param name="valueSnapshot">发生错误时单元格的原始文本快照。</param>
        /// <param name="errorCode">导入错误代码，用于指示错误类别。</param>
        public ExcelImportException(string message, Exception inner, int rowIndex, int columnIndex, string valueSnapshot, ExcelImportErrorCode errorCode)
            : base(message, inner)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ValueSnapshot = valueSnapshot ?? string.Empty;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 获取发生错误的 Excel 行索引（从 0 开始）；为 -1 时表示未知。
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// 获取发生错误的 Excel 列索引（从 0 开始）；为 -1 时表示未知。
        /// </summary>
        public int ColumnIndex { get; }

        /// <summary>
        /// 获取发生错误时单元格的原始文本快照。
        /// </summary>
        public string ValueSnapshot { get; }

        /// <summary>
        /// 获取导入错误代码，用于对异常进行分类。
        /// </summary>
        public ExcelImportErrorCode ErrorCode { get; }
    }
}

#pragma warning restore SA1642
#pragma warning restore SA1629

#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1649
#pragma warning restore SA1600
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore CS1591
