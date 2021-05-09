using HarmonyLib;
using TMPro;
using UnhollowerBaseLib;
using Object = Il2CppSystem.Object;

namespace Reactor.Patches
{
    // 2021.4.12 does not properly set the region text, so we need to help it a bit.
    internal static class FixRegionText
    {
        [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.Awake))]
        public static class MatchMakerPatch
        {
            public static void Prefix(TextMeshPro __instance)
            {
                if (__instance.name == "RegionText_TMP")
                {
                    var region = ServerManager.Instance.CurrentRegion;
                    var name = TranslationController.Instance.GetStringWithDefault(region.TranslateName, region.Name, new Il2CppReferenceArray<Object>(0));
                    __instance.text = name;
                }
            }
        }
    }
}
