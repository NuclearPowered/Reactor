using System;
using System.Linq;
using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class CustomServersPatch
{
    private static StringNames? _lastFindGameRegion;
    private static string? _lastFindGameIp;

    private static bool IsCurrentServerOfficial()
    {
        const string Domain = "among.us";

        return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
               regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
               regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
    }

    private static bool IsCurrentServerOfficialByStringNames(StringNames name)
    {
        return name is StringNames.ServerNA or StringNames.ServerEU or StringNames.ServerAS;
    }

    private static bool IsCurrentServerOfficialByFindGameInfo()
    {
        return _lastFindGameRegion is { } region && IsCurrentServerOfficialByStringNames(region)
            && AmongUsClient.Instance.networkAddress == _lastFindGameIp;
    }

    [HarmonyPatch(typeof(EnterCodeManager), nameof(EnterCodeManager.FindGameResult))]
    [HarmonyPostfix]
    public static void CacheRegionAndIpOnFindGame(
    EnterCodeManager __instance,
    [HarmonyArgument(0)] HttpMatchmakerManager.FindGameByCodeResponse response)
    {
        if (response == null)
        {
            return;
        }

        _lastFindGameRegion = response.Region;
        _lastFindGameIp = response.Game?.IPString;
    }

    [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
    [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__6), nameof(AuthManager._CoWaitForNonce_d__6.MoveNext))]
    [HarmonyPrefix]
    public static bool DisableAuthServer(ref bool __result)
    {
        if (IsCurrentServerOfficial() || IsCurrentServerOfficialByFindGameInfo())
        {
            // Info("Exchanging nonce since target server is official.");
            return true;
        }

        __result = false;
        // Info("Skipped nonce since target server is custom");
        return false;
    }

    [HarmonyPatch(typeof(AmongUsClient._CoJoinOnlinePublicGame_d__49), nameof(AmongUsClient._CoJoinOnlinePublicGame_d__49.MoveNext))]
    [HarmonyPrefix]
    public static void EnableUdpMatchmaking(AmongUsClient._CoJoinOnlinePublicGame_d__49 __instance)
    {
        // Skip to state 1 which just calls CoJoinOnlineGameDirect
        if (__instance.__1__state == 0 && !ServerManager.Instance.IsHttp)
        {
            __instance.__1__state = 1;
            __instance.__8__1 = new AmongUsClient.__c__DisplayClass49_0
            {
                matchmakerToken = string.Empty,
            };
        }
    }
}
