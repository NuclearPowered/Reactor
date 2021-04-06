using HarmonyLib;
using InnerNet;

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
    }
}
