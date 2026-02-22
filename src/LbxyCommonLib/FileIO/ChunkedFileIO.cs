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
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// 提供按块写入和读取文件的辅助方法，适合处理大文件或流式场景。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class ChunkedFileIO
    {
        /// <summary>
        /// 将一系列字节块顺序写入到指定路径的文件中。
        /// </summary>
        /// <param name="path">目标文件路径，必须为非空且非空字符串。</param>
        /// <param name="chunks">要写入的字节块序列，按枚举顺序写入。</param>
        /// <param name="overwrite">当目标文件已存在时，指示是否覆盖（true 为覆盖，false 时若存在则抛异常）。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="chunks"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="IOException">当文件系统 I/O 发生错误时抛出。</exception>
        /// <remarks>若目标目录不存在，将自动创建；空的 <see cref="ArraySegment{T}.Array"/> 会被跳过。</remarks>
        /// <example>
        /// <code>
        /// var chunks = new List&lt;ArraySegment&lt;byte&gt;&gt;
        /// {
        ///     new ArraySegment&lt;byte&gt;(part1),
        ///     new ArraySegment&lt;byte&gt;(part2),
        /// };
        /// ChunkedFileIO.WriteChunks("big.bin", chunks, overwrite: true);
        /// </code>
        /// </example>
        public static void WriteChunks(string path, IEnumerable<ArraySegment<byte>> chunks, bool overwrite)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (chunks == null)
            {
                throw new ArgumentNullException(nameof(chunks));
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
            using (var fs = new FileStream(path, mode, FileAccess.Write, FileShare.None))
            {
                foreach (var seg in chunks)
                {
                    if (seg.Array == null)
                    {
                        continue;
                    }

                    fs.Write(seg.Array, seg.Offset, seg.Count);
                }

                fs.Flush(true);
            }
        }

        /// <summary>
        /// 以指定块大小顺序读取文件内容，并按块返回字节数组序列。
        /// </summary>
        /// <param name="path">要读取的文件路径，必须为非空且非空字符串。</param>
        /// <param name="chunkSize">读取块大小（字节），必须为正整数。</param>
        /// <returns>按文件顺序返回的字节数组序列；最后一个块大小可能小于 <paramref name="chunkSize"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 <paramref name="path"/> 为空字符串时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="chunkSize"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <example>
        /// <code>
        /// foreach (var block in ChunkedFileIO.ReadChunks("big.bin", 4096))
        /// {
        ///     // 逐块处理数据
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<byte[]> ReadChunks(string path, int chunkSize)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[chunkSize];
                while (true)
                {
                    var read = fs.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        yield break;
                    }

                    if (read == buffer.Length)
                    {
                        yield return buffer;
                        buffer = new byte[chunkSize];
                    }
                    else
                    {
                        var last = new byte[read];
                        Buffer.BlockCopy(buffer, 0, last, 0, read);
                        yield return last;
                        yield break;
                    }
                }
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
