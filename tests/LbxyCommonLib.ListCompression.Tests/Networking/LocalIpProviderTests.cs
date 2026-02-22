namespace LbxyCommonLib.Networking.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using LbxyCommonLib.Networking;
    using NUnit.Framework;

    [TestFixture]
    public sealed class LocalIpProviderTests
    {
        [Test]
        public void GetLocalIPv4Addresses_Returns_NoLoopback()
        {
            IReadOnlyList<string> ips = null;
            Assert.DoesNotThrow(() => ips = LocalIpProvider.GetLocalIPv4Addresses());
            Assert.That(ips, Is.Not.Null);

            for (var i = 0; i < ips.Count; i++)
            {
                var ip = ips[i];
                Assert.That(ip, Is.Not.Null.And.Not.Empty);
                Assert.That(ip.StartsWith("127.", StringComparison.Ordinal), Is.False);
            }
        }

        [Test]
        public async Task GetLocalIPv4AddressesAsync_Returns_Consistent_Or_Empty()
        {
            var sync = LocalIpProvider.GetLocalIPv4Addresses();
            var asyncIps = await LocalIpProvider.GetLocalIPv4AddressesAsync();
            Assert.That(asyncIps, Is.Not.Null);

            // Either empty (due to environment) or equals sync set
            if (asyncIps.Count > 0 && sync.Count > 0)
            {
                Assert.That(asyncIps, Is.EquivalentTo(sync));
            }
        }

        [Test]
        public async Task GetLocalIPv4AddressesAsync_Cancellation_ReturnsEmpty()
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();
                var asyncIps = await LocalIpProvider.GetLocalIPv4AddressesAsync(cts.Token);
                Assert.That(asyncIps, Is.Not.Null);
                Assert.That(asyncIps.Count, Is.EqualTo(0));
            }
        }
    }
}
