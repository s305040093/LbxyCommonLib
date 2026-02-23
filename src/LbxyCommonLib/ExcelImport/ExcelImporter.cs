#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1629
#pragma warning disable SA1642
#pragma warning disable SA1116
#pragma warning disable SA1117
// 多类型同文件
#pragma warning disable SA1402
#pragma warning disable SA1201
#pragma warning disable SA1602

namespace LbxyCommonLib.ExcelImport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using LbxyCommonLib.ExcelProcessing;

    /// <summary>
    /// 提供基于 Excel 工作簿的高层导入功能，包括读取到 DataTable、object 矩阵以及块导入与合并等操作。
    /// 当未在 <see cref="ExcelImportSettings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/> 或
    /// <see cref="ExcelImportSettings.SheetIndex"/> 时，在默认基于 NPOI 的实现中将优先使用当前激活的工作表作为导入来源。
    /// </summary>
    public sealed partial class ExcelImporter
    {
        public static MatrixRemainderMode DefaultRemainderMode { get; set; } = MatrixRemainderMode.Error;

#if NET45
        public static IMatrixRemainderHandler RemainderHandler { get; set; }
#else
        public static IMatrixRemainderHandler? RemainderHandler { get; set; }
#endif

        /// <summary>
        /// 从指定文件路径读取 Excel 数据并加载到 DataTable。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于指定工作表、表头模式、列映射与类型转换等行为。</param>
        /// <returns>包含导入结果数据的 <see cref="DataTable"/> 实例。</returns>
        /// <exception cref="ExcelImportException">
        /// 当文件不存在、设置为空、文件格式不受支持或解析过程中发生错误（包括数值/日期转换失败等）时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-23
        /// <para>
        /// 当 <see cref="ExcelImportSettings.HasHeader"/> 为 true 且处于默认表头映射模式时，如果表头行中某些列的文本为空、
        /// 仅包含空白字符或缺失，这些列会自动使用默认列名进行填充，格式为 "Col" 加上从 1 开始的列序号
        /// （例如 Col1、Col2、Col3）。若默认列名与现有非空表头重复，则会在默认列名后追加递增后缀
        /// （例如 Col2_1、Col2_2），以保证最终生成的列名在当前表中唯一。
        /// </para>
        /// <para>
        /// 当 <see cref="ExcelImportSettings.IgnoreEmptyHeader"/> 为 true 且仍处于默认表头映射模式时，所有表头文本为空白（Trim 后长度为 0）的列
        /// 会在解析阶段被整体跳过，结果 DataTable 中不会包含这些列及其数据。
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var settings = new ExcelImportSettings
        /// {
        ///     HasHeader = true,
        /// };
        ///
        /// var importer = new ExcelImporter();
        /// var table = importer.ReadToDataTable("orders.xlsx", settings);
        /// </code>
        /// </example>
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
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取数据流；调用此重载时不会访问文件系统路径。</param>
        /// <param name="settings">导入设置，用于指定工作表、表头模式、列映射与类型转换等行为。</param>
        /// <returns>包含导入结果数据的 <see cref="DataTable"/> 实例。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="stream"/> 为 null 或不可读时抛出。</exception>
        /// <exception cref="ExcelImportException">
        /// 当设置为空、文件格式不受支持或解析过程中发生错误（包括数值/日期转换失败等）时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-23
        /// </remarks>
        /// <example>
        /// <code>
        /// using (var stream = File.OpenRead("orders.xlsx"))
        /// {
        ///     var settings = new ExcelImportSettings { HasHeader = true };
        ///     var importer = new ExcelImporter();
        ///     var table = importer.ReadToDataTable(stream, settings);
        /// }
        /// </code>
        /// </example>
        public DataTable ReadToDataTable(Stream stream, ExcelImportSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentException("stream 不能为空", nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", nameof(stream));
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException(nameof(settings)), -1, -1, string.Empty);
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

        /// <summary>
        /// 异步从指定文件路径读取 Excel 数据并加载到 DataTable。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于指定工作表、表头模式、列映射与类型转换等行为。</param>
        /// <param name="cancellationToken">可选取消标记，用于在长时间导入过程中主动取消操作。</param>
        /// <returns>包含导入结果数据的 <see cref="DataTable"/> 实例。</returns>
        /// <exception cref="ExcelImportException">
        /// 当文件不存在、设置为空、文件格式不受支持、解析过程中发生错误或导入被取消时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-22
        /// </remarks>
        /// <example>
        /// <code>
        /// var settings = new ExcelImportSettings { HasHeader = true };
        /// var importer = new ExcelImporter();
        ///
        /// using var cts = new CancellationTokenSource();
        /// var table = await importer.ReadToDataTableAsync("orders.xlsx", settings, cts.Token);
        /// </code>
        /// </example>
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
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取数据流；调用此重载时不会访问文件系统路径。</param>
        /// <param name="settings">导入设置，用于指定工作表、表头模式、列映射与类型转换等行为。</param>
        /// <param name="cancellationToken">取消标记。</param>
        /// <returns>包含导入结果数据的 <see cref="DataTable"/> 实例。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="stream"/> 为 null 或不可读时抛出。</exception>
        /// <exception cref="ExcelImportException">
        /// 当设置为空、文件格式不受支持、解析过程中发生错误或导入被取消时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-23
        /// </remarks>
        /// <example>
        /// <code>
        /// using var stream = File.OpenRead("orders.xlsx");
        /// var settings = new ExcelImportSettings { HasHeader = true };
        /// var importer = new ExcelImporter();
        /// var table = await importer.ReadToDataTableAsync(stream, settings);
        /// </code>
        /// </example>
        public async Task<DataTable> ReadToDataTableAsync(Stream stream, ExcelImportSettings settings, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentException("stream 不能为空", nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", nameof(stream));
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException(nameof(settings)), -1, -1, string.Empty);
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

        /// <summary>
        /// 使用预先定义结构的 <see cref="DataTable"/> 作为目标，将 Excel 数据按列名映射并填充到该表中。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">
        /// 导入设置，要求 <see cref="ExcelImportSettings.HasHeader"/> 为 true，
        /// 并通过 <see cref="ExcelImportSettings.HeaderReadMode"/>、<see cref="ExcelImportSettings.HeaderRowIndex"/>、
        /// <see cref="ExcelImportSettings.DataRowIndex"/> 等选项指定表头与数据范围及列映射策略。
        /// </param>
        /// <param name="target">预构建的目标 <see cref="DataTable"/>，其中列名称和数据类型定义导入目标结构。</param>
        /// <returns>
        /// <see cref="ExcelImportFillResult"/> 对象，包含填充后的 DataTable 以及导入过程中的日志（如类型转换失败、列名不匹配等）。
        /// </returns>
        /// <exception cref="ExcelImportException">
        /// 当目标 DataTable 为空、未启用表头模式、未指定高级表头读取模式、表头/数据行索引配置不合法、
        /// 表头缺失或列映射失败以及解析过程中发生错误时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-22
        /// </remarks>
        /// <example>
        /// <code>
        /// var settings = new ExcelImportSettings
        /// {
        ///     HasHeader = true,
        ///     HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
        ///     HeaderRowIndex = 0,
        ///     DataRowIndex = 1,
        /// };
        ///
        /// var table = new DataTable("Orders");
        /// table.Columns.Add("CustomerName", typeof(string));
        /// table.Columns.Add("Amount", typeof(decimal));
        ///
        /// var importer = new ExcelImporter();
        /// var result = importer.FillPredefinedDataTable("orders.xlsx", settings, table);
        /// </code>
        /// </example>
        public ExcelImportFillResult FillPredefinedDataTable(string filePath, ExcelImportSettings settings, DataTable target)
        {
            if (target == null)
            {
                throw new ExcelImportException("目标 DataTable 为空", new ArgumentNullException(nameof(target)), -1, -1, string.Empty);
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

            if (settings.HasHeader && settings.HeaderRowIndex >= settings.DataRowIndex)
            {
                throw new ExcelImportException("当 HasHeader 为 true 时必须满足 HeaderRowIndex < DataRowIndex", new ArgumentException("settings"), -1, -1, string.Empty);
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

                    var headerRow = ReadRowValuesFromSheet(sheet, settings.HeaderRowIndex, columnCount);
                    if (headerRow.Count == 0)
                    {
                        throw new ExcelImportException("未检测到表头行内容", new InvalidOperationException("EmptyHeader"), -1, -1, string.Empty);
                    }

                    var bindings = BuildAdvancedColumnBindings(headerRow, target, settings, logs);

                    for (var r = settings.DataRowIndex; r < rowCount; r++)
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
                                    settings,
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

        /// <summary>
        /// 使用预先定义结构的 <see cref="DataTable"/> 作为目标，从指定文件路径读取 Excel，
        /// 并根据 <see cref="ExcelImportSettings.HeaderRowIndex"/> 与 <see cref="ExcelImportSettings.DataRowIndex"/> 控制表头与数据起始行（从 0 开始的行索引）。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">
        /// 导入设置，要求 <see cref="ExcelImportSettings.HasHeader"/> 为 true（本方法将强制设置为 true），
        /// 并通过 <see cref="ExcelImportSettings.HeaderReadMode"/> 指定高级表头读取模式。
        /// </param>
        /// <param name="target">预构建的目标 <see cref="DataTable"/>，其中列名称和数据类型定义导入目标结构。</param>
        /// <returns>
        /// <see cref="ExcelImportFillResult"/> 对象，包含填充后的 DataTable 以及导入过程中的日志（如类型转换失败、列名不匹配等）。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 当 HeaderRowIndex 或 DataRowIndex 超出实际行数，或 HeaderRowIndex 不小于 DataRowIndex 时抛出。
        /// </exception>
        /// <exception cref="ExcelImportException">
        /// 当文件不存在、目标 DataTable 或设置为空、未指定高级表头读取模式、表头缺失或列映射失败以及解析过程中发生错误时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// var settings = new ExcelImportSettings
        /// {
        ///     HasHeader = true,
        ///     HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
        ///     HeaderRowIndex = 0,
        ///     DataRowIndex = 1,
        /// };
        ///
        /// var table = new DataTable("Orders");
        /// table.Columns.Add("CustomerName", typeof(string));
        /// table.Columns.Add("Amount", typeof(decimal));
        ///
        /// var importer = new ExcelImporter();
        /// var result = importer.ImportAdvanced("orders.xlsx", settings, table);
        /// </code>
        /// </example>
        public ExcelImportFillResult ImportAdvanced(string filePath, ExcelImportSettings settings, DataTable target)
        {
            Validate(filePath, settings);
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ImportAdvanced(stream, settings, target);
            }
        }

        /// <summary>
        /// 使用预先定义结构的 <see cref="DataTable"/> 作为目标，从流中读取 Excel，
        /// 并根据 <see cref="ExcelImportSettings.HeaderRowIndex"/> 与 <see cref="ExcelImportSettings.DataRowIndex"/> 控制表头与数据起始行（从 0 开始的行索引）。
        /// </summary>
        /// <param name="stream">Excel 数据流，必须为可读取的流。</param>
        /// <param name="settings">
        /// 导入设置，要求 <see cref="ExcelImportSettings.HasHeader"/> 为 true（本方法将强制设置为 true），
        /// 并通过 <see cref="ExcelImportSettings.HeaderReadMode"/> 指定高级表头读取模式。
        /// </param>
        /// <param name="target">预构建的目标 <see cref="DataTable"/>，其中列名称和数据类型定义导入目标结构。</param>
        /// <returns>
        /// <see cref="ExcelImportFillResult"/> 对象，包含填充后的 DataTable 以及导入过程中的日志（如类型转换失败、列名不匹配等）。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 当流不可读、HeaderRowIndex 或 DataRowIndex 超出实际行数，或 HeaderRowIndex 不小于 DataRowIndex 时抛出。
        /// </exception>
        /// <exception cref="ExcelImportException">
        /// 当目标 DataTable 或设置为空、未指定高级表头读取模式、表头缺失或列映射失败以及解析过程中发生错误时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// using (var stream = File.OpenRead("orders.xlsx"))
        /// {
        ///     var settings = new ExcelImportSettings
        ///     {
        ///         HasHeader = true,
        ///         HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
        ///         HeaderRowIndex = 0,
        ///         DataRowIndex = 1,
        ///     };
        ///
        ///     var table = new DataTable("Orders");
        ///     table.Columns.Add("CustomerName", typeof(string));
        ///     table.Columns.Add("Amount", typeof(decimal));
        ///
        ///     var importer = new ExcelImporter();
        ///     var result = importer.ImportAdvanced(stream, settings, table);
        /// }
        /// </code>
        /// </example>
        public ExcelImportFillResult ImportAdvanced(Stream stream, ExcelImportSettings settings, DataTable target)
        {
            if (stream == null)
            {
                throw new ArgumentException("stream 不能为空", nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", nameof(stream));
            }

            if (target == null)
            {
                throw new ExcelImportException("目标 DataTable 为空", new ArgumentNullException(nameof(target)), -1, -1, string.Empty);
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException(nameof(settings)), -1, -1, string.Empty);
            }

            if (settings.HeaderReadMode == ExcelHeaderReadMode.None)
            {
                throw new ExcelImportException("未指定高级表头读取模式 HeaderReadMode", new InvalidOperationException("HeaderReadMode=None"), -1, -1, string.Empty);
            }

            if (settings.HeaderRowIndex >= settings.DataRowIndex)
            {
                throw new ArgumentException("当 HasHeader 为 true 时必须满足 HeaderRowIndex < DataRowIndex", nameof(settings));
            }

            settings.HasHeader = true;

            var logs = new List<ExcelImportLogEntry>();

            try
            {
                using (var workbook = ExcelWorkbookProvider.Current.Open(stream, string.Empty))
                {
                    var sheet = GetTargetSheet(workbook, settings);
                    var rowCount = sheet.RowCount;
                    var columnCount = sheet.ColumnCount;

                    if (rowCount == 0)
                    {
                        throw new ExcelImportException("Excel 中不存在任何行，无法读取表头", new InvalidOperationException("EmptySheet"), -1, -1, string.Empty);
                    }

                    if (settings.HeaderRowIndex >= rowCount)
                    {
                        throw new ArgumentException("HeaderRowIndex 超出工作表实际行数", nameof(settings));
                    }

                    if (settings.DataRowIndex >= rowCount)
                    {
                        throw new ArgumentException("DataRowIndex 超出工作表实际行数", nameof(settings));
                    }

                    var headerRow = ReadRowValuesFromSheet(sheet, settings.HeaderRowIndex, columnCount);
                    if (headerRow.Count == 0)
                    {
                        throw new ExcelImportException("未检测到表头行内容", new InvalidOperationException("EmptyHeader"), -1, -1, string.Empty);
                    }

                    var bindings = BuildAdvancedColumnBindings(headerRow, target, settings, logs);

                    for (var r = settings.DataRowIndex; r < rowCount; r++)
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
                                    settings,
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

        /// <summary>
        /// 从指定文件路径读取 Excel 数据，并将结果转换为 object 矩阵（适合直接序列化为 JSON 数组）。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <returns>按行优先的二维 object 数组，每一行对应一条数据记录。</returns>
        /// <exception cref="ExcelImportException">
        /// 当底层 <see cref="ReadToDataTable(string, ExcelImportSettings)"/> 调用失败时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-23
        /// </remarks>
        /// <example>
        /// <code>
        /// var settings = new ExcelImportSettings { HasHeader = true };
        /// var importer = new ExcelImporter();
        /// var rows = importer.ImportExcel("orders.xlsx", settings);
        /// </code>
        /// </example>
        public object[][] ImportExcel(string filePath, ExcelImportSettings settings)
        {
            var table = ReadToDataTable(filePath, settings);
            return ConvertDataTableToMatrix(table);
        }

        /// <summary>
        /// 从指定流读取 Excel 数据，并将结果转换为 object 矩阵（适合直接序列化为 JSON 数组）。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">可读取的 Excel 数据流。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <returns>按行优先的二维 object 数组，每一行对应一条数据记录。</returns>
        /// <exception cref="ExcelImportException">
        /// 当底层 <see cref="ReadToDataTable(Stream, ExcelImportSettings)"/> 调用失败时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-22
        /// Last Modified: 2026-02-23
        /// </remarks>
        /// <example>
        /// <code>
        /// using var stream = File.OpenRead("orders.xlsx");
        /// var settings = new ExcelImportSettings { HasHeader = true };
        /// var importer = new ExcelImporter();
        /// var rows = importer.ImportExcel(stream, settings);
        /// </code>
        /// </example>
        public object[][] ImportExcel(Stream stream, ExcelImportSettings settings)
        {
            var table = ReadToDataTable(stream, settings);
            return ConvertDataTableToMatrix(table);
        }

        /// <summary>
        /// 从指定文件路径读取 Excel 数据，根据导出选项选择区域并转换为 object 矩阵。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">导出选项，用于指定起始行列及导出区域的行列数。</param>
        /// <returns>按行优先的二维 object 数组，只包含指定区域内的单元格数据。</returns>
        /// <exception cref="ExcelImportException">
        /// 当底层 <see cref="ReadToDataTable(string, ExcelImportSettings)"/> 调用失败时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="options"/> 为空时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="options"/>.StartRowIndex 或 StartColumnIndex 小于 0 时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// </remarks>
        public object[][] ImportExcel(string filePath, ExcelImportSettings settings, MatrixExportOptions options)
        {
            var table = ReadToDataTable(filePath, settings);
            return ConvertDataTableToMatrix(table, options);
        }

        /// <summary>
        /// 从指定流读取 Excel 数据，根据导出选项选择区域并转换为 object 矩阵。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取数据流。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">导出选项，用于指定起始行列及导出区域的行列数。</param>
        /// <returns>按行优先的二维 object 数组，只包含指定区域内的单元格数据。</returns>
        /// <exception cref="ExcelImportException">
        /// 当底层 <see cref="ReadToDataTable(Stream, ExcelImportSettings)"/> 调用失败时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="stream"/> 或 <paramref name="options"/> 为空时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="options"/>.StartRowIndex 或 StartColumnIndex 小于 0 时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// </remarks>
        public object[][] ImportExcel(Stream stream, ExcelImportSettings settings, MatrixExportOptions options)
        {
            var table = ReadToDataTable(stream, settings);
            return ConvertDataTableToMatrix(table, options);
        }

        /// <summary>
        /// 从指定文件路径读取 Excel 数据，根据块导出选项切分为多个 object 矩阵块。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">
        /// 块导出选项，用于指定起始行列、导出区域大小以及块的行列尺寸、遍历顺序和余数处理模式。
        /// </param>
        /// <returns>
        /// object 矩阵块的只读列表；每个块为二维 object 数组，按行优先存储。
        /// </returns>
        /// <exception cref="ExcelImportException">
        /// 当读取 Excel 失败，或块尺寸与区域大小不整除且余数模式为 Error/Prompt 且被拒绝时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="options"/> 为空时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="options"/>.StartRowIndex 或 StartColumnIndex 小于 0 时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// </remarks>
        public IReadOnlyList<object[][]> ImportExcelBlocks(string filePath, ExcelImportSettings settings, MatrixExportOptions options)
        {
            var table = ReadToDataTable(filePath, settings);
            return ConvertDataTableToBlocks(table, options);
        }

        /// <summary>
        /// 从指定流读取 Excel 数据，根据块导出选项切分为多个 object 矩阵块。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取数据流。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">
        /// 块导出选项，用于指定起始行列、导出区域大小以及块的行列尺寸、遍历顺序和余数处理模式。
        /// </param>
        /// <returns>
        /// object 矩阵块的只读列表；每个块为二维 object 数组，按行优先存储。
        /// </returns>
        /// <exception cref="ExcelImportException">
        /// 当读取 Excel 失败，或块尺寸与区域大小不整除且余数模式为 Error/Prompt 且被拒绝时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="stream"/> 或 <paramref name="options"/> 为空时抛出。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="options"/>.StartRowIndex 或 StartColumnIndex 小于 0 时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// </remarks>
        public IReadOnlyList<object[][]> ImportExcelBlocks(Stream stream, ExcelImportSettings settings, MatrixExportOptions options)
        {
            var table = ReadToDataTable(stream, settings);
            return ConvertDataTableToBlocks(table, options);
        }

        /// <summary>
        /// 从指定文件路径读取 Excel 数据，按块切分并基于指定策略合并为单一 DataTable。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="filePath">Excel 文件路径，必须为本地存在的 .xlsx/.xls/.xlsm 文件。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">
        /// 块导出选项，用于指定起始行列、导出区域大小以及块的行列尺寸、遍历顺序和余数处理模式。
        /// </param>
        /// <param name="mergeOptions">
        /// 合并选项，用于指定重复键列名及冲突处理策略（覆盖、忽略或追加）。
        /// </param>
        /// <returns>
        /// 包含最终合并结果表、合并日志以及统计信息的 <see cref="ExcelBlockMergeResult"/>。
        /// </returns>
        /// <exception cref="ExcelImportException">
        /// 当文件不存在、被锁定、解析失败，或块尺寸与区域大小不整除且余数模式为 Error/Prompt 且被拒绝时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="options"/> 或 <paramref name="mergeOptions"/> 为空时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// <para>
        /// 本方法首先根据 <paramref name="options"/> 在导入得到的 <see cref="DataTable"/> 上计算有效区域，
        /// 该区域由起始行列、行数与列数共同限定。随后按照块行数与列数以及块遍历顺序，将此二维区域划分为若干逻辑块，
        /// 每个块均视为原始区域的一个子矩阵，从而形成对源数据的“拆分”视图。
        /// </para>
        /// <para>
        /// 在合并阶段，若 <see cref="ExcelBlockMergeOptions.ConflictStrategy"/> 为 <see cref="ExcelBlockMergeConflictStrategy.Append"/>，
        /// 则所有块会按照定义好的遍历顺序依次写回到新的目标 <see cref="DataTable"/> 中：目标表的列集合由拆分区域的列集合直接构成，
        /// 行集合则通过将各块的单元格值按行优先方式拼接重建，保证每个单元格在新表中拥有唯一位置，从而完整保留原始区域的所有数据。
        /// 若冲突策略为其他值，则会先基于指定的键列构造行键，对来自不同块的同一键行执行覆盖或忽略策略。
        /// </para>
        /// </remarks>
        public ExcelBlockMergeResult ImportExcelBlocksAndMerge(string filePath, ExcelImportSettings settings, MatrixExportOptions options, ExcelBlockMergeOptions mergeOptions)
        {
            Validate(filePath, settings);
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            var table = ReadToDataTable(filePath, settings);
            return ImportExcelBlocksAndMergeCore(table, options, mergeOptions, Path.GetFileNameWithoutExtension(filePath));
        }

        /// <summary>
        /// 从指定流读取 Excel 数据，按块切分并基于指定策略合并为单一 DataTable。
        /// 在默认基于 NPOI 的实现中，若未在 <paramref name="settings"/> 中显式指定 <see cref="ExcelImportSettings.SheetName"/>
        /// 或 <see cref="ExcelImportSettings.SheetIndex"/>，则默认从当前激活的工作表中读取数据。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取数据流。</param>
        /// <param name="settings">导入设置，用于控制表头、列映射与类型转换行为。</param>
        /// <param name="options">
        /// 块导出选项，用于指定起始行列、导出区域大小以及块的行列尺寸、遍历顺序和余数处理模式。
        /// </param>
        /// <param name="mergeOptions">
        /// 合并选项，用于指定重复键列名及冲突处理策略（覆盖、忽略或追加）。
        /// </param>
        /// <returns>
        /// 包含最终合并结果表、合并日志以及统计信息的 <see cref="ExcelBlockMergeResult"/>。
        /// </returns>
        /// <exception cref="ExcelImportException">
        /// 当解析 Excel 内容失败，或块尺寸与区域大小不整除且余数模式为 Error/Prompt 且被拒绝时抛出。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="stream"/>、<paramref name="settings"/>、<paramref name="options"/> 或
        /// <paramref name="mergeOptions"/> 为空时抛出。
        /// </exception>
        /// <remarks>
        /// Author: LbxyCommonLib Contributors
        /// Created: 2026-02-23
        /// Last Modified: 2026-02-23
        /// </remarks>
        public ExcelBlockMergeResult ImportExcelBlocksAndMerge(Stream stream, ExcelImportSettings settings, MatrixExportOptions options, ExcelBlockMergeOptions mergeOptions)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            var table = ReadToDataTable(stream, settings);
            return ImportExcelBlocksAndMergeCore(table, options, mergeOptions, string.Empty);
        }

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
                throw new ArgumentException("Column name length cannot exceed 3 characters.", nameof(columnName));
            }

            for (var i = 0; i < trimmed.Length; i++)
            {
                var c = trimmed[i];
                if (c < 'A' || c > 'Z')
                {
                    throw new ArgumentException("Column name must contain only uppercase letters A-Z.", nameof(columnName));
                }
            }

            var oneBased = ExcelColumnConverter.ColumnNameToIndex(trimmed);
            var zeroBased = oneBased - 1;
            if (zeroBased < 0 || zeroBased > 16383)
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), "Column name is out of supported Excel range.");
            }

            return zeroBased;
        }

        public static string ColumnIndexToName(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex > 16383)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be between 0 and 16383.");
            }

            var oneBased = columnIndex + 1;
            return ExcelColumnConverter.ColumnIndexToName(oneBased);
        }

        private static void Validate(string filePath, ExcelImportSettings settings)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new ExcelImportException("文件不存在", new FileNotFoundException(filePath), -1, -1, filePath ?? string.Empty, ExcelImportErrorCode.FileNotFound);
            }

            if (settings == null)
            {
                throw new ExcelImportException("设置为空", new ArgumentNullException(nameof(settings)), -1, -1, string.Empty);
            }
        }

        private static object[][] ConvertDataTableToMatrix(DataTable table)
        {
            var options = new MatrixExportOptions();
            return ConvertDataTableToMatrix(table, options);
        }

        private static object[][] ConvertDataTableToMatrix(DataTable table, MatrixExportOptions options)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.StartRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.StartRowIndex));
            }

            if (options.StartColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.StartColumnIndex));
            }

            var totalRows = table.Rows.Count;
            var totalColumns = table.Columns.Count;

            if (totalRows == 0 || totalColumns == 0)
            {
                return new object[0][];
            }

            if (options.StartRowIndex >= totalRows || options.StartColumnIndex >= totalColumns)
            {
                return new object[0][];
            }

            var maxRowCount = totalRows - options.StartRowIndex;
            var maxColumnCount = totalColumns - options.StartColumnIndex;

            var rowCount = options.RowCount.HasValue && options.RowCount.Value > 0
                ? Math.Min(options.RowCount.Value, maxRowCount)
                : maxRowCount;

            var columnCount = options.ColumnCount.HasValue && options.ColumnCount.Value > 0
                ? Math.Min(options.ColumnCount.Value, maxColumnCount)
                : maxColumnCount;

            var result = new object[rowCount][];
            for (var r = 0; r < rowCount; r++)
            {
                var sourceRow = table.Rows[options.StartRowIndex + r];
                var values = new object[columnCount];
                for (var c = 0; c < columnCount; c++)
                {
                    values[c] = sourceRow[options.StartColumnIndex + c];
                }

                result[r] = values;
            }

            return result;
        }

        private static IReadOnlyList<object[][]> ConvertDataTableToBlocks(DataTable table, MatrixExportOptions options)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.StartRowIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.StartRowIndex));
            }

            if (options.StartColumnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.StartColumnIndex));
            }

            var totalRows = table.Rows.Count;
            var totalColumns = table.Columns.Count;

            if (totalRows == 0 || totalColumns == 0)
            {
                return new object[0][][];
            }

            if (options.StartRowIndex >= totalRows || options.StartColumnIndex >= totalColumns)
            {
                return new object[0][][];
            }

            var maxRowCount = totalRows - options.StartRowIndex;
            var maxColumnCount = totalColumns - options.StartColumnIndex;

            var regionRowCount = options.RowCount.HasValue && options.RowCount.Value > 0
                ? Math.Min(options.RowCount.Value, maxRowCount)
                : maxRowCount;

            var regionColumnCount = options.ColumnCount.HasValue && options.ColumnCount.Value > 0
                ? Math.Min(options.ColumnCount.Value, maxColumnCount)
                : maxColumnCount;

            if (regionRowCount <= 0 || regionColumnCount <= 0)
            {
                return new object[0][][];
            }

            var blockRowSize = options.BlockRowCount.HasValue && options.BlockRowCount.Value > 0
                ? options.BlockRowCount.Value
                : regionRowCount;

            var blockColumnSize = options.BlockColumnCount.HasValue && options.BlockColumnCount.Value > 0
                ? options.BlockColumnCount.Value
                : regionColumnCount;

            if (options.MaxEstimatedBytesPerBlock.HasValue && options.MaxEstimatedBytesPerBlock.Value > 0 && regionColumnCount > 0)
            {
                var bytesPerRow = (long)regionColumnCount * 32L;
                if (bytesPerRow <= 0)
                {
                    bytesPerRow = 32L;
                }

                var maxRowsByMemory = (int)Math.Min(int.MaxValue, options.MaxEstimatedBytesPerBlock.Value / bytesPerRow);
                if (maxRowsByMemory <= 0)
                {
                    maxRowsByMemory = 1;
                }

                if (maxRowsByMemory < blockRowSize)
                {
                    blockRowSize = maxRowsByMemory;
                }
            }

            var rowRemainder = regionRowCount % blockRowSize;
            var columnRemainder = regionColumnCount % blockColumnSize;
            var evenlySplit = rowRemainder == 0 && columnRemainder == 0;

            var effectiveRegionRowCount = regionRowCount;
            var effectiveRegionColumnCount = regionColumnCount;
            var fillMode = false;

            if (!evenlySplit)
            {
                var mode = options.RemainderMode ?? DefaultRemainderMode;
                if (mode == MatrixRemainderMode.Error)
                {
                    throw new ExcelImportException(
                        "Block remainder is not divisible",
                        new InvalidOperationException("Remainder"),
                        -1,
                        -1,
                        string.Empty,
                        ExcelImportErrorCode.BlockRemainderNotDivisible);
                }

                if (mode == MatrixRemainderMode.Fill)
                {
                    fillMode = true;
                }
                else if (mode == MatrixRemainderMode.Prompt)
                {
                    if (RemainderHandler == null)
                    {
                        throw new ExcelImportException(
                            "Remainder handler is not configured",
                            new InvalidOperationException("RemainderHandler"),
                            -1,
                            -1,
                            string.Empty,
                            ExcelImportErrorCode.BlockRemainderNotDivisible);
                    }

                    var context = new MatrixRemainderContext
                    {
                        TotalRowCount = regionRowCount,
                        TotalColumnCount = regionColumnCount,
                        BlockRowSize = blockRowSize,
                        BlockColumnSize = blockColumnSize,
                        RowRemainder = rowRemainder,
                        ColumnRemainder = columnRemainder,
                        Mode = mode,
                        Options = options,
                    };

                    var action = RemainderHandler.Handle(context);
                    if (action == MatrixRemainderAction.Abort)
                    {
                        throw new ExcelImportException(
                            "Block remainder is not divisible",
                            new InvalidOperationException("Remainder"),
                            -1,
                            -1,
                            string.Empty,
                            ExcelImportErrorCode.BlockRemainderNotDivisible);
                    }

                    if (action == MatrixRemainderAction.Truncate)
                    {
                        effectiveRegionRowCount = (regionRowCount / blockRowSize) * blockRowSize;
                        effectiveRegionColumnCount = (regionColumnCount / blockColumnSize) * blockColumnSize;
                        if (effectiveRegionRowCount <= 0 || effectiveRegionColumnCount <= 0)
                        {
                            return new object[0][][];
                        }
                    }
                    else if (action == MatrixRemainderAction.Fill)
                    {
                        fillMode = true;
                    }
                }
            }

            int blockRowCount;
            int blockColumnCount;

            if (fillMode)
            {
                var paddedRowCount = ((regionRowCount + blockRowSize - 1) / blockRowSize) * blockRowSize;
                var paddedColumnCount = ((regionColumnCount + blockColumnSize - 1) / blockColumnSize) * blockColumnSize;
                blockRowCount = paddedRowCount / blockRowSize;
                blockColumnCount = paddedColumnCount / blockColumnSize;
            }
            else
            {
                blockRowCount = effectiveRegionRowCount / blockRowSize;
                blockColumnCount = effectiveRegionColumnCount / blockColumnSize;
            }

            var blocks = new List<object[][]>(blockRowCount * blockColumnCount);

            if (options.BlockTraversalOrder == MatrixBlockTraversalOrder.LeftRightTopDown)
            {
                for (var bc = 0; bc < blockColumnCount; bc++)
                {
                    for (var br = 0; br < blockRowCount; br++)
                    {
                        var block = ExtractBlock(table, options, regionRowCount, regionColumnCount, blockRowSize, blockColumnSize, br, bc, fillMode);
                        blocks.Add(block);
                    }
                }
            }
            else
            {
                for (var br = 0; br < blockRowCount; br++)
                {
                    for (var bc = 0; bc < blockColumnCount; bc++)
                    {
                        var block = ExtractBlock(table, options, regionRowCount, regionColumnCount, blockRowSize, blockColumnSize, br, bc, fillMode);
                        blocks.Add(block);
                    }
                }
            }

            return blocks;
        }

        private static object[][] ExtractBlock(
            DataTable table,
            MatrixExportOptions options,
            int regionRowCount,
            int regionColumnCount,
            int blockRowSize,
            int blockColumnSize,
            int blockRowIndex,
            int blockColumnIndex,
            bool fillMode)
        {
            var startRow = options.StartRowIndex + (blockRowIndex * blockRowSize);
            var startColumn = options.StartColumnIndex + (blockColumnIndex * blockColumnSize);

            var totalRows = table.Rows.Count;
            var totalColumns = table.Columns.Count;

            int rowCount;
            int columnCount;

            if (fillMode)
            {
                rowCount = blockRowSize;
                columnCount = blockColumnSize;
            }
            else
            {
                var remainingRows = (options.StartRowIndex + regionRowCount) - startRow;
                var remainingColumns = (options.StartColumnIndex + regionColumnCount) - startColumn;

                rowCount = Math.Min(blockRowSize, remainingRows);
                columnCount = Math.Min(blockColumnSize, remainingColumns);
            }

            var result = new object[rowCount][];
            for (var r = 0; r < rowCount; r++)
            {
                var absoluteRow = startRow + r;
                var values = new object[columnCount];
                for (var c = 0; c < columnCount; c++)
                {
                    var absoluteColumn = startColumn + c;
                    var inRegion = absoluteRow >= options.StartRowIndex
                        && absoluteRow < options.StartRowIndex + regionRowCount
                        && absoluteColumn >= options.StartColumnIndex
                        && absoluteColumn < options.StartColumnIndex + regionColumnCount;
                    var inTable = absoluteRow >= 0
                        && absoluteRow < totalRows
                        && absoluteColumn >= 0
                        && absoluteColumn < totalColumns;

                    if (inRegion && inTable)
                    {
                        values[c] = table.Rows[absoluteRow][absoluteColumn];
                    }
                    else if (fillMode)
                    {
                        values[c] = GetDefaultPaddedValue(table, absoluteColumn);
                    }
                }

                result[r] = values;
            }

            return result;
        }

        private static object GetDefaultPaddedValue(DataTable table, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= table.Columns.Count)
            {
                return string.Empty;
            }

            var column = table.Columns[columnIndex];
            var type = column.DataType;

            if (type == typeof(string))
            {
                return string.Empty;
            }

            if (type == typeof(DateTime))
            {
                return new DateTime(1970, 1, 1);
            }

            if (type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(float)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(byte))
            {
                return 0;
            }

            return DBNull.Value;
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

        private static bool TryParseEightDigitNumericDate(string raw, out DateTime date)
        {
            date = default(DateTime);
            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }

            var trimmed = raw.Trim();
            if (trimmed.Length != 8)
            {
                return false;
            }

            for (var i = 0; i < trimmed.Length; i++)
            {
                if (trimmed[i] < '0' || trimmed[i] > '9')
                {
                    return false;
                }
            }

            int year;
            int month;
            int day;
            if (!int.TryParse(trimmed.Substring(0, 4), out year))
            {
                return false;
            }

            if (!int.TryParse(trimmed.Substring(4, 2), out month))
            {
                return false;
            }

            if (!int.TryParse(trimmed.Substring(6, 2), out day))
            {
                return false;
            }

            try
            {
                date = new DateTime(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetCellAddress(int rowIndex, int columnIndex)
        {
            if (rowIndex < 1)
            {
                rowIndex = 1;
            }

            if (columnIndex < 0)
            {
                columnIndex = 0;
            }

            var columnLetters = ColumnIndexToName(columnIndex);
            return columnLetters + rowIndex.ToString(CultureInfo.InvariantCulture);
        }

        private static void LogNumericAsDateWarning(ExcelImportSettings settings, List<ExcelImportLogEntry> logs, int rowIndex, int columnIndex, string columnName, string rawValue, DateTime parsedDate)
        {
            if (logs == null)
            {
                return;
            }

            var cellAddress = GetCellAddress(rowIndex, columnIndex);
            var dateText = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var key = settings.NumericAsDateI18nKey ?? "ExcelImporter.NumericAsDateWarning";
            var zh = string.Format(
                CultureInfo.InvariantCulture,
                "检测到单元格 {0} 的纯数字值 {1} 被系统识别为日期 {2}，请确认是否按日期格式导入。",
                cellAddress,
                rawValue,
                dateText);
            var en = string.Format(
                CultureInfo.InvariantCulture,
                "Detected that pure numeric value {1} in cell {0} is recognized as date {2}. Please confirm whether to import it as date.",
                cellAddress,
                rawValue,
                dateText);
            var message = key + "|" + zh + "|" + en;
            logs.Add(new ExcelImportLogEntry(rowIndex, columnIndex, columnName ?? string.Empty, message, rawValue));
        }

        private static DataTable ReadExcel(string filePath, ExcelImportSettings settings)
        {
            const int maxRetries = 3;
            const int retryDelayMilliseconds = 200;

            var attempt = 0;
            var stopwatch = Stopwatch.StartNew();
#if NET45
            Exception lastException = null;
#else
            Exception? lastException = null;
#endif

            while (attempt <= maxRetries)
            {
                try
                {
                    using (var workbook = ExcelWorkbookProvider.Current.Open(filePath))
                    {
                        var result = ReadExcelFromWorkbook(workbook, Path.GetFileName(filePath), settings);
                        stopwatch.Stop();
                        Trace.WriteLine(BuildFileOpenLogMessage(filePath, true, attempt, stopwatch.ElapsedMilliseconds));
                        return result;
                    }
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    attempt++;
                    if (attempt > maxRetries || !IsFileLockIOException(ex))
                    {
                        stopwatch.Stop();
                        var snapshot = BuildFileLockSnapshot(filePath, attempt, stopwatch.ElapsedMilliseconds, ex);
                        Trace.WriteLine(BuildFileOpenLogMessage(filePath, false, attempt, stopwatch.ElapsedMilliseconds, ex));
                        throw new ExcelImportException("Excel 文件在共享模式下打开失败", ex, -1, -1, snapshot, ExcelImportErrorCode.FileLocked);
                    }

                    Thread.Sleep(retryDelayMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Trace.WriteLine(BuildFileOpenLogMessage(filePath, false, attempt + 1, stopwatch.ElapsedMilliseconds, ex));
                    throw WrapAsExcelImportException("Excel 导入失败", ex);
                }
            }

            stopwatch.Stop();
            var safeException = lastException ?? new IOException("未知文件打开错误");
            var fallbackSnapshot = BuildFileLockSnapshot(filePath, attempt, stopwatch.ElapsedMilliseconds, safeException);
            Trace.WriteLine(BuildFileOpenLogMessage(filePath, false, attempt, stopwatch.ElapsedMilliseconds, safeException));
            throw new ExcelImportException("Excel 文件在共享模式下打开失败", safeException, -1, -1, fallbackSnapshot, ExcelImportErrorCode.FileLocked);
        }

        private static bool IsFileLockIOException(IOException ex)
        {
            var message = ex.Message ?? string.Empty;
            if (message.IndexOf("used by another process", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.Contains("因为它正由另一进程使用") ||
                message.Contains("由于另一进程正在使用该文件") ||
                message.Contains("正由另一进程使用"))
            {
                return true;
            }

            return false;
        }

        private static string BuildFileLockSnapshot(string filePath, int attempts, long elapsedMilliseconds, Exception ex)
        {
            var fullPath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
            var error = ex == null ? string.Empty : ex.Message ?? string.Empty;
            return "Path=" + fullPath + "; Attempts=" + attempts.ToString(CultureInfo.InvariantCulture) + "; ElapsedMs=" + elapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + "; Error=" + error;
        }

        private static string BuildFileOpenLogMessage(string filePath, bool success, int attempts, long elapsedMilliseconds)
        {
            var status = success ? "Success" : "Failure";
            var fullPath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
            return "[ExcelImporter] OpenFile " + status + " Path=" + fullPath + " Mode=Read,FileShare.ReadWrite Attempts=" + (attempts + 1).ToString(CultureInfo.InvariantCulture) + " ElapsedMs=" + elapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildFileOpenLogMessage(string filePath, bool success, int attempts, long elapsedMilliseconds, Exception ex)
        {
            var baseMessage = BuildFileOpenLogMessage(filePath, success, attempts, elapsedMilliseconds);
            var error = ex == null ? string.Empty : ex.Message ?? string.Empty;
            return string.IsNullOrEmpty(error) ? baseMessage : baseMessage + " Error=" + error;
        }

        private static DataTable ReadExcelFromWorkbook(IExcelWorkbook workbook, string tableName, ExcelImportSettings settings)
        {
            var sheet = GetTargetSheet(workbook, settings);
            var dt = new DataTable(string.IsNullOrEmpty(tableName) ? "Sheet" : tableName);

            if (settings.HasHeader && settings.HeaderRowIndex >= settings.DataRowIndex)
            {
                throw new ArgumentException("当 HasHeader 为 true 时必须满足 HeaderRowIndex < DataRowIndex", nameof(settings));
            }

            var rowCount = sheet.RowCount;
            var columnCount = sheet.ColumnCount;
            var headerProcessed = false;
            var colMap = new List<int>();
            var headerNames = new List<string>();
            var startRow = 0;

            if (settings.HasHeader && rowCount > settings.HeaderRowIndex)
            {
                headerNames = ReadRowValuesFromSheet(sheet, settings.HeaderRowIndex, columnCount);
                if (settings.IgnoreEmptyHeader && IsDefaultHeaderMapping(settings))
                {
                    var includedIndexes = new List<int>();
                    var filteredHeaderNames = new List<string>();
                    for (var c = 0; c < headerNames.Count; c++)
                    {
                        var text = headerNames[c];
                        var trimmed = text == null ? string.Empty : text.Trim();
                        if (trimmed.Length == 0)
                        {
                            continue;
                        }

                        includedIndexes.Add(c);
                        filteredHeaderNames.Add(trimmed);
                    }

                    BuildSchema(dt, filteredHeaderNames, settings);
                    BuildColumnMapForIgnoredHeaders(colMap, columnCount, includedIndexes);
                }
                else
                {
                    BuildSchema(dt, headerNames, settings);
                    BuildColumnMap(colMap, headerNames.Count, settings);
                }

                headerProcessed = true;
                startRow = settings.DataRowIndex;
            }

            if (!headerProcessed)
            {
                var firstRowIndex = settings.DataRowIndex < 0 ? 0 : settings.DataRowIndex;
                var firstRowValues = rowCount > firstRowIndex ? ReadRowValuesFromSheet(sheet, firstRowIndex, columnCount) : new List<string>();
                var cols = firstRowValues.Count > 0 ? firstRowValues.Count : columnCount;
                BuildSchemaNoHeader(dt, cols, settings);
                BuildColumnMap(colMap, cols, settings);
                headerProcessed = true;
                startRow = firstRowIndex;
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

                var index = 0;
                if (workbook is NpoiWorkbook fileWorkbook)
                {
                    index = fileWorkbook.ActiveSheetIndex;
                }
                else if (workbook is NpoiStreamWorkbook streamWorkbook)
                {
                    index = streamWorkbook.ActiveSheetIndex;
                }

                var names = workbook.GetSheetNames();
                if (names != null && names.Count > 0)
                {
                    if (index < 0 || index >= names.Count)
                    {
                        index = 0;
                    }
                }

                return workbook.GetWorksheet(index);
            }
            catch (Exception ex)
            {
                var snapshot = settings.SheetName ?? (settings.SheetIndex.HasValue ? settings.SheetIndex.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                throw new ExcelImportException("未找到目标工作表", ex, -1, -1, snapshot);
            }
        }

        private static bool IsDefaultHeaderMapping(ExcelImportSettings settings)
        {
            if (settings == null)
            {
                return true;
            }

            if (settings.HeaderReadMode != ExcelHeaderReadMode.None)
            {
                return false;
            }

            if (settings.DispersedMapByIndex != null && settings.DispersedMapByIndex.Count > 0)
            {
                return false;
            }

            if (settings.DispersedMapByLetter != null && settings.DispersedMapByLetter.Count > 0)
            {
                return false;
            }

            if (settings.StartColumnIndex.HasValue || !string.IsNullOrEmpty(settings.StartColumnName) || settings.ColumnCount.HasValue)
            {
                return false;
            }

            return true;
        }

        private static void BuildColumnMapForIgnoredHeaders(List<int> colMap, int totalSourceColumns, List<int> includedIndexes)
        {
            colMap.Clear();
            var indexToTarget = new Dictionary<int, int>();
            for (var i = 0; i < includedIndexes.Count; i++)
            {
                indexToTarget[includedIndexes[i]] = i;
            }

            for (var s = 0; s < totalSourceColumns; s++)
            {
                if (indexToTarget.TryGetValue(s, out var target))
                {
                    colMap.Add(target);
                }
                else
                {
                    colMap.Add(-1);
                }
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

            if (settings.HeaderReadMode == ExcelHeaderReadMode.HeaderByName)
            {
                var nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < headerRow.Count; i++)
                {
                    var rawName = headerRow[i] ?? string.Empty;
                    if (string.IsNullOrEmpty(rawName))
                    {
                        continue;
                    }

                    var canonicalName = rawName;
                    if (settings.HeaderRenameMapByName != null && settings.HeaderRenameMapByName.TryGetValue(rawName, out var mappedName))
                    {
                        canonicalName = mappedName ?? string.Empty;
                    }

                    if (string.IsNullOrEmpty(canonicalName))
                    {
                        continue;
                    }

                    if (!nameToIndex.ContainsKey(canonicalName))
                    {
                        nameToIndex[canonicalName] = i;
                    }
                    else
                    {
                        logs.Add(new ExcelImportLogEntry(1, i, canonicalName, "表头名称重复: " + canonicalName, rawName));
                    }
                }

                for (var i = 0; i < target.Columns.Count; i++)
                {
                    var column = target.Columns[i];
                    var columnName = column.ColumnName ?? string.Empty;
                    if (string.IsNullOrEmpty(columnName) || !nameToIndex.TryGetValue(columnName, out var srcIndex))
                    {
                        throw new ExcelImportException("在表头中未找到匹配的列名: " + columnName, new ArgumentException("target.Columns"), -1, -1, columnName);
                    }

                    bindings.Add(new ExcelColumnBinding(srcIndex, i));
                }

                return bindings;
            }

            if (settings.HeaderReadMode == ExcelHeaderReadMode.HeaderStartIndex)
            {
                if (!settings.HeaderStartColumnIndex.HasValue && string.IsNullOrEmpty(settings.HeaderStartColumnName))
                {
                    throw new ExcelImportException("HeaderReadMode 为 HeaderStartIndex 时必须提供 HeaderStartColumnIndex 或 HeaderStartColumnName", new ArgumentNullException("HeaderStartColumnIndex"), -1, -1, string.Empty);
                }

                var start = settings.HeaderStartColumnIndex.HasValue
                    ? settings.HeaderStartColumnIndex.Value
                    : ColumnNameToIndex(settings.HeaderStartColumnName);

                if (!settings.HeaderStartColumnIndex.HasValue && !string.IsNullOrEmpty(settings.HeaderStartColumnName))
                {
                    System.Diagnostics.Trace.TraceWarning("ExcelImportSettings.HeaderStartColumnName is specified without HeaderStartColumnIndex. Using columnName as primary column locator.");
                }
                else if (settings.HeaderStartColumnIndex.HasValue && !string.IsNullOrEmpty(settings.HeaderStartColumnName))
                {
                    System.Diagnostics.Trace.TraceWarning("Both HeaderStartColumnIndex and HeaderStartColumnName are specified. HeaderStartColumnName will be used as primary column locator.");
                    start = ColumnNameToIndex(settings.HeaderStartColumnName);
                }

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

        private static object ConvertToColumnType(object value, Type targetType, ExcelImportSettings settings, List<ExcelImportLogEntry> logs, int rowIndex, int columnIndex, string columnName, string raw)
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

            var rawString = raw ?? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            var hasNumericDateCandidate = TryParseEightDigitNumericDate(rawString, out DateTime numericDateCandidate);
            var skipNumericDateParsing = hasNumericDateCandidate && settings != null && !settings.AcceptNumericAsDate;
            var acceptNumericDateAsDate = hasNumericDateCandidate && settings != null && settings.AcceptNumericAsDate && underlying == typeof(DateTime);

            try
            {
                if (hasNumericDateCandidate && settings != null)
                {
                    LogNumericAsDateWarning(settings, logs, rowIndex, columnIndex, columnName, rawString, numericDateCandidate);
                    if (acceptNumericDateAsDate)
                    {
                        return numericDateCandidate;
                    }
                }

                if (underlying == typeof(DateTime))
                {
                    if (skipNumericDateParsing)
                    {
                        return value;
                    }

                    if (value is DateTime dt)
                    {
                        return dt;
                    }

                    if (value is double d)
                    {
                        return DateTime.FromOADate(d);
                    }

                    if (value is float f)
                    {
                        return DateTime.FromOADate(f);
                    }

                    if (value is decimal dec)
                    {
                        return DateTime.FromOADate((double)dec);
                    }

                    var dateString = Convert.ToString(value, CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                        {
                            return parsed;
                        }

                        if (double.TryParse(dateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double oa))
                        {
                            return DateTime.FromOADate(oa);
                        }
                    }
                }

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
                var rawValue = rawString;
                logs.Add(new ExcelImportLogEntry(rowIndex, columnIndex, columnName, "数据类型转换失败: " + ex.Message, rawValue));
                return DBNull.Value;
            }
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
#if NET45
                    string cellValue = null;
#else
                    string? cellValue = null;
#endif

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
                        var path = (!string.IsNullOrEmpty(id) && idToPath.TryGetValue(id, out var mappedPath)) ? mappedPath : string.Empty;

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
            if (settings.DispersedMapByLetter != null && settings.DispersedMapByLetter.Count > 0 && settings.HeaderReadMode == ExcelHeaderReadMode.None && (settings.DispersedMapByIndex == null || settings.DispersedMapByIndex.Count == 0) && !settings.StartColumnIndex.HasValue && string.IsNullOrEmpty(settings.StartColumnName))
            {
                var mappings = NormalizeDispersedMapByLetter(settings, headerNames.Count);
                for (var i = 0; i < mappings.Count; i++)
                {
                    dt.Columns.Add(mappings[i].Value, typeof(object));
                }

                return;
            }

            var cols = headerNames.Count;
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cols; i++)
            {
                var original = headerNames[i];
                var trimmed = original == null ? string.Empty : original.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    usedNames.Add(trimmed);
                }
            }

            var prefix = ResolveHeaderPrefix(settings);

            for (var i = 0; i < cols; i++)
            {
                var raw = headerNames[i];
                var name = raw == null ? string.Empty : raw.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    var baseName = prefix + (i + 1).ToString(CultureInfo.InvariantCulture);
                    var candidate = baseName;
                    var suffix = 1;
                    while (usedNames.Contains(candidate))
                    {
                        candidate = baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture);
                        suffix++;
                    }

                    name = candidate;
                    headerNames[i] = name;
                    usedNames.Add(name);
                }

                if (settings.HeaderRenameMapByIndex.TryGetValue(i, out var renamedByIndex))
                {
                    name = renamedByIndex;
                }
                else if (!string.IsNullOrEmpty(name) && settings.HeaderRenameMapByName.TryGetValue(name, out var renamedByName))
                {
                    name = renamedByName;
                }

                dt.Columns.Add(name, typeof(object));
            }
        }

        private static string ResolveHeaderPrefix(ExcelImportSettings settings)
        {
            string prefix = settings.HeaderPrefix ?? string.Empty;

            if (settings.HeaderPrefixI18nMap != null && settings.HeaderPrefixI18nMap.Count > 0)
            {
                var culture = CultureInfo.CurrentUICulture;
                var candidates = new List<string>
                {
                    culture.Name,
                    culture.TwoLetterISOLanguageName,
                };

                candidates.Add("default");

                foreach (var key in candidates)
                {
                    if (settings.HeaderPrefixI18nMap.TryGetValue(key, out var mapped) && !string.IsNullOrEmpty(mapped))
                    {
                        prefix = mapped;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "Col";
            }

            if (ContainsInvalidHeaderPrefixChar(prefix))
            {
                throw new ArgumentException("HeaderPrefix contains invalid characters (: \\\\/ ? * [ ])", "HeaderPrefix");
            }

            return prefix;
        }

        private static bool ContainsInvalidHeaderPrefixChar(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            return prefix.IndexOfAny(invalid) >= 0;
        }

        private static void BuildSchemaNoHeader(DataTable dt, int cols, ExcelImportSettings settings)
        {
            if (settings.DispersedMapByLetter != null && settings.DispersedMapByLetter.Count > 0)
            {
                var mappings = NormalizeDispersedMapByLetter(settings, cols);
                for (var i = 0; i < mappings.Count; i++)
                {
                    dt.Columns.Add(mappings[i].Value, typeof(object));
                }

                return;
            }

            if (settings.DispersedMapByIndex.Count == 0 && (settings.DispersedMapByLetter == null || settings.DispersedMapByLetter.Count == 0) && !settings.StartColumnIndex.HasValue && string.IsNullOrEmpty(settings.StartColumnName))
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

        private static List<KeyValuePair<int, string>> NormalizeDispersedMapByLetter(ExcelImportSettings settings, int maxSourceColumns)
        {
            var result = new List<KeyValuePair<int, string>>();
            if (settings.DispersedMapByLetter == null || settings.DispersedMapByLetter.Count == 0)
            {
                return result;
            }

            var usedSourceIndexes = new HashSet<int>();
            foreach (var kv in settings.DispersedMapByLetter)
            {
                if (kv.Key == null)
                {
                    continue;
                }

                var sourceIndex = ColumnNameToIndex(kv.Key);
                if (sourceIndex < 0 || sourceIndex >= maxSourceColumns)
                {
                    throw new ExcelImportException(
                        "DispersedMapByLetter 中的列字母超出源列范围: " + kv.Key,
                        new ArgumentOutOfRangeException("DispersedMapByLetter"),
                        -1,
                        sourceIndex,
                        kv.Key);
                }

                if (!usedSourceIndexes.Add(sourceIndex))
                {
                    throw new ExcelImportException(
                        "DispersedMapByLetter 中存在重复的列映射: " + kv.Key,
                        new InvalidOperationException("DuplicateDispersedLetter"),
                        -1,
                        sourceIndex,
                        kv.Key);
                }

                result.Add(new KeyValuePair<int, string>(sourceIndex, kv.Value));
            }

            result.Sort((x, y) => x.Key.CompareTo(y.Key));
            return result;
        }

        private static void BuildColumnMap(List<int> colMap, int srcCols, ExcelImportSettings settings)
        {
            colMap.Clear();
            if (settings.DispersedMapByLetter != null && settings.DispersedMapByLetter.Count > 0)
            {
                var mappings = NormalizeDispersedMapByLetter(settings, srcCols);
                var sourceToTarget = new Dictionary<int, int>();
                for (var i = 0; i < mappings.Count; i++)
                {
                    sourceToTarget[mappings[i].Key] = i;
                }

                for (var i = 0; i < srcCols; i++)
                {
                    if (sourceToTarget.TryGetValue(i, out var target))
                    {
                        colMap.Add(target);
                    }
                    else
                    {
                        colMap.Add(-1);
                    }
                }

                return;
            }

            var names = ResolveTargetNames(srcCols, settings);
            for (var i = 0; i < srcCols; i++)
            {
                colMap.Add(i < names.Length ? i : -1);
            }
        }

        private static string[] ResolveTargetNames(int srcCols, ExcelImportSettings settings)
        {
            if (settings.ColumnCount.HasValue && (settings.StartColumnIndex.HasValue || !string.IsNullOrEmpty(settings.StartColumnName)))
            {
                var start = settings.StartColumnIndex.HasValue
                    ? settings.StartColumnIndex.Value
                    : ColumnNameToIndex(settings.StartColumnName);

                if (!settings.StartColumnIndex.HasValue && !string.IsNullOrEmpty(settings.StartColumnName))
                {
                    System.Diagnostics.Trace.TraceWarning("ExcelImportSettings.StartColumnName is specified without StartColumnIndex. Using columnName as primary column locator.");
                }
                else if (settings.StartColumnIndex.HasValue && !string.IsNullOrEmpty(settings.StartColumnName))
                {
                    System.Diagnostics.Trace.TraceWarning("Both StartColumnIndex and StartColumnName are specified. StartColumnName will be used as primary column locator.");
                    start = ColumnNameToIndex(settings.StartColumnName);
                }

                var names = new List<string>();
                for (var i = 0; i < settings.ColumnCount.Value; i++)
                {
                    names.Add("Col" + (start + i).ToString(CultureInfo.InvariantCulture));
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
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d))
                {
                    return d;
                }

                return DBNull.Value;
            }

            if (settings.EnableBracketNegative && raw.Length > 2 && raw[0] == '(' && raw[raw.Length - 1] == ')')
            {
                var inner = raw.Substring(1, raw.Length - 2).Trim();
                if (decimal.TryParse(inner, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d1))
                {
                    if (settings.BracketAsNumeric)
                    {
                        return (object)(-d1);
                    }

                    if (settings.BracketNegativeDefaultValue.HasValue)
                    {
                        return settings.BracketNegativeDefaultValue.Value;
                    }

                    return raw;
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
                        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal dv))
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

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal dn))
            {
                return dn;
            }

            return raw;
        }

        private static ExcelBlockMergeResult ImportExcelBlocksAndMergeCore(DataTable source, MatrixExportOptions options, ExcelBlockMergeOptions mergeOptions, string tableName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            var logs = new List<ExcelImportLogEntry>();

            var totalColumns = source.Columns.Count;
            var regionColumnCount = 0;
            if (totalColumns > 0 && options.StartColumnIndex < totalColumns)
            {
                var maxColumnCount = totalColumns - options.StartColumnIndex;
                regionColumnCount = options.ColumnCount.HasValue && options.ColumnCount.Value > 0
                    ? Math.Min(options.ColumnCount.Value, maxColumnCount)
                    : maxColumnCount;
            }

            var totalRows = source.Rows.Count;
            var regionRowCount = 0;
            if (totalRows > 0 && options.StartRowIndex < totalRows)
            {
                var maxRowCount = totalRows - options.StartRowIndex;
                regionRowCount = options.RowCount.HasValue && options.RowCount.Value > 0
                    ? Math.Min(options.RowCount.Value, maxRowCount)
                    : maxRowCount;
            }

            if (regionRowCount <= 0 || regionColumnCount <= 0)
            {
                var emptyTable = new DataTable(string.IsNullOrEmpty(tableName) ? "MergedBlocks" : tableName);
                logs.Add(new ExcelImportLogEntry(-1, -1, string.Empty, "Block merge skipped because effective region is empty.", string.Empty));
                var emptyStatistics = new ExcelBlockMergeStatistics();
                return new ExcelBlockMergeResult(emptyTable, logs, emptyStatistics);
            }

            var blocks = ConvertDataTableToBlocks(source, options);

            var targetColumnCount = regionColumnCount;
            if (mergeOptions != null && mergeOptions.ConflictStrategy == ExcelBlockMergeConflictStrategy.Append)
            {
                if (options.BlockColumnCount.HasValue && options.BlockColumnCount.Value > 0)
                {
                    targetColumnCount = Math.Min(options.BlockColumnCount.Value, regionColumnCount);
                }
            }

            var target = new DataTable(string.IsNullOrEmpty(tableName) ? "MergedBlocks" : tableName);
            if (targetColumnCount > 0)
            {
                for (var i = 0; i < targetColumnCount; i++)
                {
                    var srcColumn = source.Columns[options.StartColumnIndex + i];
                    target.Columns.Add(srcColumn.ColumnName, srcColumn.DataType);
                }
            }

            source.Clear();
            source.Dispose();

            var stopwatch = Stopwatch.StartNew();
            logs.Add(new ExcelImportLogEntry(-1, -1, string.Empty, "Block merge started.", string.Empty));

            var statistics = MergeBlocksIntoDataTable(blocks, mergeOptions, target, logs, regionRowCount, options);

            stopwatch.Stop();
            statistics.MergeElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            logs.Add(new ExcelImportLogEntry(-1, -1, string.Empty, "Block merge completed.", string.Empty));

            return new ExcelBlockMergeResult(target, logs, statistics);
        }

#if NET45
        private static ExcelBlockMergeStatistics MergeBlocksIntoDataTable(IReadOnlyList<object[][]> blocks, ExcelBlockMergeOptions mergeOptions, DataTable target, List<ExcelImportLogEntry> logs, int regionRowCount, MatrixExportOptions options)
#else
        private static ExcelBlockMergeStatistics MergeBlocksIntoDataTable(IReadOnlyList<object[][]> blocks, ExcelBlockMergeOptions? mergeOptions, DataTable target, List<ExcelImportLogEntry> logs, int regionRowCount, MatrixExportOptions options)
#endif
        {
            var statistics = new ExcelBlockMergeStatistics();

            if (blocks == null || target == null)
            {
                return statistics;
            }

            statistics.TotalBlocks = blocks.Count;

            if (blocks.Count == 0 || target.Columns.Count == 0)
            {
                return statistics;
            }

            var columns = target.Columns.Count;

            if (mergeOptions != null
                && mergeOptions.ConflictStrategy == ExcelBlockMergeConflictStrategy.Append)
            {
                MergeBlocksByRowAppend(blocks, target, logs, statistics, columns);
                return statistics;
            }

            var keyColumnIndexes = BuildKeyColumnIndexes(target, mergeOptions);
            var comparer = new RowKeyComparer();
            var rowIndexByKey = new Dictionary<object[], int>(comparer);

            target.BeginLoadData();
            try
            {
                for (var b = 0; b < blocks.Count; b++)
                {
                    var block = blocks[b];
                    if (block == null)
                    {
                        continue;
                    }

                    for (var r = 0; r < block.Length; r++)
                    {
                        var rowValues = block[r];
                        if (rowValues == null)
                        {
                            continue;
                        }

                        var values = new object[columns];
                        for (var c = 0; c < columns; c++)
                        {
                            if (c < rowValues.Length)
                            {
                                values[c] = rowValues[c] ?? DBNull.Value;
                            }
                            else
                            {
                                values[c] = DBNull.Value;
                            }
                        }

                        var key = BuildRowKey(values, keyColumnIndexes);
                        if (rowIndexByKey.TryGetValue(key, out var existingIndex))
                        {
                            statistics.DuplicateRowCount++;
                            if (mergeOptions == null)
                            {
                                continue;
                            }

                            if (mergeOptions.ConflictStrategy == ExcelBlockMergeConflictStrategy.Ignore)
                            {
                                continue;
                            }

                            if (mergeOptions.ConflictStrategy == ExcelBlockMergeConflictStrategy.Overwrite)
                            {
                                var existingRow = target.Rows[existingIndex];
                                for (var c = 0; c < columns; c++)
                                {
                                    existingRow[c] = values[c];
                                }

                                logs.Add(new ExcelImportLogEntry(existingIndex, -1, string.Empty, "Duplicate row overwritten during merge.", string.Empty));
                                continue;
                            }

                            logs.Add(new ExcelImportLogEntry(existingIndex, -1, string.Empty, "Duplicate row appended during merge.", string.Empty));
                        }

                        var newRow = target.NewRow();
                        for (var c = 0; c < columns; c++)
                        {
                            newRow[c] = values[c];
                        }

                        target.Rows.Add(newRow);

                        var storedKey = BuildRowKey(values, keyColumnIndexes);
                        rowIndexByKey[storedKey] = target.Rows.Count - 1;
                    }

                    statistics.SuccessfulBlocks++;
                }
            }
            finally
            {
                target.EndLoadData();
            }

            return statistics;
        }

        private static void MergeBlocksByRowAppend(IReadOnlyList<object[][]> blocks, DataTable target, List<ExcelImportLogEntry> logs, ExcelBlockMergeStatistics statistics, int expectedColumnCount)
        {
            if (blocks == null || blocks.Count == 0 || target.Columns.Count == 0)
            {
                return;
            }

            var canonicalColumnCount = expectedColumnCount;
            var totalRowCount = 0;

#if NET45
            object[][] firstNonEmptyBlock = null;
#else
            object[][]? firstNonEmptyBlock = null;
#endif
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null || block.Length == 0)
                {
                    continue;
                }

                firstNonEmptyBlock = block;
                break;
            }

            if (firstNonEmptyBlock == null)
            {
                return;
            }

            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null)
                {
                    continue;
                }

                totalRowCount += block.Length;
            }

            target.BeginLoadData();
            try
            {
                for (var b = 0; b < blocks.Count; b++)
                {
                    var block = blocks[b];
                    if (block == null || block.Length == 0)
                    {
                        continue;
                    }

                    var hasRowInBlock = false;

                    for (var r = 0; r < block.Length; r++)
                    {
                        var rowValues = block[r];
                        if (rowValues == null)
                        {
                            continue;
                        }

                        if (rowValues.Length != canonicalColumnCount)
                        {
                            var message = "Block row column count does not match expected column count during append merge.";
                            logs.Add(new ExcelImportLogEntry(-1, -1, string.Empty, message, string.Empty));
                            throw new ExcelImportException(message, new InvalidOperationException("ColumnCountMismatch"), -1, -1, string.Empty);
                        }

                        var newRow = target.NewRow();
                        for (var c = 0; c < canonicalColumnCount; c++)
                        {
                            var value = c < rowValues.Length ? rowValues[c] : DBNull.Value;
                            newRow[c] = value ?? DBNull.Value;
                        }

                        target.Rows.Add(newRow);
                        hasRowInBlock = true;
                    }

                    if (hasRowInBlock)
                    {
                        statistics.SuccessfulBlocks++;
                    }
                }
            }
            finally
            {
                target.EndLoadData();
            }
        }

        private static void MergeBlocksByRegionAppend(IReadOnlyList<object[][]> blocks, DataTable target, int regionRowCount, int regionColumnCount, MatrixExportOptions options)
        {
            if (blocks == null || blocks.Count == 0)
            {
                return;
            }

            if (regionRowCount <= 0 || regionColumnCount <= 0)
            {
                return;
            }

            while (target.Rows.Count < regionRowCount)
            {
                target.Rows.Add(target.NewRow());
            }

            var blockRowSize = options.BlockRowCount.HasValue && options.BlockRowCount.Value > 0
                ? options.BlockRowCount.Value
                : regionRowCount;

            var blockColumnSize = options.BlockColumnCount.HasValue && options.BlockColumnCount.Value > 0
                ? options.BlockColumnCount.Value
                : regionColumnCount;

            if (blockRowSize <= 0 || blockColumnSize <= 0)
            {
                return;
            }

            var rowRemainder = regionRowCount % blockRowSize;
            var columnRemainder = regionColumnCount % blockColumnSize;
            var evenlySplit = rowRemainder == 0 && columnRemainder == 0;

            var fillMode = false;
            if (!evenlySplit)
            {
                var mode = options.RemainderMode ?? DefaultRemainderMode;
                if (mode == MatrixRemainderMode.Fill)
                {
                    fillMode = true;
                }
                else if (mode == MatrixRemainderMode.Prompt)
                {
                    fillMode = true;
                }
            }

            int blockRowCount;
            int blockColumnCount;

            if (fillMode)
            {
                var paddedRowCount = ((regionRowCount + blockRowSize - 1) / blockRowSize) * blockRowSize;
                var paddedColumnCount = ((regionColumnCount + blockColumnSize - 1) / blockColumnSize) * blockColumnSize;
                blockRowCount = paddedRowCount / blockRowSize;
                blockColumnCount = paddedColumnCount / blockColumnSize;
            }
            else
            {
                blockRowCount = regionRowCount / blockRowSize;
                blockColumnCount = regionColumnCount / blockColumnSize;
            }

            if (blockRowCount <= 0 || blockColumnCount <= 0)
            {
                return;
            }

            if (blockRowCount * blockColumnCount != blocks.Count)
            {
                if (blockRowCount > 0)
                {
                    blockColumnCount = blocks.Count / blockRowCount;
                    if (blockColumnCount <= 0)
                    {
                        return;
                    }
                }
            }

            for (var index = 0; index < blocks.Count; index++)
            {
                var block = blocks[index];
                if (block == null)
                {
                    continue;
                }

                int blockRowIndex;
                int blockColumnIndex;

                if (options.BlockTraversalOrder == MatrixBlockTraversalOrder.LeftRightTopDown)
                {
                    blockColumnIndex = index / blockRowCount;
                    blockRowIndex = index % blockRowCount;
                }
                else
                {
                    blockRowIndex = index / blockColumnCount;
                    blockColumnIndex = index % blockColumnCount;
                }

                for (var r = 0; r < block.Length; r++)
                {
                    var rowValues = block[r];
                    if (rowValues == null)
                    {
                        continue;
                    }

                    var destRow = (blockRowIndex * blockRowSize) + r;
                    if (destRow < 0 || destRow >= regionRowCount)
                    {
                        continue;
                    }

                    var dataRow = target.Rows[destRow];

                    for (var c = 0; c < rowValues.Length; c++)
                    {
                        var destColumn = (blockColumnIndex * blockColumnSize) + c;
                        if (destColumn < 0 || destColumn >= regionColumnCount)
                        {
                            continue;
                        }

                        dataRow[destColumn] = rowValues[c] ?? DBNull.Value;
                    }
                }
            }
        }

#if NET45
        private static int[] BuildKeyColumnIndexes(DataTable target, ExcelBlockMergeOptions mergeOptions)
#else
        private static int[] BuildKeyColumnIndexes(DataTable target, ExcelBlockMergeOptions? mergeOptions)
#endif
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var columnCount = target.Columns.Count;
            if (columnCount == 0)
            {
                return new int[0];
            }

#if NET45
            string[] keyColumns;
            if (mergeOptions == null || mergeOptions.DuplicateKeyColumnNames == null || mergeOptions.DuplicateKeyColumnNames.Length == 0)
#else
            string[]? keyColumns;
            if (mergeOptions == null || mergeOptions.DuplicateKeyColumnNames == null || mergeOptions.DuplicateKeyColumnNames.Length == 0)
#endif
            {
                keyColumns = null;
            }
            else
            {
                keyColumns = mergeOptions.DuplicateKeyColumnNames;
            }

            if (keyColumns == null)
            {
                var all = new int[columnCount];
                for (var i = 0; i < columnCount; i++)
                {
                    all[i] = i;
                }

                return all;
            }

            var nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < columnCount; i++)
            {
                var name = target.Columns[i].ColumnName ?? string.Empty;
                if (!nameToIndex.ContainsKey(name))
                {
                    nameToIndex[name] = i;
                }
            }

            var indexes = new List<int>(keyColumns.Length);
            for (var i = 0; i < keyColumns.Length; i++)
            {
                var keyName = keyColumns[i] ?? string.Empty;
                if (!nameToIndex.TryGetValue(keyName, out var idx))
                {
                    throw new ExcelImportException("Merge key column not found: " + keyName, new ArgumentException("DuplicateKeyColumnNames"), -1, -1, keyName);
                }

                indexes.Add(idx);
            }

            return indexes.ToArray();
        }

        private static object[] BuildRowKey(object[] values, int[] keyColumnIndexes)
        {
            if (keyColumnIndexes == null || keyColumnIndexes.Length == 0)
            {
                var copy = new object[values.Length];
                for (var i = 0; i < values.Length; i++)
                {
                    copy[i] = values[i];
                }

                return copy;
            }

            var key = new object[keyColumnIndexes.Length];
            for (var i = 0; i < keyColumnIndexes.Length; i++)
            {
                var idx = keyColumnIndexes[i];
                key[i] = idx >= 0 && idx < values.Length ? values[idx] : DBNull.Value;
            }

            return key;
        }
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
