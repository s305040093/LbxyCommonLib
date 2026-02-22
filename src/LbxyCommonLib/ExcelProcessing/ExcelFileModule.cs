#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1402
#pragma warning disable SA1602
#pragma warning disable SA1201
#pragma warning disable SA1513
#pragma warning disable CS8603

namespace LbxyCommonLib.ExcelProcessing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;

    /// <summary>
    /// Excel 文件格式类型，用于区分不同的工作簿物理存储格式。
    /// </summary>
    public enum ExcelFileFormat
    {
        /// <summary>
        /// 未知格式，无法从扩展名或文件头推断出具体类型。
        /// </summary>
        Unknown,

        /// <summary>
        /// Office 2007 及以上版本的基于 OpenXML 的 .xlsx 格式。
        /// </summary>
        Xlsx,

        /// <summary>
        /// 早期二进制格式 .xls。
        /// </summary>
        Xls,

        /// <summary>
        /// 含有宏的基于 OpenXML 的 .xlsm 格式。
        /// </summary>
        Xlsm,
    }

    /// <summary>
    /// 表示 Excel 文件的基本信息，包括路径、实际检测到的格式以及扩展名推断的格式。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class ExcelFileInfo
    {
        /// <summary>
        /// 初始化 <see cref="ExcelFileInfo"/> 类型的新实例。
        /// </summary>
        /// <param name="path">Excel 文件的完整路径。</param>
        /// <param name="format">根据文件头和扩展名综合推断出的实际格式。</param>
        /// <param name="extensionFormat">仅根据扩展名推断出的格式。</param>
        public ExcelFileInfo(string path, ExcelFileFormat format, ExcelFileFormat extensionFormat)
        {
            Path = path;
            Format = format;
            ExtensionFormat = extensionFormat;
        }

        /// <summary>
        /// 获取 Excel 文件的完整路径。
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 获取根据文件头和扩展名综合推断出的实际格式。
        /// </summary>
        public ExcelFileFormat Format { get; }

        /// <summary>
        /// 获取仅根据扩展名推断出的格式。
        /// </summary>
        public ExcelFileFormat ExtensionFormat { get; }
    }

    /// <summary>
    /// 在 Excel 文件检测或读取过程中发生错误时抛出的异常类型。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class ExcelProcessingException : Exception
    {
        /// <summary>
        /// 使用指定错误消息初始化 <see cref="ExcelProcessingException"/> 的新实例。
        /// </summary>
        /// <param name="message">描述错误原因的消息。</param>
        public ExcelProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 使用指定错误消息和内部异常初始化 <see cref="ExcelProcessingException"/> 的新实例。
        /// </summary>
        /// <param name="message">描述错误原因的消息。</param>
        /// <param name="innerException">导致当前异常的内部异常实例。</param>
        public ExcelProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 提供 Excel 文件格式检测功能，可基于文件扩展名和魔数进行格式识别并缓存结果。
    /// </summary>
    public static class ExcelFileDetector
    {
        private static readonly ConcurrentDictionary<string, ExcelFileInfo> Cache = new ConcurrentDictionary<string, ExcelFileInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 检测指定路径下的 Excel 文件，并返回包含格式信息的 <see cref="ExcelFileInfo"/>。
        /// </summary>
        /// <param name="path">Excel 文件路径，可以是相对或绝对路径。</param>
        /// <returns>包含文件路径、实际格式和扩展名格式的 <see cref="ExcelFileInfo"/> 实例。</returns>
        /// <exception cref="ExcelProcessingException">当路径为空、文件不存在或读取头部数据时发生错误时抛出。</exception>
        public static ExcelFileInfo Detect(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ExcelProcessingException("文件路径为空");
            }

            path = Path.GetFullPath(path);
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                throw new ExcelProcessingException("文件不存在: " + path);
            }

            var key = path + "|" + info.Length.ToString() + "|" + info.LastWriteTimeUtc.Ticks.ToString();
            ExcelFileInfo cached;
            if (Cache.TryGetValue(key, out cached))
            {
                return cached;
            }

            var extensionFormat = GetFormatFromExtension(Path.GetExtension(path));
            var headerFormat = GetFormatFromHeader(path);
            var finalFormat = headerFormat;
            if (finalFormat == ExcelFileFormat.Unknown && extensionFormat != ExcelFileFormat.Unknown)
            {
                finalFormat = extensionFormat;
            }

            var result = new ExcelFileInfo(path, finalFormat, extensionFormat);
            Cache[key] = result;
            return result;
        }

        private static ExcelFileFormat GetFormatFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return ExcelFileFormat.Unknown;
            }

            var ext = extension.ToLowerInvariant();
            if (ext == ".xlsx")
            {
                return ExcelFileFormat.Xlsx;
            }

            if (ext == ".xls")
            {
                return ExcelFileFormat.Xls;
            }

            if (ext == ".xlsm")
            {
                return ExcelFileFormat.Xlsm;
            }

            return ExcelFileFormat.Unknown;
        }

        private static ExcelFileFormat GetFormatFromHeader(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var header = new byte[8];
                var read = fs.Read(header, 0, header.Length);
                if (read < 8)
                {
                    return ExcelFileFormat.Unknown;
                }

                if (header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0 && header[4] == 0xA1 && header[5] == 0xB1 && header[6] == 0x1A && header[7] == 0xE1)
                {
                    return ExcelFileFormat.Xls;
                }

                if (header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
                {
                    var ext = Path.GetExtension(path)?.ToLowerInvariant();
                    if (ext == ".xlsm")
                    {
                        return ExcelFileFormat.Xlsm;
                    }

                    return ExcelFileFormat.Xlsx;
                }

                return ExcelFileFormat.Unknown;
            }
        }
    }

    /// <summary>
    /// 抽象表示一个 Excel 工作簿，提供工作表访问与 VBA 项目读取等能力。
    /// </summary>
    public interface IExcelWorkbook : IDisposable
    {
        /// <summary>
        /// 获取工作簿的物理格式类型。
        /// </summary>
        ExcelFileFormat Format { get; }

        /// <summary>
        /// 获取工作簿对应的路径或名称（对于流创建的工作簿可能为逻辑名称）。
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 获取工作簿中所有工作表名称的只读列表。
        /// </summary>
        IReadOnlyList<string> GetSheetNames();

        /// <summary>
        /// 按索引（从 0 开始）获取工作表。
        /// </summary>
        /// <param name="index">工作表索引，从 0 开始。</param>
        /// <returns>对应索引的 <see cref="IExcelWorksheet"/> 实例。</returns>
        IExcelWorksheet GetWorksheet(int index);

        /// <summary>
        /// 按名称获取工作表。
        /// </summary>
        /// <param name="name">工作表名称。</param>
        /// <returns>名称匹配的 <see cref="IExcelWorksheet"/> 实例。</returns>
        IExcelWorksheet GetWorksheet(string name);

        /// <summary>
        /// 获取包含 VBA 项目二进制内容的字节数组，仅对 .xlsm 文件有效。
        /// </summary>
        /// <returns>VBA 项目二进制内容；当文件不包含 VBA 项目时返回空数组。</returns>
        byte[] GetVbaProjectBytes();
    }

    /// <summary>
    /// 抽象表示一个 Excel 工作表，提供行列信息和单元格读取能力。
    /// </summary>
    public interface IExcelWorksheet
    {
        /// <summary>
        /// 获取工作表名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取工作表的总行数（从 1 开始计数）。
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// 获取工作表最大列数（从 1 开始计数）。
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// 获取指定行列位置的单元格值。
        /// </summary>
        /// <param name="rowIndex">行索引，从 0 开始。</param>
        /// <param name="columnIndex">列索引，从 0 开始。</param>
        /// <param name="evaluateFormula">是否对公式单元格进行求值。</param>
        /// <returns>单元格的原始或求值后的 .NET 值，例如 bool、double 或 string；空单元格返回 null。</returns>
        object GetCellValue(int rowIndex, int columnIndex, bool evaluateFormula);
    }

    /// <summary>
    /// 定义工作簿提供程序接口，用于从路径或流创建 <see cref="IExcelWorkbook"/> 实例。
    /// </summary>
    public interface IExcelWorkbookProvider
    {
        /// <summary>
        /// 从文件路径打开 Excel 工作簿。
        /// </summary>
        /// <param name="path">Excel 文件路径。</param>
        /// <returns>打开的 <see cref="IExcelWorkbook"/> 实例。</returns>
        IExcelWorkbook Open(string path);

        /// <summary>
        /// 从流打开 Excel 工作簿。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取流。</param>
        /// <param name="name">逻辑名称或显示用名称。</param>
        /// <returns>打开的 <see cref="IExcelWorkbook"/> 实例。</returns>
        IExcelWorkbook Open(Stream stream, string name);
    }

    /// <summary>
    /// 全局工作簿提供程序入口，默认使用基于 NPOI 的 <see cref="XlsxAdapter"/> 实现。
    /// </summary>
    public static class ExcelWorkbookProvider
    {
        private static IExcelWorkbookProvider current = new XlsxAdapter();

        /// <summary>
        /// 获取或设置当前全局的 <see cref="IExcelWorkbookProvider"/> 实例。
        /// </summary>
        public static IExcelWorkbookProvider Current
        {
            get
            {
                return current;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                current = value;
            }
        }
    }

    /// <summary>
    /// 根据文件内容自动检测 Excel 格式并创建对应的 <see cref="IExcelWorkbook"/> 实例。
    /// </summary>
    public static class ExcelWorkbookFactory
    {
        /// <summary>
        /// 打开指定路径的 Excel 文件，并返回对应格式的 <see cref="IExcelWorkbook"/>。
        /// </summary>
        /// <param name="path">Excel 文件路径。</param>
        /// <returns>与文件格式匹配的 <see cref="IExcelWorkbook"/> 实例。</returns>
        /// <exception cref="ExcelProcessingException">当文件格式无法识别或不受支持时抛出。</exception>
        public static IExcelWorkbook Open(string path)
        {
            var info = ExcelFileDetector.Detect(path);
            if (info.Format == ExcelFileFormat.Unknown)
            {
                throw new ExcelProcessingException("无法识别的Excel文件格式: " + path);
            }

            if (info.Format == ExcelFileFormat.Xls)
            {
                return new NpoiWorkbook(path, ExcelFileFormat.Xls);
            }

            if (info.Format == ExcelFileFormat.Xlsx || info.Format == ExcelFileFormat.Xlsm)
            {
                return new NpoiWorkbook(path, info.Format);
            }

            throw new ExcelProcessingException("不支持的Excel文件格式: " + info.Format.ToString());
        }
    }

    /// <summary>
    /// 默认的工作簿提供程序实现，基于 <see cref="ExcelWorkbookFactory"/>，支持从路径或流打开工作簿。
    /// </summary>
    public sealed class XlsxAdapter : IExcelWorkbookProvider
    {
        /// <summary>
        /// 从文件路径打开工作簿。
        /// </summary>
        /// <param name="path">Excel 文件路径。</param>
        /// <returns>打开的 <see cref="IExcelWorkbook"/> 实例。</returns>
        public IExcelWorkbook Open(string path)
        {
            return ExcelWorkbookFactory.Open(path);
        }

        /// <summary>
        /// 从流打开工作簿，会将原始流内容拷贝到内存缓冲区以便 NPOI 处理。
        /// </summary>
        /// <param name="stream">包含 Excel 内容的可读取流。</param>
        /// <param name="name">逻辑名称或显示用名称。</param>
        /// <returns>打开的 <see cref="IExcelWorkbook"/> 实例。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="stream"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="stream"/> 不可读取时抛出。</exception>
        public IExcelWorkbook Open(Stream stream, string name)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream 必须为可读取的流", "stream");
            }

            var buffer = new MemoryStream();
            var canSeek = stream.CanSeek;
            long originalPosition = 0;
            if (canSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            stream.CopyTo(buffer);
            if (canSeek)
            {
                stream.Position = originalPosition;
            }

            buffer.Position = 0;
            var workbook = WorkbookFactory.Create(buffer);
            var evaluator = WorkbookFactory.CreateFormulaEvaluator(workbook);
            return new NpoiStreamWorkbook(buffer, workbook, evaluator, name);
        }
    }

    /// <summary>
    /// 基于 NPOI 的文件工作簿实现，从物理文件中打开并读取 Excel 内容。
    /// </summary>
    public sealed class NpoiWorkbook : IExcelWorkbook
    {
        private readonly FileStream stream;

        private readonly IWorkbook workbook;

        private readonly IFormulaEvaluator evaluator;

        /// <summary>
        /// 使用指定路径和格式创建 <see cref="NpoiWorkbook"/> 实例。
        /// </summary>
        /// <param name="path">Excel 文件路径。</param>
        /// <param name="format">文件格式类型。</param>
        public NpoiWorkbook(string path, ExcelFileFormat format)
        {
            Path = System.IO.Path.GetFullPath(path);
            Format = format;
            stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (format == ExcelFileFormat.Xls)
            {
                workbook = new HSSFWorkbook(stream);
            }
            else
            {
                workbook = new XSSFWorkbook(stream);
            }

            evaluator = WorkbookFactory.CreateFormulaEvaluator(workbook);
        }

        /// <summary>
        /// 获取工作簿的物理格式类型。
        /// </summary>
        public ExcelFileFormat Format { get; }

        /// <summary>
        /// 获取工作簿的完整文件路径。
        /// </summary>
        public string Path { get; }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSheetNames()
        {
            var list = new List<string>();
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                list.Add(workbook.GetSheetName(i));
            }

            return list;
        }

        /// <inheritdoc/>
        public IExcelWorksheet GetWorksheet(int index)
        {
            if (index < 0 || index >= workbook.NumberOfSheets)
            {
                throw new ExcelProcessingException("工作表索引超出范围: " + index.ToString());
            }

            var sheet = workbook.GetSheetAt(index);
            return new NpoiWorksheet(sheet, evaluator);
        }

        /// <inheritdoc/>
        public IExcelWorksheet GetWorksheet(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ExcelProcessingException("工作表名称为空");
            }

            var sheet = workbook.GetSheet(name);
            if (sheet == null)
            {
                throw new ExcelProcessingException("未找到工作表: " + name);
            }

            return new NpoiWorksheet(sheet, evaluator);
        }

        /// <inheritdoc/>
        public byte[] GetVbaProjectBytes()
        {
            if (Format != ExcelFileFormat.Xlsm)
            {
                return new byte[0];
            }

            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read, true))
            {
                var entry = zip.GetEntry("xl/vbaProject.bin");
                if (entry == null)
                {
                    return new byte[0];
                }

                using (var es = entry.Open())
                using (var ms = new MemoryStream())
                {
                    es.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            evaluator?.ClearAllCachedResultValues();
            workbook?.Close();
            stream?.Dispose();
        }
    }

    /// <summary>
    /// 基于 NPOI 的流工作簿实现，从内存流中读取 Excel 内容。
    /// 适用于通过网络或其它非文件来源获得的 Excel 数据。
    /// </summary>
    public sealed class NpoiStreamWorkbook : IExcelWorkbook
    {
        private readonly MemoryStream stream;

        private readonly IWorkbook workbook;

        private readonly IFormulaEvaluator evaluator;

        /// <summary>
        /// 使用内存流、NPOI 工作簿和公式计算器创建 <see cref="NpoiStreamWorkbook"/> 实例。
        /// </summary>
        /// <param name="stream">承载 Excel 内容的内存流。</param>
        /// <param name="workbook">NPOI 工作簿实例。</param>
        /// <param name="evaluator">公式计算器实例。</param>
        /// <param name="name">逻辑名称或显示用名称。</param>
        public NpoiStreamWorkbook(MemoryStream stream, IWorkbook workbook, IFormulaEvaluator evaluator, string name)
        {
            this.stream = stream;
            this.workbook = workbook;
            this.evaluator = evaluator;
            Path = name ?? string.Empty;
            Format = ExcelFileFormat.Unknown;
        }

        /// <summary>
        /// 获取工作簿的物理格式类型，对于流构造的工作簿通常为 <see cref="ExcelFileFormat.Unknown"/>。
        /// </summary>
        public ExcelFileFormat Format { get; }

        /// <summary>
        /// 获取工作簿的逻辑名称或显示用名称。
        /// </summary>
        public string Path { get; }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSheetNames()
        {
            var list = new List<string>();
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                list.Add(workbook.GetSheetName(i));
            }

            return list;
        }

        /// <inheritdoc/>
        public IExcelWorksheet GetWorksheet(int index)
        {
            if (index < 0 || index >= workbook.NumberOfSheets)
            {
                throw new ExcelProcessingException("工作表索引超出范围: " + index.ToString());
            }

            var sheet = workbook.GetSheetAt(index);
            return new NpoiWorksheet(sheet, evaluator);
        }

        /// <inheritdoc/>
        public IExcelWorksheet GetWorksheet(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ExcelProcessingException("工作表名称为空");
            }

            var sheet = workbook.GetSheet(name);
            if (sheet == null)
            {
                throw new ExcelProcessingException("未找到工作表: " + name);
            }

            return new NpoiWorksheet(sheet, evaluator);
        }

        /// <inheritdoc/>
        public byte[] GetVbaProjectBytes()
        {
            return new byte[0];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            evaluator?.ClearAllCachedResultValues();
            workbook?.Close();
            stream?.Dispose();
        }
    }

    /// <summary>
    /// 基于 NPOI 的工作表实现，封装单个工作表的行列统计与单元格访问逻辑。
    /// </summary>
    public sealed class NpoiWorksheet : IExcelWorksheet
    {
        private readonly ISheet sheet;

        private readonly IFormulaEvaluator evaluator;

        /// <summary>
        /// 使用 NPOI 工作表与公式计算器创建 <see cref="NpoiWorksheet"/> 实例。
        /// </summary>
        /// <param name="sheet">NPOI 工作表实例。</param>
        /// <param name="evaluator">公式计算器实例。</param>
        public NpoiWorksheet(ISheet sheet, IFormulaEvaluator evaluator)
        {
            this.sheet = sheet;
            this.evaluator = evaluator;
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return sheet.SheetName; }
        }

        /// <inheritdoc/>
        public int RowCount
        {
            get { return sheet.LastRowNum + 1; }
        }

        /// <inheritdoc/>
        public int ColumnCount
        {
            get
            {
                var max = 0;
                for (var i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }

                    if (row.LastCellNum > max)
                    {
                        max = row.LastCellNum;
                    }
                }

                return max;
            }
        }

        /// <inheritdoc/>
        public object GetCellValue(int rowIndex, int columnIndex, bool evaluateFormula)
        {
            if (rowIndex < 0 || columnIndex < 0)
            {
                throw new ExcelProcessingException("单元格索引必须为非负数");
            }

            var row = sheet.GetRow(rowIndex);
            if (row == null)
            {
                return null;
            }

            var cell = row.GetCell(columnIndex);
            if (cell == null)
            {
                return null;
            }

            if (evaluateFormula && cell.CellType == CellType.Formula)
            {
                var cv = evaluator.Evaluate(cell);
                if (cv == null)
                {
                    return null;
                }

                switch (cv.CellType)
                {
                    case CellType.Boolean:
                        return cv.BooleanValue;
                    case CellType.Numeric:
                        return cv.NumberValue;
                    case CellType.String:
                        return cv.StringValue;
                    default:
                        return null;
                }
            }

            switch (cell.CellType)
            {
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Numeric:
                    return cell.NumericCellValue;
                case CellType.String:
                    try
                    {
                        return cell.StringCellValue;
                    }
                    catch (FormatException)
                    {
                        return cell.ToString();
                    }
                case CellType.Blank:
                    return null;
                default:
                    return null;
            }
        }
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore SA1201
#pragma warning restore SA1602
#pragma warning restore SA1513
#pragma warning restore SA1649
#pragma warning restore SA1600
#pragma warning restore SA1633
#pragma warning restore CS1591
