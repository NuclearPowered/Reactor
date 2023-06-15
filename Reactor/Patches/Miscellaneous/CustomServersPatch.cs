using System;
using System.Linq;
using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class CustomServersPatch
{
    private static bool IsCurrentServerOfficial()
    {
        const string Domain = "among.us";

        return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
               regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
               regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
    }

    [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
    [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__6), nameof(AuthManager._CoWaitForNonce_d__6.MoveNext))]
    [HarmonyPrefix]
    public static bool DisableAuthServer(ref bool __result)
    {
        if (IsCurrentServerOfficial())
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(AmongUsClient._CoJoinOnlinePublicGame_d__1), nameof(AmongUsClient._CoJoinOnlinePublicGame_d__1.MoveNext))]
    [HarmonyPrefix]
    public static void EnableUdpMatchmaking(AmongUsClient._CoJoinOnlinePublicGame_d__1 __instance)
    {
        // Skip to state 1 which just calls CoJoinOnlineGameDirect
        if (__instance.__1__state == 0 && !ServerManager.Instance.IsHttp)
        {
            __instance.__1__state = 1;
            __instance.__8__1 = new AmongUsClient.__c__DisplayClass1_0
            {
                matchmakerToken = string.Empty,
            };
        }
    }
}
