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
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// 表示按固定大小分块计算文件哈希值时使用的清单信息。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class ChunkManifest
    {
        /// <summary>
        /// 获取或设置分块大小（以字节为单位），必须为正整数。
        /// </summary>
        public int ChunkSize { get; set; }

        /// <summary>
        /// 获取或设置使用的哈希算法名称，例如 MD5、SHA256 等。
        /// </summary>
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置按照文件顺序计算得到的每个块的哈希值列表。
        /// </summary>
        public List<byte[]> Hashes { get; set; } = new List<byte[]>();
    }

    /// <summary>
    /// 提供基于分块哈希的文件完整性校验辅助方法。
    /// </summary>
    public static class ChunkManifestUtil
    {
        /// <summary>
        /// 为指定文件构建分块哈希清单。
        /// </summary>
        /// <param name="path">要计算的文件路径。</param>
        /// <param name="chunkSize">分块大小（字节），必须为正整数。</param>
        /// <param name="algorithm">哈希算法名称，例如 MD5、SHA256。</param>
        /// <returns>包含每个块哈希值的 <see cref="ChunkManifest"/> 实例。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 或 <paramref name="algorithm"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="chunkSize"/> 小于等于 0 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="NotSupportedException">当指定的算法名称不受支持时抛出。</exception>
        /// <example>
        /// <code>
        /// var manifest = ChunkManifestUtil.BuildFileManifest("large.bin", 4096, "SHA256");
        /// </code>
        /// </example>
        public static ChunkManifest BuildFileManifest(string path, int chunkSize, string algorithm)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            var manifest = new ChunkManifest
            {
                ChunkSize = chunkSize,
                Algorithm = algorithm,
                Hashes = new List<byte[]>(),
            };

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hasher = CreateHasher(algorithm))
            {
                var buffer = new byte[chunkSize];
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }

                    hasher.Initialize();
                    var hash = hasher.ComputeHash(buffer, 0, read);
                    manifest.Hashes.Add(hash);
                }
            }

            return manifest;
        }

        /// <summary>
        /// 验证指定文件与给定清单是否一致。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="manifest">预先生成的分块哈希清单。</param>
        /// <returns>当文件大小及所有块哈希都与清单一致时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="manifest"/> 为 null 时抛出。</exception>
        public static bool VerifyFileAgainstManifest(string path, ChunkManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            var rebuilt = BuildFileManifest(path, manifest.ChunkSize, manifest.Algorithm);
            if (rebuilt.Hashes.Count != manifest.Hashes.Count)
            {
                return false;
            }

            for (var i = 0; i < manifest.Hashes.Count; i++)
            {
                if (!AreEqual(manifest.Hashes[i], rebuilt.Hashes[i]))
                {
                    return false;
                }
            }

            return true;
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
#pragma warning restore SA1513
#pragma warning restore CS1591
