#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1513
#pragma warning disable SA1516
#pragma warning disable SA1402

namespace LbxyCommonLib.FileIO
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// 表示一对 ECC 公钥/私钥参数，用于椭圆曲线数字签名。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public sealed class EccKeyPair
    {
#if NETFRAMEWORK
        /// <summary>
        /// 在 .NET Framework 目标下指示当前环境不支持 ECC 签名。
        /// </summary>
        public bool Unsupported { get; set; }
#else
        /// <summary>
        /// 获取或设置 ECC 公钥参数。
        /// </summary>
        public ECParameters PublicKey { get; set; }

        /// <summary>
        /// 获取或设置 ECC 私钥参数。
        /// </summary>
        public ECParameters PrivateKey { get; set; }
#endif
    }

    /// <summary>
    /// 提供基于 ECC 的数据与文件签名及验证辅助方法。
    /// </summary>
    public static class EccSignatureUtil
    {
        /// <summary>
        /// 生成一对新的 ECC 公钥/私钥。
        /// </summary>
        /// <returns>包含公钥与私钥参数的 <see cref="EccKeyPair"/> 实例。</returns>
        /// <exception cref="NotSupportedException">在不支持 ECC 的目标平台（如部分 .NET Framework 目标）上抛出。</exception>
        public static EccKeyPair GenerateKeyPair()
        {
#if NETFRAMEWORK
            return new EccKeyPair { Unsupported = true };
#else
            using (var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256))
            {
                return new EccKeyPair
                {
                    PublicKey = ecdsa.ExportParameters(false),
                    PrivateKey = ecdsa.ExportParameters(true),
                };
            }
#endif
        }

        /// <summary>
        /// 使用指定 ECC 私钥对数据进行签名。
        /// </summary>
        /// <param name="data">要签名的原始数据。</param>
        /// <param name="keyPair">包含私钥（以及可选公钥）的密钥对。</param>
        /// <returns>生成的签名字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="data"/> 为 null 时抛出。</exception>
        /// <exception cref="NotSupportedException">在不支持 ECC 的目标平台上抛出。</exception>
        public static byte[] SignData(byte[] data, EccKeyPair keyPair)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

#if NETFRAMEWORK
            throw new NotSupportedException("ECC signatures are not supported on .NET Framework target in this module.");
#else
            using (var ecdsa = ECDsa.Create(keyPair.PrivateKey))
            {
                return ecdsa.SignData(data, HashAlgorithmName.SHA256);
            }
#endif
        }

        /// <summary>
        /// 使用指定 ECC 公钥验证数据与签名是否匹配。
        /// </summary>
        /// <param name="data">原始数据。</param>
        /// <param name="signature">签名字节数组。</param>
        /// <param name="keyPair">包含公钥参数的密钥对。</param>
        /// <returns>当签名有效时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="data"/> 或 <paramref name="signature"/> 为 null 时抛出。
        /// </exception>
        /// <exception cref="NotSupportedException">在不支持 ECC 的目标平台上抛出。</exception>
        public static bool VerifyData(byte[] data, byte[] signature, EccKeyPair keyPair)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

#if NETFRAMEWORK
            throw new NotSupportedException("ECC signatures are not supported on .NET Framework target in this module.");
#else
            using (var ecdsa = ECDsa.Create(keyPair.PublicKey))
            {
                return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
            }
#endif
        }

        /// <summary>
        /// 使用指定 ECC 私钥对文件内容进行签名。
        /// </summary>
        /// <param name="path">要签名的文件路径。</param>
        /// <param name="keyPair">包含私钥参数的密钥对。</param>
        /// <returns>生成的签名字节数组。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <example>
        /// <code>
        /// var keyPair = EccSignatureUtil.GenerateKeyPair();
        /// var signature = EccSignatureUtil.SignFile("file.bin", keyPair);
        /// var ok = EccSignatureUtil.VerifyFile("file.bin", signature, keyPair);
        /// </code>
        /// </example>
        public static byte[] SignFile(string path, EccKeyPair keyPair)
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
            return SignData(data, keyPair);
        }

        /// <summary>
        /// 使用指定 ECC 公钥验证文件内容与签名是否匹配。
        /// </summary>
        /// <param name="path">要验证的文件路径。</param>
        /// <param name="signature">签名字节数组。</param>
        /// <param name="keyPair">包含公钥参数的密钥对。</param>
        /// <returns>当签名有效时返回 true，否则返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        public static bool VerifyFile(string path, byte[] signature, EccKeyPair keyPair)
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
            return VerifyData(data, signature, keyPair);
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
#pragma warning restore SA1516
#pragma warning restore SA1513
#pragma warning restore CS1591
