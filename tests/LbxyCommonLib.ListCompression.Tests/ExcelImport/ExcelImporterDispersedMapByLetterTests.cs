namespace LbxyCommonLib.ExcelImport.Tests
{
    using System;
    using System.Data;
    using LbxyCommonLib.ExcelImport;
    using LbxyCommonLib.ExcelProcessing;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ExcelImporterDispersedMapByLetterTests
    {
        [Test]
        public void ReadToDataTable_WithDispersedMapByLetter_ShouldSelectAndOrderColumnsByLetter()
        {
            var cells = new object[2, 4];
            cells[0, 0] = "v11";
            cells[0, 1] = "v12";
            cells[0, 2] = "v13";
            cells[0, 3] = "v14";
            cells[1, 0] = "v21";
            cells[1, 1] = "v22";
            cells[1, 2] = "v23";
            cells[1, 3] = "v24";

            using (var workbook = new TestWorkbook(cells))
            {
                var importer = new ExcelImporter();
                var settings = new ExcelImportSettings
                {
                    HasHeader = false,
                    DataRowIndex = 0,
                };

                settings.DispersedMapByLetter["B"] = "Second";
                settings.DispersedMapByLetter["D"] = "Fourth";

                var table = InvokeReadExcelFromWorkbook(importer, workbook, "t", settings);

                Assert.That(table.Columns.Count, Is.EqualTo(2));
                Assert.That(table.Columns[0].ColumnName, Is.EqualTo("Second"));
                Assert.That(table.Columns[1].ColumnName, Is.EqualTo("Fourth"));

                Assert.That(table.Rows.Count, Is.EqualTo(2));
                Assert.That(table.Rows[0][0], Is.EqualTo("v12"));
                Assert.That(table.Rows[0][1], Is.EqualTo("v14"));
                Assert.That(table.Rows[1][0], Is.EqualTo("v22"));
                Assert.That(table.Rows[1][1], Is.EqualTo("v24"));
            }
        }

        [Test]
        public void ReadToDataTable_WithDispersedMapByLetter_OutOfRange_ShouldThrow()
        {
            var cells = new object[1, 2];
            cells[0, 0] = "v11";
            cells[0, 1] = "v12";

            using (var workbook = new TestWorkbook(cells))
            {
                var importer = new ExcelImporter();
                var settings = new ExcelImportSettings
                {
                    HasHeader = false,
                    DataRowIndex = 0,
                };

                settings.DispersedMapByLetter["C"] = "Third";

                var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => InvokeReadExcelFromWorkbook(importer, workbook, "t", settings));
                Assert.That(ex, Is.Not.Null);
                Assert.That(ex.InnerException, Is.InstanceOf<ExcelImportException>());
            }
        }

        private static DataTable InvokeReadExcelFromWorkbook(ExcelImporter importer, IExcelWorkbook workbook, string tableName, ExcelImportSettings settings)
        {
            var method = typeof(ExcelImporter).GetMethod("ReadExcelFromWorkbook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                throw new InvalidOperationException("ReadExcelFromWorkbook method not found via reflection.");
            }

            var result = method.Invoke(null, new object[] { workbook, tableName, settings });
            return (DataTable)result;
        }

        private sealed class TestWorksheet : IExcelWorksheet
        {
            private readonly object[,] cells;

            public TestWorksheet(object[,] cells)
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

        private sealed class TestWorkbook : IExcelWorkbook
        {
            private readonly IExcelWorksheet sheet;

            public TestWorkbook(object[,] cells)
            {
                sheet = new TestWorksheet(cells);
            }

            public ExcelFileFormat Format
            {
                get { return ExcelFileFormat.Xlsx; }
            }

            public string Path
            {
                get { return string.Empty; }
            }

            public void Dispose()
            {
            }

            public IReadOnlyList<string> GetSheetNames()
            {
                return new[] { sheet.Name };
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
                return Array.Empty<byte>();
            }
        }
    }
}
