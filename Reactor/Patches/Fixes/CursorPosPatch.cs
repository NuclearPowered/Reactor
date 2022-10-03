using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Reactor.Patches.Fixes;

/// <summary>
/// "Fixes" an issue where empty TextBoxes have wrong cursor positions
/// </summary>
[HarmonyPatch]
internal static class CursorPosPatch
{
    [HarmonyPatch(typeof(TextMeshProExtensions), nameof(TextMeshProExtensions.CursorPos))]
    [HarmonyPrefix]
    public static bool FixCursorPosPatch(TextMeshPro self, ref Vector2 __result)
    {
        if (self.textInfo == null || self.textInfo.lineCount == 0 || self.textInfo.lineInfo[0].characterCount <= 0)
        {
            __result = self.GetTextInfo(" ").lineInfo.First().lineExtents.max;
            return false;
        }

        return true;
    }
}
