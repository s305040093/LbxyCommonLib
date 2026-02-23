#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1201
#pragma warning disable SA1602

namespace LbxyCommonLib.ExcelImport
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 表头读取模式，用于控制在存在表头行时如何解析和映射 Excel 列。
    /// </summary>
    public enum ExcelHeaderReadMode
    {
        /// <summary>
        /// 不启用高级表头读取模式，按默认行为处理表头与数据。
        /// </summary>
        None = 0,

        /// <summary>
        /// 按预先指定的表头索引列表读取列（HeaderIndexList），适用于只关心离散列的场景。
        /// </summary>
        HeaderIndexList = 1,

        /// <summary>
        /// 从指定起始列索引（HeaderStartColumnIndex）开始连续读取表头，适用于一段连续业务列。
        /// </summary>
        HeaderStartIndex = 2,

        /// <summary>
        /// 通过表头名称与目标列名进行匹配（可结合 HeaderRenameMapByName 实现别名映射），完全忽略列顺序。
        /// </summary>
        HeaderByName = 3,
    }

    /// <summary>
    /// Excel 导入配置项，控制工作表选择、表头解析、列映射与数值/日期解析等行为。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class ExcelImportSettings
    {
        private bool hasHeader;

        private int headerRowIndex;

        private int dataRowIndex;

        private bool isDataRowIndexExplicit;

        /// <summary>
        /// 要读取的工作表名称，优先于 <see cref="SheetIndex"/>。
        /// 当同时指定 SheetName 和 SheetIndex 时，将优先按名称匹配工作表。
        /// </summary>
        public string SheetName { get; set; } = string.Empty;

        /// <summary>
        /// 要读取的工作表索引（从 0 开始）；当 <see cref="SheetName"/> 为空或未匹配到目标工作表时生效。
        /// 若 SheetName 和 SheetIndex 均未设置，则默认读取当前激活的工作表（对于默认基于 NPOI 的实现而言）。
        /// </summary>
        public int? SheetIndex { get; set; }

        /// <summary>
        /// 指示是否存在表头行，默认值为 true。
        /// 为 true 时使用 <see cref="HeaderRowIndex"/> 作为表头行索引，并在未显式设置 <see cref="DataRowIndex"/> 的情况下自动保持 DataRowIndex = HeaderRowIndex + 1；
        /// 为 false 时忽略 HeaderRowIndex，仅从 DataRowIndex 开始读取数据行。
        /// </summary>
        public bool HasHeader
        {
            get
            {
                return hasHeader;
            }

            set
            {
                hasHeader = value;
                if (hasHeader && !isDataRowIndexExplicit)
                {
                    dataRowIndex = headerRowIndex + 1;
                }
            }
        }

        /// <summary>
        /// 按源列索引重命名表头的映射表，键为源列索引（从 0 开始），值为目标列名。
        /// 适用于列位置固定但表头文本不稳定的场景。
        /// </summary>
        public Dictionary<int, string> HeaderRenameMapByIndex { get; set; }

        /// <summary>
        /// 按原始表头名称重命名表头的映射表，键为源表头名称，值为目标列名，比较时忽略大小写。
        /// 可与 <see cref="ExcelHeaderReadMode.HeaderByName"/> 结合使用，通过配置多个别名映射到统一目标列名。
        /// </summary>
        public Dictionary<string, string> HeaderRenameMapByName { get; set; }

        /// <summary>
        /// 要读取的起始列索引（从 0 开始），为空时从第一列开始。
        /// </summary>
        public int? StartColumnIndex { get; set; }

        public string StartColumnName { get; set; }

        /// <summary>
        /// 要读取的列数；当为 null 时从 <see cref="StartColumnIndex"/> 指定位置一直读取到最后一列。
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// 离散列映射（按列字母，例如 A、B、C），用于将特定列映射到目标名称。
        /// 适用于列位置与数量固定、只关心部分列的场景。
        /// </summary>
        public Dictionary<string, string> DispersedMapByLetter { get; set; }

        /// <summary>
        /// 离散列映射（按列索引），用于将特定列映射到目标名称。
        /// 适用于按位置声明离散业务列的场景，与 <see cref="DispersedMapByLetter"/> 互补。
        /// </summary>
        public Dictionary<int, string> DispersedMapByIndex { get; set; }

        /// <summary>
        /// 表头读取模式，用于高级表头映射场景。
        /// 决定如何根据表头行生成列映射规则。
        /// </summary>
        public ExcelHeaderReadMode HeaderReadMode { get; set; }

        /// <summary>
        /// 当 <see cref="HeaderReadMode"/> 为 <see cref="ExcelHeaderReadMode.HeaderIndexList"/> 时，指定要读取的表头索引列表（从 0 开始）。
        /// </summary>
        public List<int> HeaderIndexList { get; set; }

        /// <summary>
        /// 当 <see cref="HeaderReadMode"/> 为 <see cref="ExcelHeaderReadMode.HeaderStartIndex"/> 时，指定表头起始列索引（从 0 开始）。
        /// </summary>
        public int? HeaderStartColumnIndex { get; set; }

        public string HeaderStartColumnName { get; set; }

        /// <summary>
        /// 指示是否启用括号负数解析，例如将 "(123.45)" 解析为 -123.45，默认值为 true。
        /// </summary>
        public bool EnableBracketNegative { get; set; }

        /// <summary>
        /// 指示括号负数解析时是否按数值处理。
        /// 为 false 时，括号负数可根据 <see cref="BracketNegativeDefaultValue"/> 转换为特定占位值。
        /// </summary>
        public bool BracketAsNumeric { get; set; }

        /// <summary>
        /// 当括号格式负数不按数值解析时使用的默认替换值（例如 0）。
        /// </summary>
        public decimal? BracketNegativeDefaultValue { get; set; }

        /// <summary>
        /// 指示是否接受类似日期格式的纯数字作为日期解析。
        /// 为 true 时，例如 "20240101" 可以被识别为日期并写入 DateTime 列。
        /// </summary>
        public bool AcceptNumericAsDate { get; set; }

        /// <summary>
        /// 数字被识别为日期时，日志记录中使用的国际化键。
        /// </summary>
        public string NumericAsDateI18nKey { get; set; } = "ExcelImporter.NumericAsDateWarning";

        /// <summary>
        /// 自定义负数匹配正则表达式，用于扩展括号负数之外的特殊格式。
        /// </summary>
        public string CustomNegativeRegex { get; set; } = string.Empty;

        /// <summary>
        /// 当表头单元格为空或仅包含空白字符时，用于生成默认列名的前缀。
        /// 默认值为 "Col"，生成的列名格式为 "Col1"、"Col2" 等。
        /// 若同时配置 <see cref="HeaderPrefixI18nMap"/>，则会优先按当前区域性从映射表中解析前缀。
        /// </summary>
        public string HeaderPrefix { get; set; } = "Col";

        /// <summary>
        /// 用于多语言环境的表头前缀映射表，键为区域性名称（例如 "zh-CN"、"en-US" 或 "en"），值为对应语言下使用的前缀文本。
        /// 当该映射表非空时，将按照当前 <see cref="System.Globalization.CultureInfo.CurrentUICulture"/> 的 Name 和两位语言代码依次查找匹配前缀；
        /// 若均未匹配到，则回退到 <see cref="HeaderPrefix"/>。
        /// </summary>
        public Dictionary<string, string> HeaderPrefixI18nMap { get; set; }

        /// <summary>
        /// 指示在存在表头行且某些列表头为空白（Trim 后长度为 0）时，是否忽略这些列。
        /// 当为 false（默认）时，空白表头列会参与导入，并自动生成默认列名；
        /// 当为 true 时，解析阶段将跳过所有空白表头列，最终导入的 DataTable 中不会包含这些列及其数据。
        /// </summary>
        public bool IgnoreEmptyHeader { get; set; }

        /// <summary>
        /// 表头所在行的行索引（从 0 开始），默认值为 0。
        /// 当 <see cref="HasHeader"/> 为 true 且未显式设置 <see cref="DataRowIndex"/> 时，DataRowIndex 将保持为 HeaderRowIndex + 1；
        /// 当 HasHeader 为 false 时，该值在读取过程中被忽略。
        /// </summary>
        public int HeaderRowIndex
        {
            get
            {
                return headerRowIndex;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("HeaderRowIndex", "HeaderRowIndex 不能为负数");
                }

                headerRowIndex = value;
                if (HasHeader && !isDataRowIndexExplicit)
                {
                    dataRowIndex = headerRowIndex + 1;
                }
            }
        }

        /// <summary>
        /// 数据起始行的行索引（从 0 开始）。
        /// 当 <see cref="HasHeader"/> 为 true 且未显式设置 DataRowIndex 时，默认值为 HeaderRowIndex + 1；
        /// 当 HasHeader 为 false 时，默认值为 1，但可以显式设置为任意非负整数。
        /// </summary>
        public int DataRowIndex
        {
            get
            {
                return dataRowIndex;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("DataRowIndex", "DataRowIndex 不能为负数");
                }

                dataRowIndex = value;
                isDataRowIndexExplicit = true;
            }
        }

        /// <summary>
        /// 初始化 <see cref="ExcelImportSettings"/> 类型的新实例，并设置合理的默认值。
        /// </summary>
        public ExcelImportSettings()
        {
            HasHeader = true;
            EnableBracketNegative = true;
            BracketAsNumeric = true;
            HeaderRenameMapByIndex = new Dictionary<int, string>();
            HeaderRenameMapByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DispersedMapByLetter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DispersedMapByIndex = new Dictionary<int, string>();
            HeaderReadMode = ExcelHeaderReadMode.None;
            HeaderIndexList = new List<int>();
            HeaderPrefixI18nMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            HeaderRowIndex = 0;
            StartColumnName = string.Empty;
            HeaderStartColumnName = string.Empty;
        }
    }
}

#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1201
#pragma warning restore SA1602
#pragma warning restore SA1649
#pragma warning restore SA1600
#pragma warning restore SA1633
#pragma warning restore CS1591
