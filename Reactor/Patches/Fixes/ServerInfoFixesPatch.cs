using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;
using InnerNet;

namespace Reactor.Patches.Fixes;

[HarmonyPatch]
internal static class ServerInfoFixesPatch
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Connect))]
    [HarmonyPrefix]
    public static void LogConnect(InnerNetClient __instance)
    {
        Info($"Joining {__instance.networkAddress}:{__instance.networkPort}");
    }

    // Fixes hardcoded port and filters out IPv6 in DnsRegionInfo.
    [HarmonyPatch(typeof(DnsRegionInfo), nameof(DnsRegionInfo.PopulateServers))]
    [HarmonyPrefix]
    public static bool FixPopulateServers(DnsRegionInfo __instance)
    {
        try
        {
            var i = 0;
            var servers = Dns.GetHostAddresses(__instance.Fqdn)
                .Distinct()
                .Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork)
                .Select(ipAddress => new ServerInfo($"{__instance.Name}-{i++}", ipAddress.ToString(), __instance.Port, __instance.UseDtls))
                .ToArray();

            __instance.cachedServers = servers;
            Info($"Populated {__instance.Name} ({__instance.Fqdn}:{__instance.Port}) with {servers.Length} server(s) {{{servers.Select(x => x.ToString()).Join()}}}");
        }
        catch (Exception e)
        {
            Info($"Failed to populate {__instance.Name}: {e}");
            __instance.cachedServers = new[]
            {
                new ServerInfo(__instance.Name ?? string.Empty, __instance.DefaultIp, __instance.Port, __instance.UseDtls),
            };
        }

        return false;
    }
}
