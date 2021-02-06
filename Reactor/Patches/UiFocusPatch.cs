using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Reactor.Extensions;

namespace Reactor.Patches
{
    internal static class UiFocusPatch
    {
        // [HarmonyPatch(typeof(PassiveButtonManager), nameof(PassiveButtonManager.HandleMouseOver))]
        [HarmonyPatch]
        public static class HandleFocusPatch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(PassiveButtonManager).GetMethods(typeof(void), typeof(PassiveUiElement), typeof(Collider2D));
            }

            public static bool Prefix()
            {
                return Application.isFocused;
            }
        }
    }
}
