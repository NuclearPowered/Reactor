using HarmonyLib;
using Reactor.Networking;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class DisableServerAuthorityPatch
{
    private const int DisableServerAuthorityFlag = 25;

    public static bool Enabled => ModList.IsAnyModDisableServerAuthority || ReactorConfig.ForceDisableServerAuthority.Value;

    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    [HarmonyPostfix]
    public static void GetBroadcastVersionPatch(ref int __result)
    {
        if (!Enabled) return;
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;

        Debug("Sending the DisableServerAuthority flag");

        var revision = __result % 50;
        if (revision < DisableServerAuthorityFlag)
        {
            __result += DisableServerAuthorityFlag;
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
    [HarmonyPrefix]
    public static bool IsVersionModdedPatch(ref bool __result)
    {
        if (Enabled)
        {
            __result = true;
            return false;
        }

        return true;
    }
}
