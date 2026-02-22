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
    /// 提供基于对称密钥的 HMAC 计算与验证辅助方法。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class HmacUtil
    {
        /// <summary>
        /// 使用指定算法和密钥计算文件的 HMAC 值。
        /// </summary>
        /// <param name="path">要计算的文件路径。</param>
        /// <param name="key">用于 HMAC 计算的对称密钥。</param>
        /// <param name="algorithm">HMAC 算法名称，例如 HMACSHA256。</param>
        /// <returns>计算得到的 HMAC 字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/>、<paramref name="key"/> 或 <paramref name="algorithm"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="NotSupportedException">当指定的 HMAC 算法名称不受支持时抛出。</exception>
        /// <example>
        /// <code>
        /// var key = Encoding.UTF8.GetBytes("secret");
        /// var mac = HmacUtil.ComputeFileHmac("file.bin", key, "HMACSHA256");
        /// </code>
        /// </example>
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

        /// <summary>
        /// 使用指定算法和密钥计算内存数据的 HMAC 值。
        /// </summary>
        /// <param name="data">要计算的字节数组。</param>
        /// <param name="key">用于 HMAC 计算的对称密钥。</param>
        /// <param name="algorithm">HMAC 算法名称。</param>
        /// <returns>计算得到的 HMAC 字节数组。</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="data"/>、<paramref name="key"/> 或 <paramref name="algorithm"/> 为 null 时抛出。
        /// </exception>
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

        /// <summary>
        /// 验证指定文件的 HMAC 值是否与预期值一致。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="expectedHmac">预期的 HMAC 字节数组。</param>
        /// <param name="key">用于 HMAC 计算的对称密钥。</param>
        /// <param name="algorithm">HMAC 算法名称。</param>
        /// <returns>当实际 HMAC 与预期值一致时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="expectedHmac"/> 为 null 时抛出。</exception>
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
