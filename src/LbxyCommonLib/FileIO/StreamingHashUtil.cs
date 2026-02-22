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

    /// <summary>
    /// 提供流式哈希计算工具，适合处理大文件或受限内存环境。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class StreamingHashUtil
    {
        /// <summary>
        /// 使用指定算法以流式方式计算文件的哈希值，避免一次性将文件全部加载到内存。
        /// </summary>
        /// <param name="path">要计算的文件路径。</param>
        /// <param name="algorithm">哈希算法名称，例如 MD5、SHA256。</param>
        /// <param name="bufferSize">每次读取的缓冲区大小（字节），必须为正整数。</param>
        /// <returns>计算得到的哈希字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="algorithm"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="bufferSize"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="NotSupportedException">当指定的算法名称不受支持时抛出。</exception>
        /// <example>
        /// <code>
        /// var hash = StreamingHashUtil.ComputeFileHashStreamed("large.bin", "SHA256", 81920);
        /// </code>
        /// </example>
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

        /// <summary>
        /// 以流式方式验证指定文件的哈希值是否与预期值一致。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="expectedHash">预期哈希值字节数组。</param>
        /// <param name="algorithm">哈希算法名称。</param>
        /// <param name="bufferSize">每次读取的缓冲区大小（字节）。</param>
        /// <returns>当实际哈希与预期哈希一致时返回 true，否则返回 false。</returns>
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
