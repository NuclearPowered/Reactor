using HarmonyLib;
using InnerNet;

namespace Reactor.Patches.QOL;

[HarmonyPatch]
internal static class BetterGuestPatch
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool ChatModeTypePatch(out QuickChatModes __result)
    {
        __result = QuickChatModes.FreeChatOrQuickChat;
        return false;
    }
}
