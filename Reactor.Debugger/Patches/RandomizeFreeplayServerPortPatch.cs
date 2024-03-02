using System;
using System.Net;
using HarmonyLib;
using InnerNet;

namespace Reactor.Debugger.Patches;

[HarmonyPatch]
internal static class RandomizeFreeplayServerPortPatch
{
    private static ushort? _lastPort;

    [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.StartAsLocalServer))]
    public static class StartServerPatch
    {
        public static void Prefix(InnerNetServer __instance)
        {
            var port = (ushort) Random.Shared.Next(1024, IPEndPoint.MaxPort);
            _lastPort = port;
            __instance.Port = port;
        }

        public static void Postfix(InnerNetServer __instance)
        {
            __instance.Port = Constants.GamePlayPort;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
    [HarmonyPrefix]
    public static void SetEndpointPatch(InnerNetClient __instance, string addr, ref ushort port)
    {
        if (_lastPort != null && __instance.NetworkMode == NetworkModes.FreePlay)
        {
            port = _lastPort.Value;
            _lastPort = null;
        }
    }
}
