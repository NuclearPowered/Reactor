using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;
using InnerNet;

namespace Reactor.Patches
{
    /// <summary>
    /// Fixes hardcoded ports
    /// </summary>
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
    public static class SetEndpointPatch
    {
        public static void Prefix(InnerNetClient __instance, ref string addr, ref ushort port)
        {
            var serverManager = ServerManager.Instance;
            if (addr == serverManager.OnlineNetAddress && __instance.GameMode == GameModes.OnlineGame && port != serverManager.OnlineNetPort)
            {
                Logger<ReactorPlugin>.Info($"Set endpoint to {addr}:{port}");
                Logger<ReactorPlugin>.Info($"Correcting port to {serverManager.OnlineNetPort}");
                port = serverManager.OnlineNetPort;
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Connect))]
    public static class ConnectPatch
    {
        public static void Prefix(InnerNetClient __instance)
        {
            Logger<ReactorPlugin>.Info($"Joining {__instance.networkAddress}:{__instance.networkPort}");
        }
    }

    /// <summary>
    /// Fixes hardcoded port and filters out IPv6 in DnsRegionInfo
    /// </summary>
    [HarmonyPatch(typeof(DnsRegionInfo), nameof(DnsRegionInfo.PopulateServers))]
    public static class PopulateServersPatch
    {
        public static bool Prefix(DnsRegionInfo __instance)
        {
            try
            {
                var i = 0;
                var servers = Dns.GetHostAddresses(__instance.Fqdn)
                    .Distinct()
                    .Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ipAddress => new ServerInfo($"{__instance.Name}-{i++}", ipAddress.ToString(), __instance.Port))
                    .ToArray();

                __instance.cachedServers = servers;
                Logger<ReactorPlugin>.Info($"Populated {__instance.Name} ({__instance.Fqdn}:{__instance.Port}) with {servers.Length} server(s) {{{servers.Join()}}}");
            }
            catch (Exception e)
            {
                Logger<ReactorPlugin>.Info($"Failed to populate {__instance.Name}: {e}");
                __instance.cachedServers = new[]
                {
                    new ServerInfo(__instance.Name ?? string.Empty, __instance.DefaultIp, __instance.Port),
                };
            }

            return false;
        }
    }

    /// <summary>
    /// De-inlines AmongUsClient#SetEndpoint from JoinGameButton 
    /// </summary>
    [HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
    public static class JoinGameButtonPatch
    {
        public static void Postfix(JoinGameButton __instance)
        {
            if (__instance.GameMode == GameModes.OnlineGame)
            {
                AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, DestroyableSingleton<ServerManager>.Instance.OnlineNetPort);
            }
        }
    }
}
