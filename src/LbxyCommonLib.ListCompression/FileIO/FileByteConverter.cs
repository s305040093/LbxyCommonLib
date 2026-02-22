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

    public static class FileByteConverter
    {
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
#pragma warning restore CS1591

