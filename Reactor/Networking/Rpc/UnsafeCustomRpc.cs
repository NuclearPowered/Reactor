using System;
using System.Collections.Concurrent;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Reactor.Utilities;
using Buffer = Il2CppSystem.Buffer;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Base type for custom rpc's.
/// </summary>
public abstract class UnsafeCustomRpc
{
    internal CustomRpcManager? Manager { get; set; }

    /// <summary>
    /// Gets a value indicating whether the rpc is a singleton and can be used with <see cref="Rpc{T}"/>.
    /// </summary>
    protected internal virtual bool IsSingleton => true;

    /// <summary>
    /// Gets the id of the rpc.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the plugin of the rpc.
    /// </summary>
    public BasePlugin UnsafePlugin { get; }

    /// <summary>
    /// Gets the mod of the rpc.
    /// </summary>
    public Mod Mod { get; }

    /// <summary>
    /// Gets the InnerNetObject type the rpc targets.
    /// </summary>
    public abstract Type InnerNetObjectType { get; }

    /// <summary>
    /// Gets the send option of the rpc.
    /// </summary>
    public virtual SendOption SendOption => SendOption.Reliable;

    /// <summary>
    /// Gets the local handling method of the rpc.
    /// </summary>
    public abstract RpcLocalHandling LocalHandling { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsafeCustomRpc"/> class.
    /// </summary>
    /// <param name="plugin">The plugin that the rpc is attached to.</param>
    /// <param name="id">The id of the rpc.</param>
    protected UnsafeCustomRpc(BasePlugin plugin, uint id)
    {
        UnsafePlugin = plugin;
        Mod = ModList.GetByPluginType(plugin.GetType());
        Id = id;
    }

    /// <summary>
    /// Writes <paramref name="data"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="data">The data to be written.</param>
    public abstract void UnsafeWrite(MessageWriter writer, object? data);

    /// <summary>
    /// Reads rpc data from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>The rpc data from the <paramref name="reader"/>.</returns>
    public abstract object? UnsafeRead(MessageReader reader);

    /// <summary>
    /// Handles rpc data.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> the rpc is being handled on.</param>
    /// <param name="data">The data associated with the rpc.</param>
    public abstract void UnsafeHandle(InnerNetObject innerNetObject, object? data);

    /// <summary>
    /// Sends this custom rpc on the specified <paramref name="innerNetObject"/> with the specified <paramref name="data"/>.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="immediately">Whether to send it immediately.</param>
    /// <param name="targetClientId">Target client id, defaults to broadcast.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void UnsafeSend(InnerNetObject innerNetObject, object? data, bool immediately = false, int targetClientId = -1, Action? ackCallback = null)
    {
        ArgumentNullException.ThrowIfNull(innerNetObject);

        if (Manager == null)
        {
            throw new InvalidOperationException("Can't send unregistered CustomRpc");
        }

        if (LocalHandling == RpcLocalHandling.Before)
        {
            UnsafeHandle(innerNetObject, data);
        }

        if (immediately == false)
        {
            Warning("Non-immediate RPCs were removed in 2025.5.20! Reactor will now always send immediately!");
        }

        var writer = AmongUsClient.Instance.StartRpcImmediately(
            innerNetObject.NetId,
            CustomRpcManager.CallId,
            SendOption,
            targetClientId);

        writer.Write(Mod);
        writer.WritePacked(Id);

        writer.StartMessage(0);
        UnsafeWrite(writer, data);
        writer.EndMessage();

        if (ackCallback != null)
        {
            if (SendOption != SendOption.Reliable) throw new ArgumentException("Can't add an ack callback to unreliable rpc", nameof(ackCallback));
            AckCallbacks.TryAdd(writer, ackCallback);
        }

        AmongUsClient.Instance.FinishRpcImmediately(writer);

        if (LocalHandling == RpcLocalHandling.After)
        {
            UnsafeHandle(innerNetObject, data);
        }
    }

    private static ConcurrentDictionary<MessageWriter, Action> AckCallbacks { get; } = new(Il2CppEqualityComparer<MessageWriter>.Instance);

    [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.Send))]
    private static class MessageAckPatch
    {
        public static bool Prefix(UdpConnection __instance, MessageWriter msg)
        {
            if (__instance._state != ConnectionState.Connected) return true;

            if (msg.SendOption == SendOption.Reliable && AckCallbacks.TryRemove(msg, out var ackCallback))
            {
                var buffer = new Il2CppStructArray<byte>(msg.Length);
                Buffer.BlockCopy(new Il2CppSystem.Array(msg.Buffer.Pointer), 0, new Il2CppSystem.Array(buffer.Pointer), 0, msg.Length);

                __instance.ResetKeepAliveTimer();

                __instance.AttachReliableID(buffer, 1, ackCallback);
                __instance.WriteBytesToConnection(buffer, buffer.Length);

                return false;
            }

            return true;
        }
    }
}
