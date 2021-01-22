#if STEAM
using System.IO;
using HarmonyLib;
using Steamworks;

namespace Reactor.Patches
{
    internal static class SteamPatch
    {
        [HarmonyPatch(typeof(SteamAPI), nameof(SteamAPI.RestartAppIfNecessary))]
        public static class RestartAppIfNecessaryPatch
        {
            public static bool Prefix(out bool __result)
            {
                const string file = "steam_appid.txt";

                if (!File.Exists(file))
                {
                    File.WriteAllText(file, "945360");
                }

                return __result = false;
            }
        }
    }
}
#endif
