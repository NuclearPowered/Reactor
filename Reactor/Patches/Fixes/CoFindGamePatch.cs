using HarmonyLib;

namespace Reactor.Patches.Fixes;

/// <summary>
/// Fixes Game Lists not working on servers using legacy matchmaking.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient_CoFindGame), nameof(AmongUsClient_CoFindGame.MoveNext))]
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
