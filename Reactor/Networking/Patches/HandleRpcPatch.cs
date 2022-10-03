using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using Reactor.Networking.Rpc;
using Reactor.Plugins;

namespace Reactor.Networking.Patches;

[HarmonyPatch]
internal static class HandleRpcPatch
{
    internal static List<Type> InnerNetObjectTypes { get; } = typeof(InnerNetObject).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(InnerNetObject)) && x != typeof(LobbyBehaviour)).ToList();

    public static IEnumerable<MethodBase> TargetMethods()
    {
        return InnerNetObjectTypes.Select(x => x.GetMethod(nameof(InnerNetObject.HandleRpc), AccessTools.allDeclared)).Where(m => m != null)!;
    }

    public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        return !HandleRpc(__instance, callId, reader);
    }
    
    public static bool HandleRpc(InnerNetObject innerNetObject, byte callId, MessageReader reader)
    {
        if (callId == CustomRpcManager.CallId)
        {
            var manager = PluginSingleton<ReactorPlugin>.Instance.CustomRpcManager;
            var customRpcs = manager._map[innerNetObject.GetType()];

            var mod = reader.ReadMod();
            var id = reader.ReadPackedUInt32();

            var customRpc = customRpcs[mod][id];

            customRpc.UnsafeHandle(innerNetObject, customRpc.UnsafeRead(reader.ReadMessage()));

            return true;
        }

        return false;
    }
}
