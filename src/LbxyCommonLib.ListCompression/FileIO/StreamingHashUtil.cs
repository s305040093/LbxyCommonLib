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
    using System.Security.Cryptography;

    public static class StreamingHashUtil
    {
        public static byte[] ComputeFileHashStreamed(string path, string algorithm, int bufferSize)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hasher = CreateHasher(algorithm))
            {
                var buffer = new byte[bufferSize];
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    hasher.TransformBlock(buffer, 0, read, null, 0);
                }

                hasher.TransformFinalBlock(buffer, 0, 0);
                return hasher.Hash;
            }
        }

        public static bool VerifyFileHashStreamed(string path, byte[] expectedHash, string algorithm, int bufferSize)
        {
            var actual = ComputeFileHashStreamed(path, algorithm, bufferSize);
            return HashUtil.ToBase64(actual) == HashUtil.ToBase64(expectedHash);
        }

        private static HashAlgorithm CreateHasher(string algorithm)
        {
            switch (algorithm.ToUpperInvariant())
            {
                case "MD5":
                    return MD5.Create();
                case "SHA1":
                    return SHA1.Create();
                case "SHA256":
                    return SHA256.Create();
                case "SHA384":
                    return SHA384.Create();
                case "SHA512":
                    return SHA512.Create();
                default:
                    throw new NotSupportedException("Unsupported hash algorithm: " + algorithm);
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
