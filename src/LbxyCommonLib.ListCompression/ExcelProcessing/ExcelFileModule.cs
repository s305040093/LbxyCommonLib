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

    public enum ExcelFileFormat
    {
        Unknown,
        Xlsx,
        Xls,
        Xlsm,
    }

    public sealed class ExcelFileInfo
    {
        public ExcelFileInfo(string path, ExcelFileFormat format, ExcelFileFormat extensionFormat)
        {
            Path = path;
            Format = format;
            ExtensionFormat = extensionFormat;
        }

        public string Path { get; }

        public ExcelFileFormat Format { get; }

        public ExcelFileFormat ExtensionFormat { get; }
    }

    public sealed class ExcelProcessingException : Exception
    {
        public ExcelProcessingException(string message)
            : base(message)
        {
        }

        public ExcelProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class ExcelFileDetector
    {
        private static readonly ConcurrentDictionary<string, ExcelFileInfo> Cache = new ConcurrentDictionary<string, ExcelFileInfo>(StringComparer.OrdinalIgnoreCase);

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

    public interface IExcelWorkbook : IDisposable
    {
        ExcelFileFormat Format { get; }

        string Path { get; }

        IReadOnlyList<string> GetSheetNames();

        IExcelWorksheet GetWorksheet(int index);

        IExcelWorksheet GetWorksheet(string name);

        byte[] GetVbaProjectBytes();
    }

    public interface IExcelWorksheet
    {
        string Name { get; }

        int RowCount { get; }

        int ColumnCount { get; }

        object GetCellValue(int rowIndex, int columnIndex, bool evaluateFormula);
    }

    public interface IExcelWorkbookProvider
    {
        IExcelWorkbook Open(string path);

        IExcelWorkbook Open(Stream stream, string name);
    }

    public static class ExcelWorkbookProvider
    {
        private static IExcelWorkbookProvider current = new XlsxAdapter();

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

    public static class ExcelWorkbookFactory
    {
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

    public sealed class XlsxAdapter : IExcelWorkbookProvider
    {
        public IExcelWorkbook Open(string path)
        {
            return ExcelWorkbookFactory.Open(path);
        }

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

    public sealed class NpoiWorkbook : IExcelWorkbook
    {
        private readonly FileStream stream;

        private readonly IWorkbook workbook;

        private readonly IFormulaEvaluator evaluator;

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

        public ExcelFileFormat Format { get; }

        public string Path { get; }

        public IReadOnlyList<string> GetSheetNames()
        {
            var list = new List<string>();
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                list.Add(workbook.GetSheetName(i));
            }

            return list;
        }

        public IExcelWorksheet GetWorksheet(int index)
        {
            if (index < 0 || index >= workbook.NumberOfSheets)
            {
                throw new ExcelProcessingException("工作表索引超出范围: " + index.ToString());
            }

            var sheet = workbook.GetSheetAt(index);
            return new NpoiWorksheet(sheet, evaluator);
        }

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

        public void Dispose()
        {
            evaluator?.ClearAllCachedResultValues();
            workbook?.Close();
            stream?.Dispose();
        }
    }

    public sealed class NpoiStreamWorkbook : IExcelWorkbook
    {
        private readonly MemoryStream stream;

        private readonly IWorkbook workbook;

        private readonly IFormulaEvaluator evaluator;

        public NpoiStreamWorkbook(MemoryStream stream, IWorkbook workbook, IFormulaEvaluator evaluator, string name)
        {
            this.stream = stream;
            this.workbook = workbook;
            this.evaluator = evaluator;
            Path = name ?? string.Empty;
            Format = ExcelFileFormat.Unknown;
        }

        public ExcelFileFormat Format { get; }

        public string Path { get; }

        public IReadOnlyList<string> GetSheetNames()
        {
            var list = new List<string>();
            for (var i = 0; i < workbook.NumberOfSheets; i++)
            {
                list.Add(workbook.GetSheetName(i));
            }

            return list;
        }

        public IExcelWorksheet GetWorksheet(int index)
        {
            if (index < 0 || index >= workbook.NumberOfSheets)
            {
                throw new ExcelProcessingException("工作表索引超出范围: " + index.ToString());
            }

            var sheet = workbook.GetSheetAt(index);
            return new NpoiWorksheet(sheet, evaluator);
        }

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

        public byte[] GetVbaProjectBytes()
        {
            return new byte[0];
        }

        public void Dispose()
        {
            evaluator?.ClearAllCachedResultValues();
            workbook?.Close();
            stream?.Dispose();
        }
    }

    public sealed class NpoiWorksheet : IExcelWorksheet
    {
        private readonly ISheet sheet;

        private readonly IFormulaEvaluator evaluator;

        public NpoiWorksheet(ISheet sheet, IFormulaEvaluator evaluator)
        {
            this.sheet = sheet;
            this.evaluator = evaluator;
        }

        public string Name
        {
            get { return sheet.SheetName; }
        }

        public int RowCount
        {
            get { return sheet.LastRowNum + 1; }
        }

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
