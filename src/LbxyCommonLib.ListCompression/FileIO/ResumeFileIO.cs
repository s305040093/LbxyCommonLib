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

    public static class ResumeFileIO
    {
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
