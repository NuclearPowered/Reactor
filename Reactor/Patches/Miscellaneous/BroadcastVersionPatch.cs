using System;
using System.Linq;
using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class BroadcastVersionPatch
{
    private static int? _moddedBroadcastVersion;

    internal static void Initialize()
    {
        Constants.CompatVersions = Constants.CompatVersions.AddItem(Constants.GetBroadcastVersion()).ToArray();
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    [HarmonyReversePatch]
    public static int OriginalGetBroadcastVersion()
    {
        throw new NotImplementedException("Stub was not replaced");
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void GetModdedVersion(ref int __result)
    {
        __result = _moddedBroadcastVersion ??= OriginalGetBroadcastVersion() + Constants.MODDED_REVISION_MODIFIER_VALUE;
        Info($"Using modded broadcast version {_moddedBroadcastVersion}");
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
    [HarmonyPostfix]
    public static void IsModded(ref bool __result)
    {
        __result = true;
    }
}
