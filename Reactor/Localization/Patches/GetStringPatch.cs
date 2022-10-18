using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Localization.Patches;

[HarmonyPatch]
internal static class GetStringPatch
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetStringWithDefault), typeof(StringNames), typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
    [HarmonyPrefix]
    public static bool StringNamesPatch(StringNames id, Il2CppReferenceArray<Il2CppSystem.Object> parts, out string __result)
    {
        if (LocalizationManager.TryGetTextFormatted(id, parts, out __result))
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(SystemTypes))]
    [HarmonyPrefix]
    public static bool SystemTypesStringPatch(TranslationController __instance, SystemTypes room, ref string __result)
    {
        if (LocalizationManager.TryGetStringName(room, out var stringName))
        {
            if (LocalizationManager.TryGetText(stringName, out var text))
            {
                __result = text;
                return false;
            }

            __result = __instance.GetString(stringName);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetSystemName))]
    [HarmonyPrefix]
    public static bool SystemTypesStringNamesPatch(SystemTypes room, ref StringNames __result)
    {
        if (LocalizationManager.TryGetStringName(room, out var stringName))
        {
            __result = stringName;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(TaskTypes))]
    [HarmonyPrefix]
    public static bool TaskTypesStringPatch(TranslationController __instance, TaskTypes task, ref string __result)
    {
        if (LocalizationManager.TryGetStringName(task, out var stringName))
        {
            if (LocalizationManager.TryGetText(stringName, out var text))
            {
                __result = text;
                return false;
            }

            __result = __instance.GetString(stringName);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetTaskName))]
    [HarmonyPrefix]
    public static bool TaskTypesStringNamesPatch(TaskTypes task, ref StringNames __result)
    {
        if (LocalizationManager.TryGetStringName(task, out var stringName))
        {
            __result = stringName;
            return false;
        }

        return true;
    }
}
