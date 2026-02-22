#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1402

namespace LbxyCommonLib.FileIO
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 提供基于 <see cref="FileStream"/> 的简单异步文件读写帮助方法。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class AsyncFileIO
    {
        /// <summary>
        /// 异步将整个字节数组写入到指定路径的文件中。
        /// </summary>
        /// <param name="path">目标文件路径，必须为非空且非空字符串。</param>
        /// <param name="data">要写入的字节数组，不能为 null。</param>
        /// <param name="overwrite">当目标文件已存在时，指示是否覆盖（true 为覆盖，false 时若存在则抛异常）。</param>
        /// <param name="cancellationToken">用于取消写入操作的取消标记。</param>
        /// <returns>表示异步写入操作的 <see cref="Task"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="data"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="IOException">当文件系统 I/O 发生错误时抛出。</exception>
        /// <remarks>
        /// 若目标目录不存在，将自动创建；写入完成后会调用 <see cref="FileStream.FlushAsync(CancellationToken)"/> 确保数据持久化。
        /// </remarks>
        /// <example>
        /// <code>
        /// var data = Encoding.UTF8.GetBytes("hello");
        /// await AsyncFileIO.WriteAsync("output.bin", data, overwrite: true, CancellationToken.None);
        /// </code>
        /// </example>
        public static async Task WriteAsync(string path, byte[] data, bool overwrite, CancellationToken cancellationToken)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (var fs = new FileStream(path, mode, FileAccess.Write, FileShare.None, 4096, true))
            {
                await fs.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 异步读取指定路径文件的全部内容并返回字节数组。
        /// </summary>
        /// <param name="path">要读取的文件路径，必须为非空且非空字符串。</param>
        /// <param name="cancellationToken">用于取消读取操作的取消标记。</param>
        /// <returns>包含文件全部内容的字节数组；当文件长度为 0 时返回空数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="IOException">
        /// 当文件过大而无法加载到单个字节数组，或在读取过程中遇到意外的文件结束时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// var bytes = await AsyncFileIO.ReadAsync("output.bin", CancellationToken.None);
        /// </code>
        /// </example>
        public static async Task<byte[]> ReadAsync(string path, CancellationToken cancellationToken)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                var length = fs.Length;
                if (length == 0)
                {
                    return new byte[0];
                }

                if (length > int.MaxValue)
                {
                    throw new IOException("File is too large to load into a single byte array.");
                }

                var buffer = new byte[(int)length];
                var offset = 0;
                while (offset < buffer.Length)
                {
                    var read = await fs.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw new IOException("Unexpected end of file while reading.");
                    }

                    offset += read;
                }

                return buffer;
            }
        }
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore CS1591
