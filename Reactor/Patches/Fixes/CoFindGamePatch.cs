using System.Reflection;
using HarmonyLib;
using Reactor.Utilities;

namespace Reactor.Patches.Fixes;

/// <summary>
/// Fixes Game Lists not working on servers using legacy matchmaking.
/// </summary>
[HarmonyPatch]
internal static class CoFindGamePatch
{
    public static MethodBase TargetMethod()
    {
        return StateMachineWrapper<AmongUsClient>.GetStateMachineMoveNext(nameof(AmongUsClient.CoFindGame))!;
    }

    public static void Prefix()
    {
        if (AmongUsClient.Instance.LastDisconnectReason == DisconnectReasons.Unknown)
        {
            AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.ExitGame;
        }
    }
}
