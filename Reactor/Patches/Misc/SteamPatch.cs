using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace Reactor.Patches.Misc;

[HarmonyPatch]
internal static class SteamPatch
{
    public const string TypeName = "Steamworks.SteamAPI, Assembly-CSharp-firstpass";
    public const string MethodName = "RestartAppIfNecessary";

    [HarmonyPrepare]
    public static bool Prepare()
    {
        return Type.GetType(TypeName, false) != null;
    }

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(TypeName + ":" + MethodName);
    }

    [HarmonyPrefix]
    public static bool RestartAppIfNecessaryPatch(out bool __result)
    {
        const string file = "steam_appid.txt";

        if (!File.Exists(file))
        {
            File.WriteAllText(file, "945360");
        }

        return __result = false;
    }
}
