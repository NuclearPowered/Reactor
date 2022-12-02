using HarmonyLib;

namespace Reactor.Patches.Fixes;

/// <summary>
/// Fixes Game Lists not working on servers using legacy matchmaking.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient._CoFindGame_d__3), nameof(AmongUsClient._CoFindGame_d__3.MoveNext))]
internal static class CoFindGamePatch
{
    public static void Prefix()
    {
        if (AmongUsClient.Instance.LastDisconnectReason == DisconnectReasons.Unknown)
        {
            AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.ExitGame;
        }
    }
}
