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

    /// <summary>
    /// 提供通用哈希计算与编码/解码辅助方法。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class HashUtil
    {
        /// <summary>
        /// 使用指定算法计算文件的哈希值。
        /// </summary>
        /// <param name="path">要计算的文件路径。</param>
        /// <param name="algorithm">哈希算法名称，例如 MD5、SHA256。</param>
        /// <returns>计算得到的哈希字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="algorithm"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="NotSupportedException">当指定的算法名称不受支持时抛出。</exception>
        /// <example>
        /// <code>
        /// var hash = HashUtil.ComputeFileHash("file.bin", "SHA256");
        /// var base64 = HashUtil.ToBase64(hash);
        /// </code>
        /// </example>
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

        /// <summary>
        /// 使用指定算法计算内存中数据的哈希值。
        /// </summary>
        /// <param name="data">要计算的字节数组。</param>
        /// <param name="algorithm">哈希算法名称，例如 MD5、SHA256。</param>
        /// <returns>计算得到的哈希字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 或 <paramref name="algorithm"/> 为 null 时抛出。</exception>
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

        /// <summary>
        /// 验证指定文件的哈希值是否与预期值一致。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="expectedHash">预期哈希值字节数组。</param>
        /// <param name="algorithm">哈希算法名称。</param>
        /// <returns>当实际哈希与预期哈希完全一致时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="expectedHash"/> 为 null 时抛出。</exception>
        public static bool VerifyFileHash(string path, byte[] expectedHash, string algorithm)
        {
            if (expectedHash == null)
            {
                throw new ArgumentNullException(nameof(expectedHash));
            }

            var actual = ComputeFileHash(path, algorithm);
            return AreEqual(expectedHash, actual);
        }

        /// <summary>
        /// 将字节数组编码为 Base64 字符串。
        /// </summary>
        /// <param name="bytes">要编码的字节数组。</param>
        /// <returns>Base64 编码后的字符串。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="bytes"/> 为 null 时抛出。</exception>
        public static string ToBase64(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 从 Base64 字符串解码为字节数组。
        /// </summary>
        /// <param name="s">Base64 编码的字符串。</param>
        /// <returns>解码后的字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="s"/> 为 null 时抛出。</exception>
        /// <exception cref="FormatException">当 <paramref name="s"/> 不是有效的 Base64 字符串时抛出。</exception>
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
