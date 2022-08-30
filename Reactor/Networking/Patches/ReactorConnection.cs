using HarmonyLib;
using InnerNet;

namespace Reactor.Networking.Patches;

public class ReactorConnection
{
    public Syncer? Syncer { get; internal set; }

    public static ReactorConnection? Instance { get; private set; }

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CoConnect), typeof(string))]
        [HarmonyPrefix]
        public static void CoConnect()
        {
            Debug("New ReactorConnection created");
            Instance = new ReactorConnection();
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
        [HarmonyPostfix]
        public static void DisconnectInternalPostfix()
        {
            Debug("ReactorConnection disconnected");
            Instance = null;
        }
    }
}

public enum Syncer
{
    Server,
    Host,
}
