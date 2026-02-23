#pragma warning disable CS1591
#pragma warning disable SA1633
#pragma warning disable SA1600
#pragma warning disable SA1649
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1518
#pragma warning disable SA1201
#pragma warning disable SA1602
#pragma warning disable SA1601

namespace LbxyCommonLib.ExcelImport
{
    public sealed partial class ExcelImporter
    {
        /// <summary>
        /// 定义在分块导出过程中处理块余数情况的回调接口。
        /// </summary>
        public interface IMatrixRemainderHandler
        {
            /// <summary>
            /// 处理当前矩阵块余数上下文并返回要采取的操作。
            /// </summary>
            /// <param name="context">当前块划分与余数信息上下文。</param>
            /// <returns>调用方应采取的余数处理策略。</returns>
            MatrixRemainderAction Handle(MatrixRemainderContext context);
        }
    }
}
