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

    public sealed class EccKeyPair
    {
#if NETFRAMEWORK
        public bool Unsupported { get; set; }
#else
        public ECParameters PublicKey { get; set; }
        public ECParameters PrivateKey { get; set; }
#endif
    }

    public static class EccSignatureUtil
    {
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
