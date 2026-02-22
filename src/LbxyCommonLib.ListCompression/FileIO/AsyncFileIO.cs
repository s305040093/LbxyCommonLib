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

    public static class AsyncFileIO
    {
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

