#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1629
#pragma warning disable SA1116
#pragma warning disable SA1117
#pragma warning disable CS8600
// 多类型同文件
#pragma warning disable SA1402
#pragma warning disable SA1201

namespace LbxyCommonLib.ExcelImport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using LbxyCommonLib.ExcelProcessing;

    public sealed class ExcelImporter
    {
        /// <summary>
        /// 从指定文件路径读取 Excel 数据并加载到 DataTable。
        /// </summary>
        /// <param name="filePath">Excel 文件路径。</param>
        /// <param name="settings">导入设置。</param>
        /// <returns>包含导入结果的 DataTable。</returns>
        public DataTable ReadToDataTable(string filePath, ExcelImportSettings settings)
        {
            Validate(filePath, settings);
            try
            {
                return ReadExcel(filePath, settings);
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ExcelImportException("Excel 导入失败", ex, -1, -1, string.Empty);
            }
        }

        /// <summary>
        /// 从指定流读取 Excel 数据并加载到 DataTable。
        /// </summary>
        /// <param name="stream">传入可读取的Excel数据流，若提供则优先使用流而非文件路径。</param>
        /// <param name="settings">导入设置。</param>
        /// <returns>包含导入结果的 DataTable。</returns>
        /// <exception cref="ArgumentException">当 stream 为 null 或不可读时抛出。</exception>
        public DataTable ReadToDataTable(Stream stream, ExcelImportSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentException("stream 不能为空", "stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", "stream");
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException("settings"), -1, -1, string.Empty);
            }

            try
            {
                return ReadExcelFromStream(stream, settings);
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WrapAsExcelImportException("Excel 导入失败", ex);
            }
        }

        public async Task<DataTable> ReadToDataTableAsync(string filePath, ExcelImportSettings settings, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(filePath, settings);
            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ReadExcel(filePath, settings);
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException oce)
            {
                throw new ExcelImportException("Excel 导入被取消", oce, -1, -1, string.Empty);
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WrapAsExcelImportException("Excel 导入失败", ex);
            }
        }

        /// <summary>
        /// 异步从指定流读取 Excel 数据并加载到 DataTable。
        /// </summary>
        /// <param name="stream">传入可读取的Excel数据流，若提供则优先使用流而非文件路径。</param>
        /// <param name="settings">导入设置。</param>
        /// <param name="cancellationToken">取消标记。</param>
        /// <returns>包含导入结果的 DataTable。</returns>
        /// <exception cref="ArgumentException">当 stream 为 null 或不可读时抛出。</exception>
        public async Task<DataTable> ReadToDataTableAsync(Stream stream, ExcelImportSettings settings, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentException("stream 不能为空", "stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", "stream");
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException("settings"), -1, -1, string.Empty);
            }

            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ReadExcelFromStream(stream, settings);
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException oce)
            {
                throw new ExcelImportException("Excel 导入被取消", oce, -1, -1, string.Empty);
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WrapAsExcelImportException("Excel 导入失败", ex);
            }
        }

        public ExcelImportFillResult FillPredefinedDataTable(string filePath, ExcelImportSettings settings, DataTable target)
        {
            if (target == null)
            {
                throw new ExcelImportException("目标 DataTable 为空", new ArgumentNullException("target"), -1, -1, string.Empty);
            }

            Validate(filePath, settings);
            if (!settings.HasHeader)
            {
                throw new ExcelImportException("高级读取模式要求存在表头行（HasHeader=true）", new InvalidOperationException("HasHeader=false"), -1, -1, string.Empty);
            }

            if (settings.HeaderReadMode == ExcelHeaderReadMode.None)
            {
                throw new ExcelImportException("未指定高级表头读取模式 HeaderReadMode", new InvalidOperationException("HeaderReadMode=None"), -1, -1, string.Empty);
            }

            var logs = new List<ExcelImportLogEntry>();

            try
            {
                using (var workbook = ExcelWorkbookFactory.Open(filePath))
                {
                    var sheet = GetTargetSheet(workbook, settings);
                    var rowCount = sheet.RowCount;
                    var columnCount = sheet.ColumnCount;

                    if (rowCount == 0)
                    {
                        throw new ExcelImportException("Excel 中不存在任何行，无法读取表头", new InvalidOperationException("EmptySheet"), -1, -1, string.Empty);
                    }

                    var headerRow = ReadRowValuesFromSheet(sheet, 0, columnCount);
                    if (headerRow.Count == 0)
                    {
                        throw new ExcelImportException("未检测到表头行内容", new InvalidOperationException("EmptyHeader"), -1, -1, string.Empty);
                    }

                    var bindings = BuildAdvancedColumnBindings(headerRow, target, settings, logs);

                    for (var r = 1; r < rowCount; r++)
                    {
                        var rowValues = ReadRowValuesFromSheet(sheet, r, columnCount);
                        var row = target.NewRow();

                        for (var i = 0; i < bindings.Count; i++)
                        {
                            var binding = bindings[i];
                            var srcIndex = binding.SourceIndex;
                            var targetIndex = binding.TargetColumnIndex;
                            var raw = srcIndex < rowValues.Count ? rowValues[srcIndex] : string.Empty;
                            var normalized = NormalizeValue(raw, settings, r + 1, srcIndex);
                            var column = target.Columns[targetIndex];

                            object valueToAssign;
                            if (normalized == null || normalized is DBNull)
                            {
                                valueToAssign = DBNull.Value;
                            }
                            else
                            {
                                valueToAssign = ConvertToColumnType(
                                    normalized,
                                    column.DataType,
                                    logs,
                                    r + 1,
                                    srcIndex,
                                    column.ColumnName,
                                    raw);
                            }

                            row[targetIndex] = valueToAssign;
                        }

                        target.Rows.Add(row);
                    }
                }
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WrapAsExcelImportException("Excel 导入失败", ex);
            }

            return new ExcelImportFillResult(target, logs);
        }

        public object[][] ImportExcel(string filePath, ExcelImportSettings settings)
        {
            var table = ReadToDataTable(filePath, settings);
            return ConvertDataTableToMatrix(table);
        }

        public object[][] ImportExcel(Stream stream, ExcelImportSettings settings)
        {
            var table = ReadToDataTable(stream, settings);
            return ConvertDataTableToMatrix(table);
        }

        private static void Validate(string filePath, ExcelImportSettings settings)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new ExcelImportException("文件不存在", new FileNotFoundException(filePath), -1, -1, filePath ?? string.Empty, ExcelImportErrorCode.FileNotFound);
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException("settings"), -1, -1, string.Empty);
            }
        }

        private static object[][] ConvertDataTableToMatrix(DataTable table)
        {
            var result = new object[table.Rows.Count][];
            for (var r = 0; r < table.Rows.Count; r++)
            {
                var row = table.Rows[r];
                var values = new object[table.Columns.Count];
                for (var c = 0; c < table.Columns.Count; c++)
                {
                    values[c] = row[c];
                }

                result[r] = values;
            }

            return result;
        }

        private static DataTable ReadExcelFromStream(Stream stream, ExcelImportSettings settings)
        {
            try
            {
                using (var workbook = ExcelWorkbookProvider.Current.Open(stream, string.Empty))
                {
                    return ReadExcelFromWorkbook(workbook, string.Empty, settings);
                }
            }
            catch (ExcelImportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw WrapAsExcelImportException("Excel 导入失败", ex);
            }
        }

        private static ExcelImportException WrapAsExcelImportException(string message, Exception ex)
        {
            var code = ExcelImportErrorCode.Unknown;
            var processingException = ex as LbxyCommonLib.ExcelProcessing.ExcelProcessingException;
            if (processingException != null)
            {
                var msg = processingException.Message ?? string.Empty;
                if (msg.Contains("无法识别的Excel文件格式") || msg.Contains("不支持的Excel文件格式"))
                {
                    code = ExcelImportErrorCode.UnsupportedFormat;
                }
                else
                {
                    code = ExcelImportErrorCode.ParseFailed;
                }
            }

            return new ExcelImportException(message, ex, -1, -1, string.Empty, code);
        }

        private static DataTable ReadExcel(string filePath, ExcelImportSettings settings)
        {
            using (var workbook = ExcelWorkbookProvider.Current.Open(filePath))
            {
                return ReadExcelFromWorkbook(workbook, Path.GetFileName(filePath), settings);
            }
        }

        private static DataTable ReadExcelFromWorkbook(IExcelWorkbook workbook, string tableName, ExcelImportSettings settings)
        {
            var sheet = GetTargetSheet(workbook, settings);
            var dt = new DataTable(string.IsNullOrEmpty(tableName) ? "Sheet" : tableName);

            var rowCount = sheet.RowCount;
            var columnCount = sheet.ColumnCount;
            var headerProcessed = false;
            var colMap = new List<int>();
            var headerNames = new List<string>();
            var startRow = 0;

            if (settings.HasHeader && rowCount > 0)
            {
                headerNames = ReadRowValuesFromSheet(sheet, 0, columnCount);
                BuildSchema(dt, headerNames, settings);
                BuildColumnMap(colMap, headerNames.Count, settings);
                headerProcessed = true;
                startRow = 1;
            }

            if (!headerProcessed)
            {
                var firstRowValues = rowCount > 0 ? ReadRowValuesFromSheet(sheet, 0, columnCount) : new List<string>();
                var cols = firstRowValues.Count > 0 ? firstRowValues.Count : columnCount;
                BuildSchemaNoHeader(dt, cols, settings);
                BuildColumnMap(colMap, cols, settings);
                headerProcessed = true;
                startRow = 0;
            }

            for (var r = startRow; r < rowCount; r++)
            {
                var rowValues = ReadRowValuesFromSheet(sheet, r, colMap.Count);
                var row = dt.NewRow();
                for (var i = 0; i < rowValues.Count; i++)
                {
                    var targetOrdinal = MapTargetOrdinal(i, colMap);
                    if (targetOrdinal < 0 || targetOrdinal >= dt.Columns.Count)
                    {
                        continue;
                    }

                    var raw = rowValues[i];
                    var normalized = NormalizeValue(raw, settings, r + 1, i);
                    row[targetOrdinal] = normalized ?? DBNull.Value;
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private static IExcelWorksheet GetTargetSheet(IExcelWorkbook workbook, ExcelImportSettings settings)
        {
            try
            {
                if (!string.IsNullOrEmpty(settings.SheetName))
                {
                    return workbook.GetWorksheet(settings.SheetName);
                }

                if (settings.SheetIndex.HasValue)
                {
                    return workbook.GetWorksheet(settings.SheetIndex.Value);
                }

                return workbook.GetWorksheet(0);
            }
            catch (Exception ex)
            {
                var snapshot = settings.SheetName ?? (settings.SheetIndex.HasValue ? settings.SheetIndex.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                throw new ExcelImportException("未找到目标工作表", ex, -1, -1, snapshot);
            }
        }

        private static List<string> ReadRowValuesFromSheet(IExcelWorksheet sheet, int rowIndex, int columnCount)
        {
            var values = new List<string>();
            for (var c = 0; c < columnCount; c++)
            {
                string s;
                try
                {
                    var v = sheet.GetCellValue(rowIndex, c, true);
                    if (v == null)
                    {
                        s = string.Empty;
                    }
                    else
                    {
                        s = Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty;
                    }
                }
                catch (FormatException)
                {
                    s = string.Empty;
                }

                values.Add(s);
            }

            return values;
        }

        private static List<ExcelColumnBinding> BuildAdvancedColumnBindings(List<string> headerRow, DataTable target, ExcelImportSettings settings, List<ExcelImportLogEntry> logs)
        {
            var bindings = new List<ExcelColumnBinding>();

            if (settings.HeaderReadMode == ExcelHeaderReadMode.HeaderIndexList)
            {
                if (settings.HeaderIndexList == null || settings.HeaderIndexList.Count == 0)
                {
                    throw new ExcelImportException("HeaderReadMode 为 HeaderIndexList 时必须提供 HeaderIndexList", new ArgumentException("HeaderIndexList"), -1, -1, string.Empty);
                }

                if (target.Columns.Count < settings.HeaderIndexList.Count)
                {
                    throw new ExcelImportException("预构建 DataTable 列数不足以匹配所有指定表头索引", new ArgumentException("target.Columns"), -1, -1, string.Empty);
                }

                for (var i = 0; i < settings.HeaderIndexList.Count; i++)
                {
                    var srcIndex = settings.HeaderIndexList[i];
                    if (srcIndex < 0 || srcIndex >= headerRow.Count)
                    {
                        throw new ExcelImportException("表头索引超出范围: " + srcIndex.ToString(CultureInfo.InvariantCulture), new ArgumentOutOfRangeException("HeaderIndexList"), -1, srcIndex, string.Empty);
                    }

                    var targetIndex = i;
                    var headerName = headerRow[srcIndex] ?? string.Empty;
                    var columnName = target.Columns[targetIndex].ColumnName ?? string.Empty;
                    if (!string.Equals(headerName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        logs.Add(new ExcelImportLogEntry(1, srcIndex, columnName, "表头名称与目标列不一致: " + headerName + " -> " + columnName, headerName));
                    }

                    bindings.Add(new ExcelColumnBinding(srcIndex, targetIndex));
                }

                return bindings;
            }

            if (settings.HeaderReadMode == ExcelHeaderReadMode.HeaderStartIndex)
            {
                if (!settings.HeaderStartColumnIndex.HasValue)
                {
                    throw new ExcelImportException("HeaderReadMode 为 HeaderStartIndex 时必须提供 HeaderStartColumnIndex", new ArgumentNullException("HeaderStartColumnIndex"), -1, -1, string.Empty);
                }

                var start = settings.HeaderStartColumnIndex.Value;
                if (start < 0 || start >= headerRow.Count)
                {
                    throw new ExcelImportException("起始列索引超出表头范围: " + start.ToString(CultureInfo.InvariantCulture), new ArgumentOutOfRangeException("HeaderStartColumnIndex"), -1, start, string.Empty);
                }

                if (target.Columns.Count > headerRow.Count - start)
                {
                    throw new ExcelImportException("预构建 DataTable 列数超出从起始列到末尾的表头数量", new ArgumentException("target.Columns"), -1, -1, string.Empty);
                }

                for (var i = 0; i < target.Columns.Count; i++)
                {
                    var srcIndex = start + i;
                    var column = target.Columns[i];
                    var headerName = headerRow[srcIndex] ?? string.Empty;
                    var columnName = column.ColumnName ?? string.Empty;
                    if (!string.Equals(headerName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        logs.Add(new ExcelImportLogEntry(1, srcIndex, columnName, "表头名称与目标列不一致: " + headerName + " -> " + columnName, headerName));
                    }

                    bindings.Add(new ExcelColumnBinding(srcIndex, i));
                }

                return bindings;
            }

            throw new ExcelImportException("未识别的 HeaderReadMode", new InvalidOperationException("HeaderReadMode"), -1, -1, settings.HeaderReadMode.ToString());
        }

        private static object ConvertToColumnType(object value, Type targetType, List<ExcelImportLogEntry> logs, int rowIndex, int columnIndex, string columnName, string raw)
        {
            if (value == null || value is DBNull)
            {
                return DBNull.Value;
            }

            if (targetType == typeof(object))
            {
                return value;
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlying.IsInstanceOfType(value))
                {
                    return value;
                }

                if (value is IConvertible)
                {
                    return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
                }

                var str = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (str == null)
                {
                    return DBNull.Value;
                }

                return Convert.ChangeType(str, underlying, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                var rawValue = raw ?? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                logs.Add(new ExcelImportLogEntry(rowIndex, columnIndex, columnName, "数据类型转换失败: " + ex.Message, rawValue));
                return DBNull.Value;
            }
        }

        private sealed class ExcelColumnBinding
        {
            public ExcelColumnBinding(int sourceIndex, int targetColumnIndex)
            {
                SourceIndex = sourceIndex;
                TargetColumnIndex = targetColumnIndex;
            }

            public int SourceIndex { get; }

            public int TargetColumnIndex { get; }
        }

        private static List<string> ReadRowValues(XmlReader reader, string[] sharedStrings)
        {
            var values = new List<string>();
            if (reader.IsEmptyElement)
            {
                return values;
            }

            var depth = reader.Depth;
            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth && reader.Name == "row"))
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "c")
                {
                    var tAttr = reader.GetAttribute("t");
                    var sAttr = reader.GetAttribute("s");
                    string cellValue = null;

                    var cellDepth = reader.Depth;
                    while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Depth == cellDepth && reader.Name == "c"))
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "v")
                        {
                            cellValue = reader.ReadElementContentAsString();
                            break;
                        }
                    }

                    if (tAttr == "s" && !string.IsNullOrEmpty(cellValue))
                    {
                        int idx;
                        if (int.TryParse(cellValue, out idx) && idx >= 0 && idx < sharedStrings.Length)
                        {
                            values.Add(sharedStrings[idx]);
                        }
                        else
                        {
                            values.Add(cellValue ?? string.Empty);
                        }
                    }
                    else
                    {
                        values.Add(cellValue ?? string.Empty);
                    }
                }
            }

            return values;
        }

        private static string[] LoadSharedStrings(ZipArchive zip)
        {
            var entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return new string[0];
            }

            var list = new List<string>();
            using (var s = entry.Open())
            using (var reader = XmlReader.Create(s, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "si")
                    {
                        list.Add(ReadSharedStringItem(reader));
                    }
                }
            }

            return list.ToArray();
        }

        private static string ReadSharedStringItem(XmlReader reader)
        {
            using (var subtree = reader.ReadSubtree())
            {
                var sb = new System.Text.StringBuilder();
                while (subtree.Read())
                {
                    if (subtree.NodeType == XmlNodeType.Element && subtree.Name == "t")
                    {
                        sb.Append(subtree.ReadElementContentAsString());
                    }
                }

                return sb.ToString();
            }
        }

        private static string ResolveSheetPath(ZipArchive zip, ExcelImportSettings settings)
        {
            var workbook = zip.GetEntry("xl/workbook.xml");
            if (workbook == null)
            {
                throw new ExcelImportException("工作簿缺失", new FileNotFoundException("xl/workbook.xml"), -1, -1, string.Empty);
            }

            var idToPath = new Dictionary<string, string>(StringComparer.Ordinal);
            var rels = zip.GetEntry("xl/_rels/workbook.xml.rels");
            if (rels != null)
            {
                using (var rs = rels.Open())
                using (var rr = XmlReader.Create(rs, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
                {
                    while (rr.Read())
                    {
                        if (rr.NodeType == XmlNodeType.Element && rr.Name == "Relationship")
                        {
                            var id = rr.GetAttribute("Id");
                            var target = rr.GetAttribute("Target");
                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target) && target.StartsWith("worksheets/", StringComparison.Ordinal))
                            {
                                idToPath[id] = "xl/" + target.Replace("\\", "/");
                            }
                        }
                    }
                }
            }

            var sheetIndex = 0;
            var selectedPath = string.Empty;
            using (var s = workbook.Open())
            using (var r = XmlReader.Create(s, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
            {
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element && r.Name == "sheet")
                    {
                        sheetIndex++;
                        var name = r.GetAttribute("name") ?? string.Empty;
                        var id = r.GetAttribute("r:id");
                        var path = (!string.IsNullOrEmpty(id) && idToPath.ContainsKey(id)) ? idToPath[id] : string.Empty;

                        var nameMatch = !string.IsNullOrEmpty(settings.SheetName) && string.Equals(settings.SheetName, name, StringComparison.OrdinalIgnoreCase);
                        var indexMatch = settings.SheetIndex.HasValue && settings.SheetIndex.Value == (sheetIndex - 1);

                        if (nameMatch || indexMatch)
                        {
                            selectedPath = path;
                            break;
                        }

                        if (string.IsNullOrEmpty(settings.SheetName) && !settings.SheetIndex.HasValue && selectedPath.Length == 0)
                        {
                            selectedPath = path; // default: first sheet
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(selectedPath))
            {
                throw new ExcelImportException("未找到目标工作表", new ArgumentException("sheet"), -1, -1, string.Empty);
            }

            return selectedPath;
        }

        private static void BuildSchema(DataTable dt, List<string> headerNames, ExcelImportSettings settings)
        {
            var cols = headerNames.Count;
            for (var i = 0; i < cols; i++)
            {
                var name = headerNames[i] ?? ("Col" + i.ToString(CultureInfo.InvariantCulture));
                if (settings.HeaderRenameMapByIndex.ContainsKey(i))
                {
                    name = settings.HeaderRenameMapByIndex[i];
                }
                else if (!string.IsNullOrEmpty(name) && settings.HeaderRenameMapByName.ContainsKey(name))
                {
                    name = settings.HeaderRenameMapByName[name];
                }

                dt.Columns.Add(name, typeof(object));
            }
        }

        private static void BuildSchemaNoHeader(DataTable dt, int cols, ExcelImportSettings settings)
        {
            if (settings.DispersedMapByIndex.Count == 0 && settings.DispersedMapByLetter.Count == 0 && !settings.StartColumnIndex.HasValue)
            {
                for (var i = 0; i < cols; i++)
                {
                    dt.Columns.Add("Col" + i.ToString(CultureInfo.InvariantCulture), typeof(object));
                }
                return;
            }

            var names = ResolveTargetNames(cols, settings);
            for (var i = 0; i < names.Length; i++)
            {
                dt.Columns.Add(names[i], typeof(object));
            }
        }

        private static void BuildColumnMap(List<int> colMap, int srcCols, ExcelImportSettings settings)
        {
            colMap.Clear();
            var names = ResolveTargetNames(srcCols, settings);
            for (var i = 0; i < srcCols; i++)
            {
                colMap.Add(i < names.Length ? i : -1);
            }
        }

        private static string[] ResolveTargetNames(int srcCols, ExcelImportSettings settings)
        {
            if (settings.StartColumnIndex.HasValue && settings.ColumnCount.HasValue)
            {
                var names = new List<string>();
                for (var i = 0; i < settings.ColumnCount.Value; i++)
                {
                    names.Add("Col" + (settings.StartColumnIndex.Value + i).ToString(CultureInfo.InvariantCulture));
                }
                return names.ToArray();
            }

            if (settings.DispersedMapByIndex.Count > 0)
            {
                var names = new List<string>();
                foreach (var kv in settings.DispersedMapByIndex)
                {
                    names.Add(kv.Value);
                }
                return names.ToArray();
            }

            if (settings.DispersedMapByLetter.Count > 0)
            {
                var names = new List<string>();
                foreach (var kv in settings.DispersedMapByLetter)
                {
                    names.Add(kv.Value);
                }
                return names.ToArray();
            }

            var defaultNames = new List<string>();
            for (var i = 0; i < srcCols; i++)
            {
                defaultNames.Add("Col" + i.ToString(CultureInfo.InvariantCulture));
            }

            return defaultNames.ToArray();
        }

        private static int MapTargetOrdinal(int srcIndex, List<int> colMap)
        {
            if (srcIndex < 0 || srcIndex >= colMap.Count)
            {
                return -1;
            }

            return colMap[srcIndex];
        }

        private static object NormalizeValue(string raw, ExcelImportSettings settings, int rowIndex, int columnIndex)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return DBNull.Value;
            }

            raw = raw.Trim();

            if (raw.StartsWith("-", StringComparison.Ordinal))
            {
                decimal d;
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                {
                    return d;
                }

                return DBNull.Value;
            }

            if (settings.EnableBracketNegative && raw.Length > 2 && raw[0] == '(' && raw[raw.Length - 1] == ')')
            {
                var inner = raw.Substring(1, raw.Length - 2).Trim();
                decimal d1;
                if (decimal.TryParse(inner, NumberStyles.Any, CultureInfo.InvariantCulture, out d1))
                {
                    return settings.BracketAsNumeric ? (object)(-d1) : (object)("-" + inner);
                }

                return DBNull.Value;
            }

            if (!string.IsNullOrEmpty(settings.CustomNegativeRegex))
            {
                try
                {
                    var m = Regex.Match(raw, settings.CustomNegativeRegex);
                    if (m.Success && m.Groups.Count > 1)
                    {
                        var val = m.Groups[1].Value;
                        decimal dv;
                        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out dv))
                        {
                            return (object)(-dv);
                        }
                    }
                }
                catch (Exception rex)
                {
                    throw new ExcelImportException("自定义负数正则解析失败", rex, rowIndex, columnIndex, raw);
                }
            }

            decimal dn;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out dn))
            {
                return dn;
            }

            return raw;
        }
    }

    public sealed class ExcelImportLogEntry
    {
        public ExcelImportLogEntry(int rowIndex, int columnIndex, string columnName, string message, string rawValue)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnName = columnName;
            Message = message;
            RawValue = rawValue;
        }

        public int RowIndex { get; }

        public int ColumnIndex { get; }

        public string ColumnName { get; }

        public string Message { get; }

        public string RawValue { get; }
    }

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

#pragma warning restore SA1629
#pragma warning restore SA1513
#pragma warning restore SA1503
#pragma warning restore SA1117
#pragma warning restore SA1116
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1649
#pragma warning restore SA1600
#pragma warning restore SA1633
#pragma warning restore SA1402
#pragma warning restore SA1201
#pragma warning restore CS1591
