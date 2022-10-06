using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

/// <summary>
/// Allows launching steam builds outside of steam
/// </summary>
[HarmonyPatch]
internal static class StandaloneSteamPatch
{
    private static readonly Type? _type = Type.GetType("Steamworks.SteamAPI, Assembly-CSharp-firstpass", false);

    [HarmonyPrepare]
    public static bool Prepare()
    {
        return _type != null;
    }

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(_type, "RestartAppIfNecessary");
    }

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
