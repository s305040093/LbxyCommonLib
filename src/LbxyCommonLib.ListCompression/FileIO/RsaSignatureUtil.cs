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

    public sealed class RsaKeyPair
    {
        public RSAParameters PublicKey { get; set; }

        public RSAParameters PrivateKey { get; set; }
    }

    public static class RsaSignatureUtil
    {
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
