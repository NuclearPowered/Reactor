#if STEAM
using HarmonyLib;

namespace Reactor.Patches
{
    // Addresses the issue where players are sometimes unable to move after meetings unless they open and close a menu
    // Affects 2021.3.31.3s, check whether this is required on Itch or future updates
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    internal static class CanMovePatch
    {
        public static bool Prefix(PlayerControl __instance, ref bool __result)
        {
            __result = __instance.moveable && !Minigame.Instance && (!DestroyableSingleton<HudManager>.InstanceExists || !DestroyableSingleton<HudManager>.Instance.Chat.IsOpen && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen) /*&& (!ControllerManager.Instance || !ControllerManager.Instance.IsUiControllerActive)*/ && (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) && !MeetingHud.Instance && !CustomPlayerMenu.Instance && !ExileController.Instance && !IntroCutscene.Instance;

            return false;
        }
    }
}
#endif
