using HarmonyLib;

namespace Reactor.Patches
{
    /// <summary>
    /// Fixes kill cooldown resetting for a few frames if anyone on the map got the kill
    /// Also fixes invisible kill button if KillCooldown is set to 0
    /// </summary>
    internal class KillButtonPatches
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        [HarmonyPriority(Priority.First)]
        public static class SetKillTimerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
            {
                __instance.killTimer = time;

                // Patched because of this - game doesn't check if you're the killer
                if (__instance.AmOwner)
                {
                    HudManager.Instance.KillButton.SetCoolDown(
                        time,
                        // if MaxTimer is 0, kill button disappears
                        PlayerControl.GameOptions.KillCooldown <= 0 ? 1 : PlayerControl.GameOptions.KillCooldown
                    );
                }

                return false;
            }
        }
    }
}
