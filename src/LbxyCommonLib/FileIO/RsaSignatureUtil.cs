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
    /// 表示一对 RSA 公钥/私钥参数，用于数字签名与验证。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class RsaKeyPair
    {
        /// <summary>
        /// 获取或设置 RSA 公钥参数。
        /// </summary>
        public RSAParameters PublicKey { get; set; }

        /// <summary>
        /// 获取或设置 RSA 私钥参数。
        /// </summary>
        public RSAParameters PrivateKey { get; set; }
    }

    /// <summary>
    /// 提供基于 RSA 的数据与文件签名及验证辅助方法。
    /// </summary>
    public static class RsaSignatureUtil
    {
        /// <summary>
        /// 生成一对新的 RSA 公钥/私钥。
        /// </summary>
        /// <param name="keySize">密钥长度（位），必须大于等于 1024。</param>
        /// <returns>包含公钥与私钥参数的 <see cref="RsaKeyPair"/> 实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="keySize"/> 小于 1024 时抛出。</exception>
        public static RsaKeyPair GenerateKeyPair(int keySize)
        {
            if (keySize < 1024)
            {
                throw new ArgumentOutOfRangeException(nameof(keySize));
            }

            using (var rsa = CreateRsa(keySize))
            {
                return new RsaKeyPair
                {
                    PublicKey = rsa.ExportParameters(false),
                    PrivateKey = rsa.ExportParameters(true),
                };
            }
        }

        /// <summary>
        /// 使用指定 RSA 私钥对数据进行签名，默认采用 SHA256 哈希和 PKCS#1 填充。
        /// </summary>
        /// <param name="data">要签名的原始数据。</param>
        /// <param name="privateKey">RSA 私钥参数。</param>
        /// <returns>生成的签名字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 为 null 时抛出。</exception>
        public static byte[] SignData(byte[] data, RSAParameters privateKey)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var rsa = CreateRsa())
            {
                rsa.ImportParameters(privateKey);
#if NETFRAMEWORK
                var csp = (RSACryptoServiceProvider)rsa;
                return csp.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
#else
                return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
            }
        }

        /// <summary>
        /// 使用指定 RSA 公钥验证数据与签名是否匹配。
        /// </summary>
        /// <param name="data">原始数据。</param>
        /// <param name="signature">签名字节数组。</param>
        /// <param name="publicKey">RSA 公钥参数。</param>
        /// <returns>当签名有效时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="data"/> 或 <paramref name="signature"/> 为 null 时抛出。
        /// </exception>
        public static bool VerifyData(byte[] data, byte[] signature, RSAParameters publicKey)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            using (var rsa = CreateRsa())
            {
                rsa.ImportParameters(publicKey);
#if NETFRAMEWORK
                var csp = (RSACryptoServiceProvider)rsa;
                return csp.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature);
#else
                return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
            }
        }

        /// <summary>
        /// 使用指定 RSA 私钥对文件内容进行签名。
        /// </summary>
        /// <param name="path">要签名的文件路径。</param>
        /// <param name="privateKey">RSA 私钥参数。</param>
        /// <returns>生成的签名字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <example>
        /// <code>
        /// var keys = RsaSignatureUtil.GenerateKeyPair(2048);
        /// var signature = RsaSignatureUtil.SignFile("file.bin", keys.PrivateKey);
        /// var ok = RsaSignatureUtil.VerifyFile("file.bin", signature, keys.PublicKey);
        /// </code>
        /// </example>
        public static byte[] SignFile(string path, RSAParameters privateKey)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            var data = File.ReadAllBytes(path);
            return SignData(data, privateKey);
        }

        /// <summary>
        /// 使用指定 RSA 公钥验证文件内容与签名是否匹配。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="signature">签名字节数组。</param>
        /// <param name="publicKey">RSA 公钥参数。</param>
        /// <returns>当签名有效时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        public static bool VerifyFile(string path, byte[] signature, RSAParameters publicKey)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            var data = File.ReadAllBytes(path);
            return VerifyData(data, signature, publicKey);
        }

        private static RSA CreateRsa(int keySize = 2048)
        {
#if NETFRAMEWORK
            var rsa = new RSACryptoServiceProvider(keySize);
            return rsa;
#else
            var rsa = RSA.Create();
            rsa.KeySize = keySize;
            return rsa;
#endif
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
