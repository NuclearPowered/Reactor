using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Reactor.Plugins;

namespace Reactor.Regions.Patches;

[HarmonyPatch]
internal static class WriteAllTextPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        var type = Type.GetType("FileIO, Assembly-CSharp", false) ?? Type.GetType("Innersloth.IO.FileIO, Assembly-CSharp", true);
        return AccessTools.Method(type, "WriteAllText");
    }

    [HarmonyPrefix]
    public static bool Prefix(string path, string contents)
    {
        // Among Us' region loading code unfortunately contains a call
        // to SaveServers, which will write out the region file. This
        // will lead to a positive feedback loop when detecting writes,
        // which is undesireable. So we check if the write makes a
        // change to the file on disk and if it would write the same
        // file again, stop AU from actually writing it.
        if (ServerManager.Instance && path == ServerManager.Instance.serverInfoFileJson)
        {
            var continueWrite = !File.Exists(path) || File.ReadAllText(path) != contents;
            Debug($"Continue serverInfoFile write? {continueWrite}");
            // If we will write, ignore the next change action from the observer.
            PluginSingleton<ReactorPlugin>.Instance.RegionInfoWatcher.IgnoreNext = continueWrite;
            return continueWrite;
        }

        return true;
    }
}
