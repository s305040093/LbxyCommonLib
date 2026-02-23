namespace LbxyCommonLib.ExcelImport
{
    using System;

    /// <summary>
    /// 提供 Excel 列名称（例如 "A"、"Z"、"AA"、"XFD"）与 1 基列索引之间的相互转换工具。
    /// 该转换器遵循现代 Excel 的列范围限制：最小为 1（对应列 "A"），最大为 16384（对应列 "XFD"）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 列索引采用 1 基计数，以便与 Excel UI 中的列号保持一致：
    /// 1 → "A"，26 → "Z"，27 → "AA"，16384 → "XFD"。
    /// </para>
    /// <para>
    /// 所有转换方法都会对输入参数进行严格校验，对于空引用、空字符串、包含非字母字符的名称或超出 Excel 支持范围的索引，
    /// 会抛出相应的 <see cref="ArgumentNullException"/>、<see cref="ArgumentException"/> 或 <see cref="ArgumentOutOfRangeException"/>。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 1 基索引转列名
    /// var name1 = ExcelColumnConverter.ColumnIndexToName(1);      // "A"
    /// var name2 = ExcelColumnConverter.ColumnIndexToName(27);     // "AA"
    /// var name3 = ExcelColumnConverter.ColumnIndexToName(16384);  // "XFD"
    ///
    /// // 列名转 1 基索引（不区分大小写，会自动 Trim）
    /// var index1 = ExcelColumnConverter.ColumnNameToIndex("A");      // 1
    /// var index2 = ExcelColumnConverter.ColumnNameToIndex("aa");     // 27
    /// var index3 = ExcelColumnConverter.ColumnNameToIndex(" xfd ");  // 16384
    /// </code>
    /// </example>
    public static class ExcelColumnConverter
    {
        /// <summary>
        /// 表示支持的最小列索引（1 基），对应 Excel 列 "A"。
        /// </summary>
        public const int MinColumnIndex = 1;

        /// <summary>
        /// 表示支持的最大列索引（1 基），对应 Excel 列 "XFD"。
        /// </summary>
        public const int MaxColumnIndex = 16384;

        /// <summary>
        /// 将 1 基列索引转换为对应的 Excel 列名称（例如 1 → "A"、27 → "AA"、16384 → "XFD"）。
        /// </summary>
        /// <param name="columnIndex">1 基列索引，必须在 <see cref="MinColumnIndex"/> 与 <see cref="MaxColumnIndex"/> 之间。</param>
        /// <returns>对应的 Excel 列名称，由 1 至 3 个大写字母组成。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="columnIndex"/> 小于 <see cref="MinColumnIndex"/> 或大于 <see cref="MaxColumnIndex"/> 时抛出。
        /// </exception>
        public static string ColumnIndexToName(int columnIndex)
        {
            if (columnIndex < MinColumnIndex || columnIndex > MaxColumnIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be between 1 and 16384.");
            }

            var index = columnIndex;
            var chars = new char[3];
            var pos = chars.Length;
            while (index > 0 && pos > 0)
            {
                index--;
                var remainder = index % 26;
                chars[--pos] = (char)('A' + remainder);
                index /= 26;
            }

            return new string(chars, pos, chars.Length - pos);
        }

        /// <summary>
        /// 将 Excel 列名称转换为对应的 1 基列索引（例如 "A" → 1、"AA" → 27、"XFD" → 16384）。
        /// </summary>
        /// <param name="columnName">
        /// Excel 列名称，长度为 1 至 3 个字符，支持大小写字母，前后空白会被自动忽略。
        /// 例如 "A"、"Z"、"AA"、"AZ"、"BA"、"ZZ"、"AAA"、"XFD"。
        /// </param>
        /// <returns>对应的 1 基列索引，范围为 <see cref="MinColumnIndex"/> 至 <see cref="MaxColumnIndex"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="columnName"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="columnName"/> 为空字符串、仅包含空白字符，或包含非字母字符（A-Z/a-z 之外）时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="columnName"/> 长度大于 3，或对应的列索引超出 <see cref="MaxColumnIndex"/> 时抛出。
        /// </exception>
        public static int ColumnNameToIndex(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            var trimmed = columnName.Trim();
            if (trimmed.Length == 0)
            {
                throw new ArgumentException("Column name cannot be empty.", nameof(columnName));
            }

            if (trimmed.Length > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), "Column name length cannot exceed 3 characters.");
            }

            var upper = trimmed.ToUpperInvariant();
            var result = 0;
            for (var i = 0; i < upper.Length; i++)
            {
                var c = upper[i];
                if (c < 'A' || c > 'Z')
                {
                    throw new ArgumentException("Column name must contain only letters A-Z.", nameof(columnName));
                }

                var value = (c - 'A') + 1;
                result = (result * 26) + value;
            }

            if (result < MinColumnIndex || result > MaxColumnIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), "Column name is out of supported Excel range.");
            }

            return result;
        }
    }
}
