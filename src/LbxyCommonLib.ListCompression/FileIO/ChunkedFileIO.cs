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

    public static class ChunkedFileIO
    {
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

