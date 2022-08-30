using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using InnerNet;

namespace Reactor.Networking;

public enum RpcLocalHandling
{
    None,
    Before,
    After,
}

public class CustomRpcManager
{
    public const byte CallId = byte.MaxValue;

    private readonly List<UnsafeCustomRpc> _list = new();
    private readonly Dictionary<Type, Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>> _map = new();

    public IReadOnlyList<UnsafeCustomRpc> List => _list.AsReadOnly();

    public UnsafeCustomRpc Register(UnsafeCustomRpc customRpc)
    {
        customRpc.Manager = this;
        _list.Add(customRpc);
        _map.GetOrCreate(customRpc.InnerNetObjectType, _ => new Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>())
            .GetOrCreate(customRpc.Mod, _ => new Dictionary<uint, UnsafeCustomRpc>())
            .Add(customRpc.Id, customRpc);

        if (customRpc.IsSingleton)
        {
            typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);
        }

        return customRpc;
    }

    [HarmonyPatch]
    private static class HandleRpcPatch
    {
        internal static List<Type> InnerNetObjectTypes { get; } = typeof(InnerNetObject).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(InnerNetObject)) && x != typeof(LobbyBehaviour)).ToList();

        public static IEnumerable<MethodBase> TargetMethods()
        {
            return InnerNetObjectTypes.Select(x => x.GetMethod(nameof(InnerNetObject.HandleRpc), AccessTools.all)!);
        }

        public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            return !HandleRpc(__instance, callId, reader);
        }
    }

    public static bool HandleRpc(InnerNetObject innerNetObject, byte callId, MessageReader reader)
    {
        if (callId == CallId)
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

public static class Rpc<T> where T : UnsafeCustomRpc
{
    private static T? _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception($"{typeof(T).FullName} isn't registered");
            }

            return _instance;
        }

        internal set
        {
            if (_instance != null)
            {
                throw new Exception($"{typeof(T).FullName} is already registered");
            }

            _instance = value;
        }
    }
}
