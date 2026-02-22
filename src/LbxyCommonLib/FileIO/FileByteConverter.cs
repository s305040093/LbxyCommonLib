#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1629
#pragma warning disable SA1513
#pragma warning disable SA1402

namespace LbxyCommonLib.FileIO
{
    using System;
    using System.IO;

    /// <summary>
    /// 提供基于字节数组的简单文件读写辅助方法（同步版本）。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class FileByteConverter
    {
        /// <summary>
        /// 将整个字节数组写入到指定路径的文件中。
        /// </summary>
        /// <param name="path">目标文件路径，必须为非空且非空字符串。</param>
        /// <param name="data">要写入的字节数组，不能为 null。</param>
        /// <param name="overwrite">当目标文件已存在时，指示是否覆盖（true 为覆盖，false 时若存在则抛异常）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="data"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="IOException">当文件系统 I/O 发生错误时抛出。</exception>
        /// <remarks>若目标目录不存在，将自动创建。</remarks>
        /// <example>
        /// <code>
        /// var data = Encoding.UTF8.GetBytes("content");
        /// FileByteConverter.Write("file.bin", data, overwrite: true);
        /// </code>
        /// </example>
        public static void Write(string path, byte[] data, bool overwrite)
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

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var mode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (var fs = new FileStream(path, mode, FileAccess.Write, FileShare.None))
            {
                fs.Write(data, 0, data.Length);
                fs.Flush(true);
            }
        }

        /// <summary>
        /// 尝试写入文件，并在出现异常时捕获错误信息而不是抛出。
        /// </summary>
        /// <param name="path">目标文件路径。</param>
        /// <param name="data">要写入的字节数组。</param>
        /// <param name="overwrite">是否覆盖已存在的文件。</param>
        /// <param name="errorMessage">写入失败时输出的错误消息；成功时为空字符串。</param>
        /// <returns>写入成功返回 true，失败返回 false。</returns>
        public static bool TryWrite(string path, byte[] data, bool overwrite, out string errorMessage)
        {
            try
            {
                Write(path, data, overwrite);
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 读取指定路径文件的全部内容并返回字节数组。
        /// </summary>
        /// <param name="path">要读取的文件路径，必须为非空且非空字符串。</param>
        /// <returns>包含文件全部内容的字节数组；当文件长度为 0 时返回空数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="IOException">
        /// 当文件过大而无法加载到单个字节数组，或在读取过程中遇到意外的文件结束时抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// var bytes = FileByteConverter.Read("file.bin");
        /// </code>
        /// </example>
        public static byte[] Read(string path)
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

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                    var read = fs.Read(buffer, offset, buffer.Length - offset);
                    if (read == 0)
                    {
                        throw new IOException("Unexpected end of file while reading.");
                    }
                    offset += read;
                }

                return buffer;
            }
        }

        /// <summary>
        /// 尝试读取文件，并在出现异常时捕获错误信息而不是抛出。
        /// </summary>
        /// <param name="path">要读取的文件路径。</param>
        /// <param name="data">读取成功时输出的字节数组，失败时为长度为 0 的数组。</param>
        /// <param name="errorMessage">读取失败时输出的错误消息；成功时为空字符串。</param>
        /// <returns>读取成功返回 true，失败返回 false。</returns>
        public static bool TryRead(string path, out byte[] data, out string errorMessage)
        {
            try
            {
                data = Read(path);
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                data = new byte[0];
                errorMessage = ex.Message;
                return false;
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
#pragma warning restore SA1513
#pragma warning restore SA1629
#pragma warning restore CS1591

