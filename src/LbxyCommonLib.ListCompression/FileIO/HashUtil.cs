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
    using System.Security.Cryptography;

    public static class HashUtil
    {
        public static byte[] ComputeFileHash(string path, string algorithm)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hasher = CreateHasher(algorithm))
            {
                return hasher.ComputeHash(stream);
            }
        }

        public static byte[] ComputeHash(byte[] data, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            using (var hasher = CreateHasher(algorithm))
            {
                return hasher.ComputeHash(data);
            }
        }

        public static bool VerifyFileHash(string path, byte[] expectedHash, string algorithm)
        {
            if (expectedHash == null)
            {
                throw new ArgumentNullException(nameof(expectedHash));
            }

            var actual = ComputeFileHash(path, algorithm);
            return AreEqual(expectedHash, actual);
        }

        public static string ToBase64(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Convert.ToBase64String(bytes);
        }

        public static byte[] FromBase64(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return Convert.FromBase64String(s);
        }

        private static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
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
#pragma warning restore CS1591

