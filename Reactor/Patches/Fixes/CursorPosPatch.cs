using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Reactor.Patches.Fixes;

/// <summary>
/// "Fixes" an issue where empty TextBoxes have wrong cursor positions.
/// </summary>
[HarmonyPatch(typeof(TextMeshProExtensions), nameof(TextMeshProExtensions.CursorPos))]
internal static class CursorPosPatch
{
    public static bool Prefix(TextMeshPro self, ref Vector2 __result)
    {
        if (self.textInfo == null || self.textInfo.lineCount == 0 || self.textInfo.lineInfo[0].characterCount <= 0)
        {
            __result = self.GetTextInfo(" ").lineInfo.First().lineExtents.max;
            return false;
        }

        return true;
    }
}
