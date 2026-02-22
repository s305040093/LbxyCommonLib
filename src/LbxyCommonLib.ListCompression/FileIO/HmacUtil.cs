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

    public static class HmacUtil
    {
        public static byte[] ComputeFileHmac(string path, byte[] key, string algorithm)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
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
            using (var hmac = CreateHmac(key, algorithm))
            {
                return hmac.ComputeHash(stream);
            }
        }

        public static byte[] ComputeHmac(byte[] data, byte[] key, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            using (var hmac = CreateHmac(key, algorithm))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static bool VerifyFileHmac(string path, byte[] expectedHmac, byte[] key, string algorithm)
        {
            if (expectedHmac == null)
            {
                throw new ArgumentNullException(nameof(expectedHmac));
            }

            var actual = ComputeFileHmac(path, key, algorithm);
            return HashUtil.ToBase64(actual) == HashUtil.ToBase64(expectedHmac);
        }

        private static HMAC CreateHmac(byte[] key, string algorithm)
        {
            switch (algorithm.ToUpperInvariant())
            {
                case "HMACMD5":
                    return new HMACMD5(key);
                case "HMACSHA1":
                    return new HMACSHA1(key);
                case "HMACSHA256":
                    return new HMACSHA256(key);
                case "HMACSHA384":
                    return new HMACSHA384(key);
                case "HMACSHA512":
                    return new HMACSHA512(key);
                default:
                    throw new NotSupportedException("Unsupported HMAC algorithm: " + algorithm);
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

