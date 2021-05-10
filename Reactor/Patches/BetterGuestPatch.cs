using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace Reactor.Patches
{
    internal static class BetterGuestPatch
    {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
        public static class ChatModeTypePatch
        {
            public static bool Prefix(out QuickChatModes __result)
            {
                __result = QuickChatModes.FreeChatOrQuickChat;
                return false;
            }
        }

        [HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Open))]
        public static class EditNamePatch
        {
            public static void Prefix(AccountTab __instance)
            {
                Fix(__instance, __instance.guestMode);
                Fix(__instance, __instance.offlineMode);
            }

            private static void Fix(AccountTab __instance, GameObject mode)
            {
                var randomizeName = mode.transform.Find("RandomizeName").gameObject;
                if (!randomizeName.active)
                    return;

                randomizeName.active = false;
                Object.Instantiate(__instance.loggedInMode.editNameButton, mode.transform);
            }
        }
    }
}
