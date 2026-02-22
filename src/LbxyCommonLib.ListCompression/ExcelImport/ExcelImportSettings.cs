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

    public enum ExcelHeaderReadMode
    {
        None = 0,
        HeaderIndexList = 1,
        HeaderStartIndex = 2,
    }

    public sealed class ExcelImportSettings
    {
        public string SheetName { get; set; } = string.Empty;

        public int? SheetIndex { get; set; }

        public bool HasHeader { get; set; }

        public Dictionary<int, string> HeaderRenameMapByIndex { get; set; }

        public Dictionary<string, string> HeaderRenameMapByName { get; set; }

        public int? StartColumnIndex { get; set; }

        public int? ColumnCount { get; set; }

        public Dictionary<string, string> DispersedMapByLetter { get; set; }

        public Dictionary<int, string> DispersedMapByIndex { get; set; }

        public ExcelHeaderReadMode HeaderReadMode { get; set; }

        public List<int> HeaderIndexList { get; set; }

        public int? HeaderStartColumnIndex { get; set; }

        public bool EnableBracketNegative { get; set; }

        public bool BracketAsNumeric { get; set; }

        public string CustomNegativeRegex { get; set; } = string.Empty;

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
