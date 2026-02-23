namespace LbxyCommonLib.ExcelImport.Tests
{
    using System;
    using System.Data;
    using System.IO;
    using LbxyCommonLib.ExcelImport;
    using LbxyCommonLib.ExcelProcessing;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ExcelImporterAdvancedTests
    {
        [Test]
        public void FillPredefinedDataTable_HeaderIndexList_Mode()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderIndexList,
            };
            settings.HeaderIndexList.Add(1);
            settings.HeaderIndexList.Add(2);

            var table = new DataTable("Advanced");
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void FillPredefinedDataTable_HeaderStartIndex_Mode()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                HeaderRowIndex = 0,
                DataRowIndex = 1,
            };

            var table = new DataTable("Advanced");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1]["Name"], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void FillPredefinedDataTable_HeaderStartIndex_WithCustomHeaderAndDataRowIndex()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                HeaderRowIndex = 1,
                DataRowIndex = 2,
            };

            var table = new DataTable("AdvancedShifted");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            // 构造一个带有偏移的工作表：在原始数据上方插入一行占位行
            // 为了复用 CreateAdvancedXlsx 生成的结构，这里通过额外写入一个前置行的方式实现模拟
            // 实际场景中，HeaderRowIndex / DataRowIndex 将指向真实表头与数据起始行
            var originalPath = path;
            var shiftedPath = CreateAdvancedXlsxWithEmptyTopRow(originalPath);

            var result = importer.FillPredefinedDataTable(shiftedPath, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1]["Name"], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void FillPredefinedDataTable_HeaderByName_Mode_ShouldMatchByColumnName()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
                HeaderRowIndex = 0,
                DataRowIndex = 1,
            };

            var table = new DataTable("AdvancedByName");
            table.Columns.Add("Note", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1]["Name"], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void FillPredefinedDataTable_HeaderByName_MissingColumn_ShouldThrow()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
                HeaderRowIndex = 0,
                DataRowIndex = 1,
            };

            var table = new DataTable("AdvancedByNameMissing");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Missing", typeof(string));

            Assert.Throws<ExcelImportException>(() => importer.FillPredefinedDataTable(path, settings, table));
        }

        [Test]
        public void FillPredefinedDataTable_HeaderByName_WithAliasViaRenameMapByName()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderByName,
                HeaderRowIndex = 0,
                DataRowIndex = 1,
            };

            // 将表头 Name 映射为 CustomerName，相当于 Name/CustomerName 作为同义列名
            settings.HeaderRenameMapByName["Name"] = "CustomerName";

            var table = new DataTable("AdvancedByNameAlias");
            table.Columns.Add("CustomerName", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["CustomerName"], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[1]["CustomerName"], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
        }

        [Test]
        public void ImportAdvanced_FromFilePath_ShouldFillPredefinedTable()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                HeaderRowIndex = 0,
                DataRowIndex = 1,
            };

            var table = new DataTable("Advanced");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            var result = importer.ImportAdvanced(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1]["Name"], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void ImportAdvanced_HeaderRowIndexEqualDataRowIndex_ShouldThrowArgumentException()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                HeaderRowIndex = 1,
                DataRowIndex = 1,
            };

            var table = new DataTable("Advanced");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            Assert.Throws<ArgumentException>(() => importer.ImportAdvanced(path, settings, table));
        }

        [Test]
        public void ImportAdvanced_HeaderRowIndexGreaterThanDataRowIndex_ShouldThrowArgumentException()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                HeaderRowIndex = 2,
                DataRowIndex = 1,
            };

            var table = new DataTable("Advanced");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            Assert.Throws<ArgumentException>(() => importer.ImportAdvanced(path, settings, table));
        }

        [Test]
        public void FillPredefinedDataTable_MixedDataTypes_ShouldConvertCorrectly()
        {
            var path = CreateDataTypeRichXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
            };

            var table = new DataTable("Typed");
            table.Columns.Add("TextCol", typeof(string));
            table.Columns.Add("NumberCol", typeof(decimal));
            table.Columns.Add("DateCol", typeof(DateTime));
            table.Columns.Add("BoolCol", typeof(bool));
            table.Columns.Add("FormulaCol", typeof(double));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["TextCol"], Is.EqualTo("Hello"));
            Assert.That(result.Table.Rows[0]["NumberCol"], Is.EqualTo(1.5m));
            Assert.That(((DateTime)result.Table.Rows[0]["DateCol"]).Date, Is.EqualTo(new DateTime(2020, 1, 1)));
            Assert.That(result.Table.Rows[0]["BoolCol"], Is.EqualTo(true));
            Assert.That(result.Table.Rows[0]["FormulaCol"], Is.EqualTo(3.0d).Within(0.0001d));

            Assert.That(result.Table.Rows[1]["TextCol"], Is.EqualTo("World"));
            Assert.That(result.Table.Rows[1]["NumberCol"], Is.EqualTo(-2.5m));
            Assert.That(((DateTime)result.Table.Rows[1]["DateCol"]).Date, Is.EqualTo(new DateTime(2020, 1, 2)));
            Assert.That(result.Table.Rows[1]["BoolCol"], Is.EqualTo(false));
            Assert.That(result.Table.Rows[1]["FormulaCol"], Is.EqualTo(-5.0d).Within(0.0001d));
        }

        [Test]
        public void FillPredefinedDataTable_BracketNegative_AsText_WithDefaultValue()
        {
            var path = CreateAdvancedXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                EnableBracketNegative = true,
                BracketAsNumeric = false,
                BracketNegativeDefaultValue = 0m,
            };

            var table = new DataTable("Advanced");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Note", typeof(string));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(0m));
        }

        [Test]
        public void NumericLikeDate_AcceptNumericAsDate_True_ParsesAsDate()
        {
            var path = CreateNumericLikeDateXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                AcceptNumericAsDate = true,
            };

            var table = new DataTable("NumericDate");
            table.Columns.Add("DateValue", typeof(DateTime));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(1));
            Assert.That(((DateTime)result.Table.Rows[0]["DateValue"]).Date, Is.EqualTo(new DateTime(2026, 1, 1)));
            var hasWarning = false;
            foreach (var log in result.Logs)
            {
                if (log.Message != null && log.Message.StartsWith("ExcelImporter.NumericAsDateWarning", StringComparison.Ordinal))
                {
                    hasWarning = true;
                    break;
                }
            }

            Assert.That(hasWarning, Is.True);
        }

        [Test]
        public void NumericLikeDate_AcceptNumericAsDate_False_KeepsNumeric()
        {
            var path = CreateNumericLikeDateXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                AcceptNumericAsDate = false,
            };

            var table = new DataTable("NumericDate");
            table.Columns.Add("DateValue", typeof(long));

            var result = importer.FillPredefinedDataTable(path, settings, table);

            Assert.That(result.Table.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Table.Rows[0]["DateValue"], Is.EqualTo(20260101L));
            var hasWarning = false;
            foreach (var log in result.Logs)
            {
                if (log.Message != null && log.Message.StartsWith("ExcelImporter.NumericAsDateWarning", StringComparison.Ordinal))
                {
                    hasWarning = true;
                    break;
                }
            }

            Assert.That(hasWarning, Is.True);
        }

        [Test]
        public void NonNumericString_ShouldNotTriggerNumericDateWarning()
        {
            var path = CreateNonNumericStringXlsx();
            var importer = new ExcelImporter();

            var tableTrue = new DataTable("TextAsNumberTrue");
            tableTrue.Columns.Add("Value", typeof(string));
            var settingsTrue = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                AcceptNumericAsDate = true,
            };

            var resultTrue = importer.FillPredefinedDataTable(path, settingsTrue, tableTrue);
            var hasWarningTrue = false;
            foreach (var log in resultTrue.Logs)
            {
                if (log.Message != null && log.Message.StartsWith("ExcelImporter.NumericAsDateWarning", StringComparison.Ordinal))
                {
                    hasWarningTrue = true;
                    break;
                }
            }

            Assert.That(hasWarningTrue, Is.False);

            var tableFalse = new DataTable("TextAsNumberFalse");
            tableFalse.Columns.Add("Value", typeof(string));
            var settingsFalse = new ExcelImportSettings
            {
                HasHeader = true,
                HeaderReadMode = ExcelHeaderReadMode.HeaderStartIndex,
                HeaderStartColumnIndex = 0,
                AcceptNumericAsDate = false,
            };

            var resultFalse = importer.FillPredefinedDataTable(path, settingsFalse, tableFalse);
            var hasWarningFalse = false;
            foreach (var log in resultFalse.Logs)
            {
                if (log.Message != null && log.Message.StartsWith("ExcelImporter.NumericAsDateWarning", StringComparison.Ordinal))
                {
                    hasWarningFalse = true;
                    break;
                }
            }

            Assert.That(hasWarningFalse, Is.False);
        }

        private static string CreateAdvancedXlsx()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelAdvanced");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
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

            return path;
        }

        private static string CreateAdvancedXlsxWithEmptyTopRow(string sourcePath)
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelAdvancedShifted");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");

                // 行 0：空行（占位）
                sheet.CreateRow(0);

                // 从原始文件复制表头与数据到行 1+，以模拟 HeaderRowIndex=1, DataRowIndex=2 的场景
                using (var src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IWorkbook srcWb = new XSSFWorkbook(src);
                    var srcSheet = srcWb.GetSheetAt(0);
                    for (var r = 0; r <= 2; r++)
                    {
                        var srcRow = srcSheet.GetRow(r);
                        if (srcRow == null)
                        {
                            continue;
                        }

                        var destRow = sheet.CreateRow(r + 1);
                        for (var c = 0; c < srcRow.LastCellNum; c++)
                        {
                            var srcCell = srcRow.GetCell(c);
                            if (srcCell == null)
                            {
                                continue;
                            }

                            var destCell = destRow.CreateCell(c);
                            switch (srcCell.CellType)
                            {
                                case CellType.Numeric:
                                    destCell.SetCellValue(srcCell.NumericCellValue);
                                    break;
                                case CellType.Boolean:
                                    destCell.SetCellValue(srcCell.BooleanCellValue);
                                    break;
                                case CellType.String:
                                    destCell.SetCellValue(srcCell.StringCellValue);
                                    break;
                                default:
                                    destCell.SetCellValue(srcCell.ToString());
                                    break;
                            }
                        }
                    }
                }

                wb.Write(fs);
            }

            return path;
        }

        private static string CreateDataTypeRichXlsx()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelTyped");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();

                var sheet = wb.CreateSheet("Main");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("TextCol");
                header.CreateCell(1).SetCellValue("NumberCol");
                header.CreateCell(2).SetCellValue("DateCol");
                header.CreateCell(3).SetCellValue("BoolCol");
                header.CreateCell(4).SetCellValue("FormulaCol");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Hello");
                row1.CreateCell(1).SetCellValue(1.5d);
                row1.CreateCell(2).SetCellValue(new DateTime(2020, 1, 1));
                row1.CreateCell(3).SetCellValue(true);
                row1.CreateCell(4).SetCellFormula("B2*2");

                var row2 = sheet.CreateRow(2);
                row2.CreateCell(0).SetCellValue("World");
                row2.CreateCell(1).SetCellValue(-2.5d);
                row2.CreateCell(2).SetCellValue(new DateTime(2020, 1, 2));
                row2.CreateCell(3).SetCellValue(false);
                row2.CreateCell(4).SetCellFormula("B3*2");

                var mergedSheet = wb.CreateSheet("Merged");
                var mergedHeader = mergedSheet.CreateRow(0);
                mergedHeader.CreateCell(0).SetCellValue("MergedHeader");
                mergedSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 1));

                var rowM1 = mergedSheet.CreateRow(1);
                rowM1.CreateCell(0).SetCellValue("Value1");
                rowM1.CreateCell(1).SetCellValue("Ignored");

                wb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
                wb.Write(fs);
            }

            return path;
        }

        private static string CreateNumericLikeDateXlsx()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelNumericDate");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");

                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("DateValue");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue(20260101d);

                wb.Write(fs);
            }

            return path;
        }

        private static string CreateNonNumericStringXlsx()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelTextNumber");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");

                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Value");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("44561");

                wb.Write(fs);
            }

            return path;
        }
    }
}
