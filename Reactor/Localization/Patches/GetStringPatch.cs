using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Localization.Patches;

[HarmonyPatch]
internal static class GetStringPatch
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    [HarmonyPrefix]
    public static bool Prefix(StringNames id, Il2CppReferenceArray<Il2CppSystem.Object> parts, ref string __result)
    {
        if (LocalizationManager.TryGetText(id, out var text))
        {
            __result = string.Format(text, parts);
            return false;
        }

        return true;
    }
}
