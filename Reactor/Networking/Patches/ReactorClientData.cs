using System.Collections.Generic;
using HarmonyLib;
using InnerNet;

namespace Reactor.Networking.Patches;

internal sealed class ReactorClientData
{
    public ClientData ClientData { get; }

    public Mod[] Mods { get; }

    public ReactorClientData(ClientData clientData, Mod[] mods)
    {
        ClientData = clientData;
        Mods = mods;
    }

    private static readonly Dictionary<int, ReactorClientData> _map = new();

    public static ReactorClientData Get(int clientId)
    {
        return _map[clientId];
    }

    internal static void Set(int clientId, ReactorClientData data)
    {
        _map[clientId] = data;
    }

    [HarmonyPatch]
    private static class ClearPatches
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
        [HarmonyPostfix]
        public static void DisconnectInternalPostfix()
        {
            _map.Clear();
        }

        [HarmonyPatch(typeof(EndGameResult), nameof(EndGameResult.Create))]
        [HarmonyPostfix]
        public static void EndGamePostfix()
        {
            _map.Clear();
        }
    }
}
