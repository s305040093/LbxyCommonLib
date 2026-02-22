#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1513
#pragma warning disable SA1402

namespace LbxyCommonLib.FileIO
{
    using System;
    using System.IO;

    /// <summary>
    /// 提供支持偏移写入与范围读取的文件 I/O 工具，适用于断点续传等场景。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class ResumeFileIO
    {
        /// <summary>
        /// 在指定文件的给定偏移处写入数据，必要时可创建文件并填充空洞。
        /// </summary>
        /// <param name="path">目标文件路径，必须为非空且非空字符串。</param>
        /// <param name="data">要写入的字节数组，不能为 null。</param>
        /// <param name="offset">写入起始偏移（字节），必须为非负数。</param>
        /// <param name="createIfMissing">当文件不存在时，指示是否自动创建新文件。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="data"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 小于 0 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在且 <paramref name="createIfMissing"/> 为 false 时抛出。</exception>
        /// <remarks>
        /// 若偏移大于当前文件长度，将以零填充中间空洞直至达到偏移位置，然后写入数据。
        /// </remarks>
        /// <example>
        /// <code>
        /// // 将数据写入文件 1 MB 偏移处
        /// ResumeFileIO.WriteAtOffset("resume.bin", chunk, 1024L * 1024L, createIfMissing: true);
        /// </code>
        /// </example>
        public static void WriteAtOffset(string path, byte[] data, long offset, bool createIfMissing)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var mode = FileMode.Open;
            if (!File.Exists(path))
            {
                if (!createIfMissing)
                {
                    throw new FileNotFoundException("File not found.", path);
                }

                mode = FileMode.CreateNew;
            }

            using (var fs = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.None))
            {
                if (offset > fs.Length)
                {
                    fs.Seek(0, SeekOrigin.End);
                    var gap = offset - fs.Length;
                    if (gap > 0)
                    {
                        var zeros = new byte[Math.Min(gap, 8192)];
                        while (fs.Length < offset)
                        {
                            var toWrite = (int)Math.Min(zeros.Length, offset - fs.Length);
                            fs.Write(zeros, 0, toWrite);
                        }
                    }
                }
                else
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                }

                fs.Write(data, 0, data.Length);
                fs.Flush(true);
            }
        }

        /// <summary>
        /// 以追加方式在文件末尾写入数据，必要时可创建新文件。
        /// </summary>
        /// <param name="path">目标文件路径。</param>
        /// <param name="data">要写入的字节数组。</param>
        /// <param name="createIfMissing">当文件不存在时，指示是否自动创建新文件。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="data"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在且 <paramref name="createIfMissing"/> 为 false 时抛出。</exception>
        public static void Append(string path, byte[] data, bool createIfMissing)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var mode = FileMode.Append;
            if (!File.Exists(path))
            {
                if (!createIfMissing)
                {
                    throw new FileNotFoundException("File not found.", path);
                }
                mode = FileMode.CreateNew;
            }

            using (var fs = new FileStream(path, mode, FileAccess.Write, FileShare.None))
            {
                fs.Write(data, 0, data.Length);
                fs.Flush(true);
            }
        }

        /// <summary>
        /// 从指定文件的给定偏移处读取指定长度的字节数据。
        /// </summary>
        /// <param name="path">要读取的文件路径。</param>
        /// <param name="offset">读取起始偏移（字节），必须为非负数。</param>
        /// <param name="length">要读取的最大字节数，必须为非负数。</param>
        /// <returns>
        /// 实际读取到的字节数组；当请求长度超过文件末尾时，返回长度可能小于 <paramref name="length"/>。
        /// </returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="offset"/> 或 <paramref name="length"/> 小于 0 时抛出。
        /// </exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="IOException">当偏移大于文件长度时抛出。</exception>
        /// <example>
        /// <code>
        /// var part = ResumeFileIO.ReadRange("resume.bin", 1024L * 1024L, 4096);
        /// </code>
        /// </example>
        public static byte[] ReadRange(string path, long offset, int length)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (offset > fs.Length)
                {
                    throw new IOException("Offset beyond end of file.");
                }

                fs.Seek(offset, SeekOrigin.Begin);
                var buffer = new byte[length];
                var readTotal = 0;
                while (readTotal < length)
                {
                    var read = fs.Read(buffer, readTotal, length - readTotal);
                    if (read == 0)
                    {
                        break;
                    }
                    readTotal += read;
                }

                if (readTotal == length)
                {
                    return buffer;
                }

                var trimmed = new byte[readTotal];
                Buffer.BlockCopy(buffer, 0, trimmed, 0, readTotal);
                return trimmed;
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
#pragma warning restore CS1591
