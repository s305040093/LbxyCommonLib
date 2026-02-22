namespace LbxyCommonLib.ExcelImport.Tests
{
    using System;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using LbxyCommonLib.ExcelImport;
    using LbxyCommonLib.ExcelProcessing;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ExcelImporterTests
    {
        [Test]
        public void ReadXlsx_HeaderAndNegative()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns.Count, Is.EqualTo(3));
            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Note"));
            Assert.That(dt.Rows.Count, Is.EqualTo(2));

            // Row 1: -123.45
            Assert.That(dt.Rows[0][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[0][1], Is.EqualTo(-123.45m));
            // Row 2: (678.90) -> -678.90
            Assert.That(dt.Rows[1][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[1][1], Is.EqualTo(-678.90m));
        }

        [Test]
        public void ReadXlsx_WithHeader_UsesHeaderRowIndexAndDefaultDataRowIndex()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(settings.HeaderRowIndex, Is.EqualTo(0));
            Assert.That(settings.DataRowIndex, Is.EqualTo(1));
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(dt.Rows[0][0], Is.EqualTo("Alice"));
            Assert.That(dt.Rows[1][0], Is.EqualTo("Bob"));
        }

        [Test]
        public async Task ReadXlsx_NoHeader_Async()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = false, DataRowIndex = 0 };
            var dt = await importer.ReadToDataTableAsync(path, settings);
            Assert.That(dt.Rows.Count, Is.GreaterThanOrEqualTo(3)); // includes header as data
        }

        [Test]
        public void ReadXlsx_FromMemoryStream_ShouldMatchFile()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var expected = importer.ReadToDataTable(path, settings);

            var bytes = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(bytes))
            {
                var actual = importer.ReadToDataTable(ms, new ExcelImportSettings { HasHeader = true });
                Assert.That(actual.Columns.Count, Is.EqualTo(expected.Columns.Count));
                Assert.That(actual.Rows.Count, Is.EqualTo(expected.Rows.Count));
                Assert.That(actual.Rows[0][1], Is.EqualTo(expected.Rows[0][1]));
                Assert.That(actual.Rows[1][1], Is.EqualTo(expected.Rows[1][1]));
            }
        }

        [Test]
        public void ReadXls_FromMemoryStream_ShouldMatchFile()
        {
            var path = CreateSimpleXls();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var expected = importer.ReadToDataTable(path, settings);

            var bytes = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(bytes))
            {
                var actual = importer.ReadToDataTable(ms, new ExcelImportSettings { HasHeader = true });
                Assert.That(actual.Columns.Count, Is.EqualTo(expected.Columns.Count));
                Assert.That(actual.Rows.Count, Is.EqualTo(expected.Rows.Count));
                Assert.That(actual.Rows[0][1], Is.EqualTo(expected.Rows[0][1]));
                Assert.That(actual.Rows[1][1], Is.EqualTo(expected.Rows[1][1]));
            }
        }

        [Test]
        public void ReadXlsm_FromMemoryStream_ShouldMatchFile()
        {
            var path = CreateSimpleXlsm();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var expected = importer.ReadToDataTable(path, settings);

            var bytes = File.ReadAllBytes(path);
            using (var ms = new MemoryStream(bytes))
            {
                var actual = importer.ReadToDataTable(ms, new ExcelImportSettings { HasHeader = true });
                Assert.That(actual.Columns.Count, Is.EqualTo(expected.Columns.Count));
                Assert.That(actual.Rows.Count, Is.EqualTo(expected.Rows.Count));
                Assert.That(actual.Rows[0][1], Is.EqualTo(expected.Rows[0][1]));
                Assert.That(actual.Rows[1][1], Is.EqualTo(expected.Rows[1][1]));
            }
        }

        [Test]
        public void ReadXlsx_FromFileStream_ShouldMatchFile()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var expected = importer.ReadToDataTable(path, settings);

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var actual = importer.ReadToDataTable(fs, new ExcelImportSettings { HasHeader = true });
                Assert.That(actual.Columns.Count, Is.EqualTo(expected.Columns.Count));
                Assert.That(actual.Rows.Count, Is.EqualTo(expected.Rows.Count));
                Assert.That(actual.Rows[0][1], Is.EqualTo(expected.Rows[0][1]));
                Assert.That(actual.Rows[1][1], Is.EqualTo(expected.Rows[1][1]));
            }
        }

        [Test]
        public void ReadXlsx_StreamNull_ShouldThrowArgumentException()
        {
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            Assert.Throws<ArgumentException>(() => importer.ReadToDataTable((Stream)null, settings));
        }

        [Test]
        public void ReadXlsx_FileNotFound_ShouldThrowExcelImportExceptionWithInnerFileNotFound()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + ".xlsx");
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var ex = Assert.Throws<ExcelImportException>(() => importer.ReadToDataTable(path, settings));
            Assert.That(ex.ErrorCode, Is.EqualTo(ExcelImportErrorCode.FileNotFound));
            Assert.That(ex.InnerException, Is.InstanceOf<FileNotFoundException>());
        }

        [Test]
        public void ReadXls_HeaderAndNegative()
        {
            var path = CreateSimpleXls();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns.Count, Is.EqualTo(3));
            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Note"));
            Assert.That(dt.Rows.Count, Is.EqualTo(2));

            Assert.That(dt.Rows[0][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[0][1], Is.EqualTo(-123.45m));
            Assert.That(dt.Rows[1][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[1][1], Is.EqualTo(-678.90m));
        }

        [Test]
        public void ReadXlsm_HeaderAndNegative()
        {
            var path = CreateSimpleXlsm();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns.Count, Is.EqualTo(3));
            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Note"));
            Assert.That(dt.Rows.Count, Is.EqualTo(2));

            Assert.That(dt.Rows[0][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[0][1], Is.EqualTo(-123.45m));
            Assert.That(dt.Rows[1][1], Is.InstanceOf<decimal>());
            Assert.That((decimal)dt.Rows[1][1], Is.EqualTo(-678.90m));
        }

        [Test]
        public void ReadXlsx_WithHeaderAndCustomDataRowIndex()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true, HeaderRowIndex = 0, DataRowIndex = 2 };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0][0], Is.EqualTo("Bob"));
        }

        [Test]
        public void ReadXlsx_NoHeader_UsesDataRowIndex()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = false, DataRowIndex = 1 };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(dt.Rows[0][0], Is.EqualTo("Alice"));
            Assert.That(dt.Rows[1][0], Is.EqualTo("Bob"));
        }

        [Test]
        public void ReadXlsx_InvalidHeaderAndDataRowIndex_ShouldThrow()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true, HeaderRowIndex = 1, DataRowIndex = 1 };
            Assert.Throws<ExcelImportException>(() => importer.ReadToDataTable(path, settings));
        }

        [Test]
        public void Read_FromInMemoryAdapter_ShouldParseHeaderAndNumbers()
        {
            var cells = new object[,]
            {
                { "Name", "Amount" },
                { "Alice", "10.5" },
                { "Bob", "-3" },
            };

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var original = ExcelWorkbookProvider.Current;
            try
            {
                ExcelWorkbookProvider.Current = new InMemoryWorkbookProvider(cells);
                using (var ms = new MemoryStream(new byte[0]))
                {
                    var dt = importer.ReadToDataTable(ms, settings);
                    Assert.That(dt.Columns.Count, Is.EqualTo(2));
                    Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
                    Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
                    Assert.That(dt.Rows.Count, Is.EqualTo(2));
                    Assert.That(dt.Rows[0][1], Is.InstanceOf<decimal>());
                    Assert.That((decimal)dt.Rows[0][1], Is.EqualTo(10.5m));
                    Assert.That(dt.Rows[1][1], Is.InstanceOf<decimal>());
                    Assert.That((decimal)dt.Rows[1][1], Is.EqualTo(-3m));
                }
            }
            finally
            {
                ExcelWorkbookProvider.Current = original;
            }
        }

        [Test]
        public void ReadXlsx_WhenFileOpenedWithSharedReadWrite_ShouldSucceed()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            using (var holder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var dt = importer.ReadToDataTable(path, settings);
                Assert.That(dt.Rows.Count, Is.EqualTo(2));
                Assert.That(dt.Columns.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public void ReadXlsx_WhenFileExclusivelyLocked_ShouldThrowExcelImportExceptionWithFileLocked()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            using (var holder = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var ex = Assert.Throws<ExcelImportException>(() => importer.ReadToDataTable(path, settings));
                Assert.That(ex.ErrorCode, Is.EqualTo(ExcelImportErrorCode.FileLocked));
                StringAssert.Contains("Path=", ex.ValueSnapshot);
                StringAssert.Contains("Attempts=", ex.ValueSnapshot);
            }
        }

        [Test]
        public void Read_FromCsvAdapter_ShouldParseHeaderAndNumbers()
        {
            var csv = "Name,Amount\r\nAlice,1.5\r\nBob,-2\r\n";
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var original = ExcelWorkbookProvider.Current;
            try
            {
                ExcelWorkbookProvider.Current = new CsvWorkbookProvider();
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
                {
                    var dt = importer.ReadToDataTable(ms, settings);
                    Assert.That(dt.Columns.Count, Is.EqualTo(2));
                    Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
                    Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
                    Assert.That(dt.Rows.Count, Is.EqualTo(2));
                    Assert.That(dt.Rows[0][1], Is.InstanceOf<decimal>());
                    Assert.That((decimal)dt.Rows[0][1], Is.EqualTo(1.5m));
                    Assert.That(dt.Rows[1][1], Is.InstanceOf<decimal>());
                    Assert.That((decimal)dt.Rows[1][1], Is.EqualTo(-2m));
                }
            }
            finally
            {
                ExcelWorkbookProvider.Current = original;
            }
        }

        private static string CreateSimpleXlsx()
        {
            var temp = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + ".xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(temp));
            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, true))
            {
                Add(zip, "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
                              <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
                              <Default Extension=""xml"" ContentType=""application/xml""/>
                              <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
                              <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
                              <Override PartName=""/xl/sharedStrings.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml""/>
                            </Types>");

                Add(zip, "_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
                              <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
                            </Relationships>");

                Add(zip, "xl/workbook.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
                              <sheets>
                                <sheet name=""Sheet1"" sheetId=""1"" r:id=""rId1""/>
                              </sheets>
                            </workbook>");

                Add(zip, "xl/_rels/workbook.xml.rels", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
                              <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
                              <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"" Target=""sharedStrings.xml""/>
                            </Relationships>");

                Add(zip, "xl/sharedStrings.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <sst xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" count=""8"" uniqueCount=""8"">
                              <si><t>Name</t></si>
                              <si><t>Amount</t></si>
                              <si><t>Note</t></si>
                              <si><t>Alice</t></si>
                              <si><t>Bob</t></si>
                              <si><t>(678.90)</t></si>
                              <si><t>note-1</t></si>
                              <si><t>note-2</t></si>
                            </sst>");

                Add(zip, "xl/worksheets/sheet1.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
                              <sheetData>
                                <row r=""1"">
                                  <c r=""A1"" t=""s""><v>0</v></c>
                                  <c r=""B1"" t=""s""><v>1</v></c>
                                  <c r=""C1"" t=""s""><v>2</v></c>
                                </row>
                                <row r=""2"">
                                  <c r=""A2"" t=""s""><v>3</v></c>
                                  <c r=""B2""><v>-123.45</v></c>
                                  <c r=""C2"" t=""s""><v>6</v></c>
                                </row>
                                <row r=""3"">
                                  <c r=""A3"" t=""s""><v>4</v></c>
                                  <c r=""B3"" t=""s""><v>5</v></c>
                                  <c r=""C3"" t=""s""><v>7</v></c>
                                </row>
                              </sheetData>
                            </worksheet>");
            }

            return temp;
        }

        private static string CreateSimpleXls()
        {
            var temp = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + ".xls");
            Directory.CreateDirectory(Path.GetDirectoryName(temp));
            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new HSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Name");
                header.CreateCell(1).SetCellValue("Amount");
                header.CreateCell(2).SetCellValue("Note");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Alice");
                row1.CreateCell(1).SetCellValue(-123.45d);
                row1.CreateCell(2).SetCellValue("note-1");

                var row2 = sheet.CreateRow(2);
                row2.CreateCell(0).SetCellValue("Bob");
                row2.CreateCell(1).SetCellValue("(678.90)");
                row2.CreateCell(2).SetCellValue("note-2");

                wb.Write(fs);
            }

            return temp;
        }

        private static string CreateSimpleXlsm()
        {
            var temp = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + ".xlsm");
            Directory.CreateDirectory(Path.GetDirectoryName(temp));
            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Name");
                header.CreateCell(1).SetCellValue("Amount");
                header.CreateCell(2).SetCellValue("Note");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Alice");
                row1.CreateCell(1).SetCellValue(-123.45d);
                row1.CreateCell(2).SetCellValue("note-1");

                var row2 = sheet.CreateRow(2);
                row2.CreateCell(0).SetCellValue("Bob");
                row2.CreateCell(1).SetCellValue("(678.90)");
                row2.CreateCell(2).SetCellValue("note-2");

                wb.Write(fs);
            }

            return temp;
        }

        private static void Add(ZipArchive zip, string path, string content)
        {
            var e = zip.CreateEntry(path);
            using (var s = e.Open())
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                w.Write(content);
            }
        }

        private sealed class InMemoryWorkbookProvider : IExcelWorkbookProvider
        {
            private readonly object[,] cells;

            public InMemoryWorkbookProvider(object[,] cells)
            {
                this.cells = cells;
            }

            public IExcelWorkbook Open(string path)
            {
                return CreateWorkbook();
            }

            public IExcelWorkbook Open(Stream stream, string name)
            {
                return CreateWorkbook();
            }

            private IExcelWorkbook CreateWorkbook()
            {
                var sheet = new InMemoryWorksheet(cells);
                return new InMemoryWorkbook(sheet);
            }
        }

        private sealed class InMemoryWorkbook : IExcelWorkbook
        {
            private readonly IExcelWorksheet sheet;

            public InMemoryWorkbook(IExcelWorksheet sheet)
            {
                this.sheet = sheet;
            }

            public ExcelFileFormat Format
            {
                get { return ExcelFileFormat.Xlsx; }
            }

            public string Path
            {
                get { return string.Empty; }
            }

            public IReadOnlyList<string> GetSheetNames()
            {
                return new[] { "Sheet1" };
            }

            public IExcelWorksheet GetWorksheet(int index)
            {
                return sheet;
            }

            public IExcelWorksheet GetWorksheet(string name)
            {
                return sheet;
            }

            public byte[] GetVbaProjectBytes()
            {
                return new byte[0];
            }

            public void Dispose()
            {
            }
        }

        private sealed class InMemoryWorksheet : IExcelWorksheet
        {
            private readonly object[,] cells;

            public InMemoryWorksheet(object[,] cells)
            {
                this.cells = cells;
            }

            public string Name
            {
                get { return "Sheet1"; }
            }

            public int RowCount
            {
                get { return cells.GetLength(0); }
            }

            public int ColumnCount
            {
                get { return cells.GetLength(1); }
            }

            public object GetCellValue(int rowIndex, int columnIndex, bool evaluateFormula)
            {
                return cells[rowIndex, columnIndex];
            }
        }

        private sealed class CsvWorkbookProvider : IExcelWorkbookProvider
        {
            public IExcelWorkbook Open(string path)
            {
                throw new NotSupportedException();
            }

            public IExcelWorkbook Open(Stream stream, string name)
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                {
                    var rows = new System.Collections.Generic.List<string[]>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0)
                        {
                            continue;
                        }

                        rows.Add(line.Split(','));
                    }

                    if (rows.Count == 0)
                    {
                        var emptyCells = new object[0, 0];
                        return new InMemoryWorkbook(new InMemoryWorksheet(emptyCells));
                    }

                    var rowCount = rows.Count;
                    var columnCount = rows[0].Length;
                    var cells = new object[rowCount, columnCount];
                    for (var r = 0; r < rowCount; r++)
                    {
                        var parts = rows[r];
                        for (var c = 0; c < columnCount; c++)
                        {
                            cells[r, c] = c < parts.Length ? parts[c] : string.Empty;
                        }
                    }

                    return new InMemoryWorkbook(new InMemoryWorksheet(cells));
                }
            }
        }
    }
}
