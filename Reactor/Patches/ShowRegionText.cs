using HarmonyLib;
using UnityEngine;
using TMPro;
using System;

namespace Reactor.Patches
{
    // 2021.4.12 does not properly set the region text, so we need to help it a bit.
    // We're piggybacking on a completely unrelated method which was chosen because it was close to the element we needed to change
    internal static class ShowRegionText
    {
        [HarmonyPatch(typeof(MatchMaker), nameof(MatchMaker.Start))]
        public static class MatchMakerPatch
        {
            public static void Prefix(MatchMaker __instance)
            {
                var parent = __instance.GetComponentInParent<Transform>().parent; // Returns NormalMenu
                var textmeshes = parent.GetComponentsInChildren<TextMeshPro>();
                foreach (var textmesh in textmeshes)
                {
                    if (textmesh.name == "RegionText_TMP")
                    {
                        var region = DestroyableSingleton<ServerManager>.Instance.CurrentRegion;
                        var name = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(region.TranslateName, region.Name, Array.Empty<Il2CppSystem.Object>());
                        textmesh.text = name;
                        break;
                    }
                }
            }
        }
    }
}
