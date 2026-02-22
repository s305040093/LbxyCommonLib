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

    public sealed class ChunkManifest
    {
        public int ChunkSize { get; set; }

        public string Algorithm { get; set; } = string.Empty;

        public List<byte[]> Hashes { get; set; } = new List<byte[]>();
    }

    public static class ChunkManifestUtil
    {
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
