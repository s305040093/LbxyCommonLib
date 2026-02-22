namespace LbxyCommonLib.FileIO.Tests
{
    using System.Text;
    using LbxyCommonLib.FileIO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class EccSignatureUtilTests
    {
        [Test]
        public void SignVerify_OnNetStandard()
        {
            var key = EccSignatureUtil.GenerateKeyPair();
#if NETFRAMEWORK
            Assert.That(key.Unsupported, Is.True);
#else
            var data = Encoding.UTF8.GetBytes("ECC test data");
            var sig = EccSignatureUtil.SignData(data, key);
            var ok = EccSignatureUtil.VerifyData(data, sig, key);
            Assert.That(ok, Is.True);
#endif
        }
    }
}

