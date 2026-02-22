namespace LbxyCommonLib.ExcelProcessing.Tests
{
    using System;
    using System.IO;
    using LbxyCommonLib.ExcelProcessing;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ExcelFileModuleTests
    {
        [Test]
        public void Detect_Xlsx_ByExtensionAndHeader()
        {
            var path = CreateXlsx(false);
            var info = ExcelFileDetector.Detect(path);
            Assert.That(info.Format, Is.EqualTo(ExcelFileFormat.Xlsx));
        }

        [Test]
        public void Detect_Xls_ByHeaderWhenExtensionMissing()
        {
            var path = CreateXls(null);
            var info = ExcelFileDetector.Detect(path);
            Assert.That(info.Format, Is.EqualTo(ExcelFileFormat.Xls));
        }

        [Test]
        public void OpenWorkbook_ReadCellAndFormula()
        {
            var path = CreateXlsx(true);
            using (var wb = ExcelWorkbookFactory.Open(path))
            {
                var names = wb.GetSheetNames();
                Assert.That(names.Count, Is.EqualTo(1));
                var sheet = wb.GetWorksheet(0);
                Assert.That(sheet.Name, Is.EqualTo("Sheet1"));
                Assert.That(sheet.GetCellValue(0, 0, true), Is.EqualTo(1d));
                Assert.That(sheet.GetCellValue(0, 1, true), Is.EqualTo(2d));
                Assert.That(sheet.GetCellValue(1, 0, true), Is.EqualTo(3d));
            }
        }

        private static string CreateXlsx(bool withFormula)
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelFormats");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".xlsx");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var row0 = sheet.CreateRow(0);
                row0.CreateCell(0).SetCellValue(1d);
                row0.CreateCell(1).SetCellValue(2d);
                if (withFormula)
                {
                    var row1 = sheet.CreateRow(1);
                    row1.CreateCell(0).CellFormula = "A1+B1";
                }

                wb.Write(fs);
            }

            return path;
        }

        private static string CreateXls(string extension)
        {
            var dir = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_ExcelFormats");
            Directory.CreateDirectory(dir);
            var ext = string.IsNullOrEmpty(extension) ? string.Empty : extension;
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ext);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new HSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var row0 = sheet.CreateRow(0);
                row0.CreateCell(0).SetCellValue(1d);
                wb.Write(fs);
            }

            return path;
        }
    }
}

