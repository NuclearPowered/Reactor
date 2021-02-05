using HarmonyLib;

namespace Reactor.Patches
{
    internal class KillButtonPatches
    {
        // Fixes kill cooldown resetting for a few frames if anyone on the map got the kill
        // Also fixes disappearing kill button if KillCooldown set to 0
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        [HarmonyPriority(Priority.First)]
        public static class PlayerControlSetKillTimerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
            {
                __instance.killTimer = time;
                
                // Patched because of this - game doesn't check if you're the killer
                if (__instance.AmOwner)
                {
                    HudManager.Instance.KillButton.SetCoolDown(
                        time,
                        PlayerControl.GameOptions.KillCooldown == 0 ? 0.01f : PlayerControl.GameOptions.KillCooldown
                        // SetCooldown does timer/maxTimer, so if MaxTimer is 0 kill button disappears
                    );
                }

                return false;
            }
        }
    }
}
