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
    }
}

