namespace LbxyCommonLib.FileIO.Tests
{
    using System;
    using System.Text;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class RsaSignatureUtilTests
    {
        [Test]
        public void GenerateSignVerify_Rsa()
        {
            var pair = RsaSignatureUtil.GenerateKeyPair(2048);
            var data = Encoding.UTF8.GetBytes("Hello RSA!");
            var sig = RsaSignatureUtil.SignData(data, pair.PrivateKey);

            var ok = RsaSignatureUtil.VerifyData(data, sig, pair.PublicKey);
            Assert.That(ok, Is.True);
        }

        [Test]
        public void Verify_Fails_WhenDataTampered()
        {
            var pair = RsaSignatureUtil.GenerateKeyPair(2048);
            var data = Encoding.UTF8.GetBytes("Original");
            var sig = RsaSignatureUtil.SignData(data, pair.PrivateKey);

            var tampered = Encoding.UTF8.GetBytes("Changed");
            var ok = RsaSignatureUtil.VerifyData(tampered, sig, pair.PublicKey);
            Assert.That(ok, Is.False);
        }
    }
}

