using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using InnerNet;
using Reactor.Utilities;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Manages custom rpc's.
/// </summary>
public class CustomRpcManager
{
    /// <summary>
    /// Rpc call id for the reactor's custom rpc wrapper.
    /// </summary>
    public const byte CallId = byte.MaxValue;

    private readonly List<UnsafeCustomRpc> _list = new();
    private readonly Dictionary<Type, Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>> _map = new();

    /// <summary>
    /// Gets a list of all custom rpc's.
    /// </summary>
    public IReadOnlyList<UnsafeCustomRpc> List => _list.AsReadOnly();

    /// <summary>
    /// Registers a custom rpc.
    /// </summary>
    /// <param name="customRpc">The custom rpc to register.</param>
    public void Register(UnsafeCustomRpc customRpc)
    {
        customRpc.Manager = this;
        _list.Add(customRpc);
        _map.GetOrCreate(customRpc.InnerNetObjectType, static _ => new Dictionary<Mod, Dictionary<uint, UnsafeCustomRpc>>())
            .GetOrCreate(customRpc.Mod, static _ => new Dictionary<uint, UnsafeCustomRpc>())
            .Add(customRpc.Id, customRpc);

        if (customRpc.IsSingleton)
        {
            typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);
        }
    }

    /// <summary>
    /// Implementation of <see cref="InnerNetObject.HandleRpc"/> for reading and handling reactor custom rpc's.
    /// You can use this in your custom <see cref="InnerNetObject"/> to use reactor custom rpc's on it.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> the rpc is handled on.</param>
    /// <param name="callId">The call id of the rpc.</param>
    /// <param name="reader">The <see cref="MessageReader"/> with the data of the rpc.</param>
    /// <returns>A value indicating whether the rpc was handled.</returns>
    /// <remarks>Reactor patches base game <see cref="InnerNetObject"/>s to use this.</remarks>
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

    [HarmonyPatch]
    internal static class HandleRpcPatch
    {
        private static List<Type> InnerNetObjectTypes { get; } = typeof(InnerNetObject).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(InnerNetObject)) && x != typeof(LobbyBehaviour)).ToList();

        public static IEnumerable<MethodBase> TargetMethods()
        {
            return InnerNetObjectTypes.Select(x => x.GetMethod(nameof(InnerNetObject.HandleRpc), AccessTools.allDeclared)).Where(m => m != null)!;
        }

        public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            return !HandleRpc(__instance, callId, reader);
        }
    }
}
