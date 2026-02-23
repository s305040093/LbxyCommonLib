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
        public void ImportExcel_WithMatrixOptions_StartRowAndColumn()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 1,
                StartColumnIndex = 1,
            };

            var matrix = importer.ImportExcel(path, settings, options);

            Assert.That(matrix.Length, Is.EqualTo(1));
            Assert.That(matrix[0].Length, Is.EqualTo(2));
            Assert.That(matrix[0][0], Is.EqualTo(-678.90m));
            Assert.That(matrix[0][1], Is.EqualTo("note-2"));
        }

        [Test]
        public void ImportExcelBlocks_WithBlockSize_AndOrderTopDownLeftRight()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 1,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Fill,
                BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.TopDownLeftRight,
            };

            var blocks = importer.ImportExcelBlocks(path, settings, options);

            Assert.That(blocks.Count, Is.EqualTo(4));

            var first = blocks[0];
            Assert.That(first.Length, Is.EqualTo(1));
            Assert.That(first[0].Length, Is.EqualTo(2));
            Assert.That(first[0][0], Is.EqualTo("Alice"));
            Assert.That(first[0][1], Is.EqualTo(-123.45m));

            var second = blocks[1];
            Assert.That(second.Length, Is.EqualTo(1));
            Assert.That(second[0].Length, Is.EqualTo(2));
            Assert.That(second[0][0], Is.EqualTo("note-1"));
            Assert.That(second[0][1], Is.EqualTo(string.Empty));

            var third = blocks[2];
            Assert.That(third.Length, Is.EqualTo(1));
            Assert.That(third[0].Length, Is.EqualTo(2));
            Assert.That(third[0][0], Is.EqualTo("Bob"));
            Assert.That(third[0][1], Is.EqualTo(-678.90m));

            var fourth = blocks[3];
            Assert.That(fourth.Length, Is.EqualTo(1));
            Assert.That(fourth[0].Length, Is.EqualTo(2));
            Assert.That(fourth[0][0], Is.EqualTo("note-2"));
            Assert.That(fourth[0][1], Is.EqualTo(string.Empty));
        }

        [Test]
        public void ImportExcelBlocks_WithBlockSize_AndOrderLeftRightTopDown()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 1,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Fill,
                BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.LeftRightTopDown,
            };

            var blocks = importer.ImportExcelBlocks(path, settings, options);

            Assert.That(blocks.Count, Is.EqualTo(4));

            var first = blocks[0];
            Assert.That(first.Length, Is.EqualTo(1));
            Assert.That(first[0].Length, Is.EqualTo(2));
            Assert.That(first[0][0], Is.EqualTo("Alice"));
            Assert.That(first[0][1], Is.EqualTo(-123.45m));

            var second = blocks[1];
            Assert.That(second.Length, Is.EqualTo(1));
            Assert.That(second[0].Length, Is.EqualTo(2));
            Assert.That(second[0][0], Is.EqualTo("Bob"));
            Assert.That(second[0][1], Is.EqualTo(-678.90m));

            var third = blocks[2];
            Assert.That(third.Length, Is.EqualTo(1));
            Assert.That(third[0].Length, Is.EqualTo(2));
            Assert.That(third[0][0], Is.EqualTo("note-1"));
            Assert.That(third[0][1], Is.EqualTo(string.Empty));

            var fourth = blocks[3];
            Assert.That(fourth.Length, Is.EqualTo(1));
            Assert.That(fourth[0].Length, Is.EqualTo(2));
            Assert.That(fourth[0][0], Is.EqualTo("note-2"));
            Assert.That(fourth[0][1], Is.EqualTo(string.Empty));
        }

        [Test]
        public void ImportExcelBlocksAndMerge_WithAppendStrategy_ShouldConcatenateAllBlockRows()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 1,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Fill,
                BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.TopDownLeftRight,
            };

            var mergeOptions = new ExcelBlockMergeOptions
            {
                ConflictStrategy = ExcelBlockMergeConflictStrategy.Append,
            };

            var result = importer.ImportExcelBlocksAndMerge(path, settings, options, mergeOptions);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Table.Rows.Count, Is.EqualTo(4));
            Assert.That(result.Table.Columns.Count, Is.EqualTo(2));

            Assert.That(result.Table.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(result.Table.Columns[1].ColumnName, Is.EqualTo("Amount"));

            Assert.That(result.Table.Rows[0][0], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0][1], Is.EqualTo(-123.45m));

            Assert.That(result.Table.Rows[1][0], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1][1], Is.EqualTo(string.Empty));

            Assert.That(result.Table.Rows[2][0], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[2][1], Is.EqualTo(-678.90m));

            Assert.That(result.Table.Rows[3][0], Is.EqualTo("note-2"));
            Assert.That(result.Table.Rows[3][1], Is.EqualTo(string.Empty));

            Assert.That(result.Statistics.TotalBlocks, Is.EqualTo(4));
            Assert.That(result.Statistics.SuccessfulBlocks, Is.EqualTo(4));
        }

        [Test]
        public void ImportExcelBlocksAndMerge_WithAppendStrategy_Xls_ShouldConcatenateAllBlockRows()
        {
            var path = CreateSimpleXls();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 1,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Fill,
                BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.TopDownLeftRight,
            };

            var mergeOptions = new ExcelBlockMergeOptions
            {
                ConflictStrategy = ExcelBlockMergeConflictStrategy.Append,
            };

            var result = importer.ImportExcelBlocksAndMerge(path, settings, options, mergeOptions);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Table.Rows.Count, Is.EqualTo(4));
            Assert.That(result.Table.Columns.Count, Is.EqualTo(2));

            Assert.That(result.Table.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(result.Table.Columns[1].ColumnName, Is.EqualTo("Amount"));

            Assert.That(result.Table.Rows[0][0], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0][1], Is.EqualTo(-123.45m));

            Assert.That(result.Table.Rows[1][0], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1][1], Is.EqualTo(string.Empty));

            Assert.That(result.Table.Rows[2][0], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[2][1], Is.EqualTo(-678.90m));

            Assert.That(result.Table.Rows[3][0], Is.EqualTo("note-2"));
            Assert.That(result.Table.Rows[3][1], Is.EqualTo(string.Empty));

            Assert.That(result.Statistics.TotalBlocks, Is.EqualTo(4));
            Assert.That(result.Statistics.SuccessfulBlocks, Is.EqualTo(4));
        }

        [Test]
        public void ImportExcelBlocksAndMerge_WithAppendStrategy_Xlsm_ShouldConcatenateAllBlockRows()
        {
            var path = CreateSimpleXlsm();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 1,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Fill,
                BlockTraversalOrder = ExcelImporter.MatrixBlockTraversalOrder.TopDownLeftRight,
            };

            var mergeOptions = new ExcelBlockMergeOptions
            {
                ConflictStrategy = ExcelBlockMergeConflictStrategy.Append,
            };

            var result = importer.ImportExcelBlocksAndMerge(path, settings, options, mergeOptions);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Table.Rows.Count, Is.EqualTo(4));
            Assert.That(result.Table.Columns.Count, Is.EqualTo(2));

            Assert.That(result.Table.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(result.Table.Columns[1].ColumnName, Is.EqualTo("Amount"));

            Assert.That(result.Table.Rows[0][0], Is.EqualTo("Alice"));
            Assert.That(result.Table.Rows[0][1], Is.EqualTo(-123.45m));

            Assert.That(result.Table.Rows[1][0], Is.EqualTo("note-1"));
            Assert.That(result.Table.Rows[1][1], Is.EqualTo(string.Empty));

            Assert.That(result.Table.Rows[2][0], Is.EqualTo("Bob"));
            Assert.That(result.Table.Rows[2][1], Is.EqualTo(-678.90m));

            Assert.That(result.Table.Rows[3][0], Is.EqualTo("note-2"));
            Assert.That(result.Table.Rows[3][1], Is.EqualTo(string.Empty));

            Assert.That(result.Statistics.TotalBlocks, Is.EqualTo(4));
            Assert.That(result.Statistics.SuccessfulBlocks, Is.EqualTo(4));
        }

        [Test]
        public void ExcelBlockMergeResult_ShouldCountTypeConversionFailures_FromLogs()
        {
            var table = new DataTable("t");
            var logs = new System.Collections.Generic.List<ExcelImportLogEntry>
            {
                new ExcelImportLogEntry(1, 1, "Amount", "数据类型转换失败: invalid value", "abc"),
                new ExcelImportLogEntry(2, 1, "Amount", "其他日志", "def"),
            };

            var statistics = new ExcelBlockMergeStatistics();
            var result = new ExcelBlockMergeResult(table, logs, statistics);

            Assert.That(result.Statistics.TypeConversionFailureCount, Is.EqualTo(1));
        }

        private sealed class TestTruncateRemainderHandler : ExcelImporter.IMatrixRemainderHandler
        {
            public ExcelImporter.MatrixRemainderAction Handle(ExcelImporter.MatrixRemainderContext context)
            {
                return ExcelImporter.MatrixRemainderAction.Truncate;
            }
        }

        [Test]
        public void ImportExcelBlocks_RemainderModeError_ShouldThrow()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var options = new ExcelImporter.MatrixExportOptions
            {
                StartRowIndex = 0,
                StartColumnIndex = 0,
                RowCount = 2,
                ColumnCount = 3,
                BlockRowCount = 2,
                BlockColumnCount = 2,
                RemainderMode = ExcelImporter.MatrixRemainderMode.Error,
            };

            var ex = Assert.Throws<ExcelImportException>(() => importer.ImportExcelBlocks(path, settings, options));
            Assert.That(ex.ErrorCode, Is.EqualTo(ExcelImportErrorCode.BlockRemainderNotDivisible));
        }

        [Test]
        public void ImportExcelBlocks_RemainderModePrompt_TruncateViaHandler()
        {
            var path = CreateSimpleXlsx();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };

            var originalHandler = ExcelImporter.RemainderHandler;
            try
            {
                ExcelImporter.RemainderHandler = new TestTruncateRemainderHandler();

                var options = new ExcelImporter.MatrixExportOptions
                {
                    StartRowIndex = 0,
                    StartColumnIndex = 0,
                    RowCount = 2,
                    ColumnCount = 3,
                    BlockRowCount = 2,
                    BlockColumnCount = 2,
                    RemainderMode = ExcelImporter.MatrixRemainderMode.Prompt,
                };

                var blocks = importer.ImportExcelBlocks(path, settings, options);

                Assert.That(blocks.Count, Is.EqualTo(1));
                var block = blocks[0];
                Assert.That(block.Length, Is.EqualTo(2));
                Assert.That(block[0].Length, Is.EqualTo(2));
                Assert.That(block[1].Length, Is.EqualTo(2));
                Assert.That(block[0][0], Is.EqualTo("Alice"));
                Assert.That(block[0][1], Is.EqualTo(-123.45m));
                Assert.That(block[1][0], Is.EqualTo("Bob"));
                Assert.That(block[1][1], Is.EqualTo(-678.90m));
            }
            finally
            {
                ExcelImporter.RemainderHandler = originalHandler;
            }
        }

        [Test]
        public void ReadToDataTable_DefaultSheet_ShouldUseActiveSheet_WhenNoSheetSpecified()
        {
            var path = CreateMultiSheetXlsxWithActiveSecond();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
            };

            var table = importer.ReadToDataTable(path, settings);

            Assert.That(table.Rows.Count, Is.EqualTo(1));
            Assert.That(table.Columns.Count, Is.EqualTo(2));
            Assert.That(table.Rows[0][0], Is.EqualTo("Active"));
            Assert.That(table.Rows[0][1], Is.EqualTo(2d));
        }

        [Test]
        public void ReadToDataTable_WithSheetIndex_ShouldOverrideActiveSheet()
        {
            var path = CreateMultiSheetXlsxWithActiveSecond();
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = true,
                SheetIndex = 0,
            };

            var table = importer.ReadToDataTable(path, settings);

            Assert.That(table.Rows.Count, Is.EqualTo(1));
            Assert.That(table.Columns.Count, Is.EqualTo(2));
            Assert.That(table.Rows[0][0], Is.EqualTo("First"));
            Assert.That(table.Rows[0][1], Is.EqualTo(1d));
        }

        [Test]
        public void Open_GXB()
        {
            var path = "C:\\Users\\netc\\Documents\\GXB.XLSX";
            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings
            {
                HasHeader = false,
            };


            var table = new DataTable("Advanced");
            table.Columns.Add("Amount", typeof(string));
            table.Columns.Add("Note", typeof(string));

            var result = importer.ReadToDataTable(path, settings);

            //Assert.That(result.Table.Rows.Count, Is.EqualTo(2));
            //Assert.That(result.Table.Rows[0]["Amount"], Is.EqualTo(-123.45m));
            //Assert.That(result.Table.Rows[0]["Note"], Is.EqualTo("note-1"));
            //Assert.That(result.Table.Rows[1]["Amount"], Is.EqualTo(-678.90m));
            //Assert.That(result.Table.Rows[1]["Note"], Is.EqualTo("note-2"));
        }

        [Test]
        public void ExcelImportSettings_HeaderRowIndexChange_ShouldUpdateDefaultDataRowIndex()
        {
            var settings = new ExcelImportSettings { HasHeader = true };
            Assert.That(settings.HeaderRowIndex, Is.EqualTo(0));
            Assert.That(settings.DataRowIndex, Is.EqualTo(1));

            settings.HeaderRowIndex = 2;

            Assert.That(settings.HeaderRowIndex, Is.EqualTo(2));
            Assert.That(settings.DataRowIndex, Is.EqualTo(3));
        }

        [Test]
        public void ExcelImportSettings_NoHeader_DefaultDataRowIndex_ShouldBeOne()
        {
            var settings = new ExcelImportSettings { HasHeader = false };
            Assert.That(settings.HasHeader, Is.False);
            Assert.That(settings.DataRowIndex, Is.EqualTo(1));
        }

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
        public void ReadXlsx_Header_AllEmpty_ShouldUseDefaultNames()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_EmptyHeader.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue(string.Empty);
                header.CreateCell(1).SetCellValue(string.Empty);
                header.CreateCell(2).SetCellValue(string.Empty);

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("A");
                row1.CreateCell(1).SetCellValue("1");
                row1.CreateCell(2).SetCellValue("X");

                wb.Write(fs);
            }

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Col1"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Col2"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Col3"));
        }

        [Test]
        public void ReadXlsx_Header_PartialEmptyAndWhitespace_ShouldUseDefaultNames()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_PartialEmptyHeader.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Name");
                header.CreateCell(1).SetCellValue("   ");
                header.CreateCell(2).SetCellValue(null as string);

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Alice");
                row1.CreateCell(1).SetCellValue("10");
                row1.CreateCell(2).SetCellValue("note-1");

                wb.Write(fs);
            }

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Col2"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Col3"));
        }

        [Test]
        public void ReadXlsx_Header_MixedNormalAndEmpty_ShouldAvoidNameConflicts()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_MixedHeader.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Col1");
                header.CreateCell(1).SetCellValue(string.Empty);
                header.CreateCell(2).SetCellValue(" ");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("A");
                row1.CreateCell(1).SetCellValue("B");
                row1.CreateCell(2).SetCellValue("C");

                wb.Write(fs);
            }

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Col1"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Col2"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Col3"));
        }

        [Test]
        public void ReadXlsx_HeaderPrefix_String_ShouldApplyToEmptyHeaders()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_HeaderPrefix.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue(string.Empty);
                header.CreateCell(1).SetCellValue(" ");
                header.CreateCell(2).SetCellValue(string.Empty);

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("A");
                row1.CreateCell(1).SetCellValue("B");
                row1.CreateCell(2).SetCellValue("C");

                wb.Write(fs);
            }

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true, HeaderPrefix = "Column" };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Column1"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Column2"));
            Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Column3"));
        }

        [Test]
        public void ReadXlsx_IgnoreEmptyHeader_ShouldSkipEmptyColumns()
        {
            var path = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_IgnoreEmptyHeader.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Sheet1");
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Name");
                header.CreateCell(1).SetCellValue(string.Empty);
                header.CreateCell(2).SetCellValue("Amount");

                var row1 = sheet.CreateRow(1);
                row1.CreateCell(0).SetCellValue("Alice");
                row1.CreateCell(1).SetCellValue("Ignored");
                row1.CreateCell(2).SetCellValue("10.5");

                var row2 = sheet.CreateRow(2);
                row2.CreateCell(0).SetCellValue("Bob");
                row2.CreateCell(1).SetCellValue("Ignored2");
                row2.CreateCell(2).SetCellValue("-3");

                wb.Write(fs);
            }

            var importer = new ExcelImporter();
            var settings = new ExcelImportSettings { HasHeader = true, IgnoreEmptyHeader = true };
            var dt = importer.ReadToDataTable(path, settings);

            Assert.That(dt.Columns.Count, Is.EqualTo(2));
            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("Name"));
            Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("Amount"));
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(dt.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(dt.Rows[0]["Amount"], Is.EqualTo(10.5m));
            Assert.That(dt.Rows[1]["Name"], Is.EqualTo("Bob"));
            Assert.That(dt.Rows[1]["Amount"], Is.EqualTo(-3m));
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
                Assert.That(ex.ValueSnapshot, Does.Contain("Path="));
                Assert.That(ex.ValueSnapshot, Does.Contain("Attempts="));
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

        private static string CreateMultiSheetXlsxWithActiveSecond()
        {
            var temp = Path.Combine(Path.GetTempPath(), "LbxyCommonLibTests_Excel", Guid.NewGuid().ToString("N") + "_multisheet.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(temp));
            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                IWorkbook wb = new XSSFWorkbook();

                var first = wb.CreateSheet("First");
                var firstHeader = first.CreateRow(0);
                firstHeader.CreateCell(0).SetCellValue("Name");
                firstHeader.CreateCell(1).SetCellValue("Value");
                var firstRow = first.CreateRow(1);
                firstRow.CreateCell(0).SetCellValue("First");
                firstRow.CreateCell(1).SetCellValue(1d);

                var second = wb.CreateSheet("Second");
                var secondHeader = second.CreateRow(0);
                secondHeader.CreateCell(0).SetCellValue("Name");
                secondHeader.CreateCell(1).SetCellValue("Value");
                var secondRow = second.CreateRow(1);
                secondRow.CreateCell(0).SetCellValue("Active");
                secondRow.CreateCell(1).SetCellValue(2d);

                wb.SetActiveSheet(1);
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

        [Test]
        public void ColumnNameToIndex_BasicCases_ShouldMatchExpected()
        {
            Assert.That(ExcelImporter.ColumnNameToIndex("A"), Is.EqualTo(0));
            Assert.That(ExcelImporter.ColumnNameToIndex("Z"), Is.EqualTo(25));
            Assert.That(ExcelImporter.ColumnNameToIndex("AA"), Is.EqualTo(26));
            Assert.That(ExcelImporter.ColumnNameToIndex("AZ"), Is.EqualTo(51));
            Assert.That(ExcelImporter.ColumnNameToIndex("BA"), Is.EqualTo(52));
            Assert.That(ExcelImporter.ColumnNameToIndex("ZZ"), Is.EqualTo(701));
            Assert.That(ExcelImporter.ColumnNameToIndex("AAA"), Is.EqualTo(702));
            Assert.That(ExcelImporter.ColumnNameToIndex("XFD"), Is.EqualTo(16383));
        }

        [Test]
        public void ColumnIndexToName_BasicCases_ShouldMatchExpected()
        {
            Assert.That(ExcelImporter.ColumnIndexToName(0), Is.EqualTo("A"));
            Assert.That(ExcelImporter.ColumnIndexToName(25), Is.EqualTo("Z"));
            Assert.That(ExcelImporter.ColumnIndexToName(26), Is.EqualTo("AA"));
            Assert.That(ExcelImporter.ColumnIndexToName(51), Is.EqualTo("AZ"));
            Assert.That(ExcelImporter.ColumnIndexToName(52), Is.EqualTo("BA"));
            Assert.That(ExcelImporter.ColumnIndexToName(701), Is.EqualTo("ZZ"));
            Assert.That(ExcelImporter.ColumnIndexToName(702), Is.EqualTo("AAA"));
            Assert.That(ExcelImporter.ColumnIndexToName(16383), Is.EqualTo("XFD"));
        }

        [Test]
        public void ColumnNameToIndex_InvalidInputs_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ExcelImporter.ColumnNameToIndex(null));
            Assert.Throws<ArgumentException>(() => ExcelImporter.ColumnNameToIndex(string.Empty));
            Assert.Throws<ArgumentException>(() => ExcelImporter.ColumnNameToIndex(" "));
            Assert.Throws<ArgumentException>(() => ExcelImporter.ColumnNameToIndex("A1"));
            Assert.Throws<ArgumentException>(() => ExcelImporter.ColumnNameToIndex("a"));
            Assert.Throws<ArgumentException>(() => ExcelImporter.ColumnNameToIndex("ABCD"));
        }

        [Test]
        public void ColumnIndexToName_InvalidInputs_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ExcelImporter.ColumnIndexToName(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ExcelImporter.ColumnIndexToName(16384));
        }

        [Test]
        public void ExcelColumnConverter_BasicConversions_ShouldMatchExpected()
        {
            Assert.That(ExcelColumnConverter.ColumnIndexToName(1), Is.EqualTo("A"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(26), Is.EqualTo("Z"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(27), Is.EqualTo("AA"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(52), Is.EqualTo("AZ"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(53), Is.EqualTo("BA"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(702), Is.EqualTo("ZZ"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(703), Is.EqualTo("AAA"));
            Assert.That(ExcelColumnConverter.ColumnIndexToName(16384), Is.EqualTo("XFD"));

            Assert.That(ExcelColumnConverter.ColumnNameToIndex("A"), Is.EqualTo(1));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("Z"), Is.EqualTo(26));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("AA"), Is.EqualTo(27));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("AZ"), Is.EqualTo(52));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("BA"), Is.EqualTo(53));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("ZZ"), Is.EqualTo(702));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("AAA"), Is.EqualTo(703));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("XFD"), Is.EqualTo(16384));
        }

        [Test]
        public void ExcelColumnConverter_ShouldBeCaseInsensitiveAndTrimmed()
        {
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("a"), Is.EqualTo(1));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex("z"), Is.EqualTo(26));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex(" aa "), Is.EqualTo(27));
            Assert.That(ExcelColumnConverter.ColumnNameToIndex(" xfd "), Is.EqualTo(16384));
        }

        [Test]
        public void ExcelColumnConverter_InvalidIndexInputs_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ExcelColumnConverter.ColumnIndexToName(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ExcelColumnConverter.ColumnIndexToName(16385));
        }

        [Test]
        public void ExcelColumnConverter_InvalidNameInputs_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ExcelColumnConverter.ColumnNameToIndex(null));
            Assert.Throws<ArgumentException>(() => ExcelColumnConverter.ColumnNameToIndex(string.Empty));
            Assert.Throws<ArgumentException>(() => ExcelColumnConverter.ColumnNameToIndex(" "));
            Assert.Throws<ArgumentOutOfRangeException>(() => ExcelColumnConverter.ColumnNameToIndex("ABCD"));
            Assert.Throws<ArgumentException>(() => ExcelColumnConverter.ColumnNameToIndex("A1"));
            Assert.Throws<ArgumentException>(() => ExcelColumnConverter.ColumnNameToIndex("!"));
        }
    }
}
