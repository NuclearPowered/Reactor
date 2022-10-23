using HarmonyLib;

namespace Reactor.Localization.Patches;

[HarmonyPatch]
internal static class LanguageChangedPatch
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize))]
    [HarmonyPostfix]
    public static void Initialize(TranslationController __instance)
    {
        if (TranslationController.Instance.GetInstanceID() == __instance.GetInstanceID())
            LocalizationManager.OnLanguageChanged(__instance.currentLanguage.languageID);
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.SetLanguage))]
    [HarmonyPostfix]
    public static void SetLanguage(TranslationController __instance)
    {
        LocalizationManager.OnLanguageChanged(__instance.currentLanguage.languageID);
    }
}
