#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1629
#pragma warning disable SA1116
#pragma warning disable SA1117
#pragma warning disable SA1402

namespace LbxyCommonLib.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides local IPv4 addresses from active non-virtual network adapters, excluding loopback.
    /// </summary>
    public static class LocalIpProvider
    {
        /// <summary>
        /// Gets local IPv4 addresses from active non-virtual adapters (excludes loopback).
        /// </summary>
        /// <returns>Distinct IPv4 addresses as strings; returns empty list on errors.</returns>
        /// <code>
        /// var ips = LocalIpProvider.GetLocalIPv4Addresses();
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> GetLocalIPv4Addresses()
        {
            try
            {
                var result = new List<string>();
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                for (var i = 0; i < adapters.Length; i++)
                {
                    var ni = adapters[i];
                    if (!IsCandidateAdapter(ni))
                    {
                        continue;
                    }

                    IPInterfaceProperties props;
                    try
                    {
                        props = ni.GetIPProperties();
                    }
                    catch
                    {
                        // Some virtual/tunnel interfaces may throw; skip.
                        continue;
                    }

                    var unicast = props.UnicastAddresses;
                    for (var j = 0; j < unicast.Count; j++)
                    {
                        var info = unicast[j];
                        if (info.Address != null && info.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var ip = info.Address;
                            if (!IPAddress.IsLoopback(ip))
                            {
                                result.Add(ip.ToString());
                            }
                        }
                    }
                }

                // Distinct + stable ordering
                return result.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Gets local IPv4 addresses asynchronously from active non-virtual adapters (excludes loopback).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token. If cancelled, returns empty list.</param>
        /// <returns>Distinct IPv4 addresses as strings; returns empty list on errors or cancellation.</returns>
        /// <code>
        /// var ips = await LocalIpProvider.GetLocalIPv4AddressesAsync(ct);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<IReadOnlyList<string>> GetLocalIPv4AddressesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return GetLocalIPv4Addresses();
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        private static bool IsCandidateAdapter(NetworkInterface ni)
        {
            if (ni == null)
            {
                return false;
            }

            if (ni.OperationalStatus != OperationalStatus.Up)
            {
                return false;
            }

            var type = ni.NetworkInterfaceType;
            if (type == NetworkInterfaceType.Loopback || type == NetworkInterfaceType.Tunnel)
            {
                return false;
            }

            var name = (ni.Name ?? string.Empty).ToLowerInvariant();
            var desc = (ni.Description ?? string.Empty).ToLowerInvariant();
            if (name.Contains("virtual") || name.Contains("vmware") || name.Contains("hyper-v") || name.Contains("vethernet") || desc.Contains("virtual") || desc.Contains("vmware") || desc.Contains("hyper-v") || desc.Contains("vethernet"))
            {
                return false;
            }

            return true;
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
#pragma warning restore SA1629
#pragma warning restore SA1513
#pragma warning restore SA1503
#pragma warning restore SA1117
#pragma warning restore SA1116
#pragma warning restore CS1591
