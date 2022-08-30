using System;
using System.Collections.Concurrent;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Buffer = Il2CppSystem.Buffer;

namespace Reactor.Networking;

public abstract class UnsafeCustomRpc
{
    internal CustomRpcManager? Manager { get; set; }
    protected internal virtual bool IsSingleton => true;

    public uint Id { get; }
    public BasePlugin UnsafePlugin { get; }
    public Mod Mod { get; }

    public abstract Type InnerNetObjectType { get; }

    public virtual SendOption SendOption => SendOption.Reliable;
    public abstract RpcLocalHandling LocalHandling { get; }

    protected UnsafeCustomRpc(BasePlugin plugin, uint id)
    {
        UnsafePlugin = plugin;
        Mod = ModList.GetByPluginType(plugin.GetType());
        Id = id;
    }

    public abstract void UnsafeWrite(MessageWriter writer, object? data);
    public abstract object? UnsafeRead(MessageReader reader);
    public abstract void UnsafeHandle(InnerNetObject innerNetObject, object? data);

    public void UnsafeSend(InnerNetObject innerNetObject, object? data, bool immediately = false, int targetClientId = -1, Action? ackCallback = null)
    {
        if (innerNetObject == null) throw new ArgumentNullException(nameof(innerNetObject));

        if (Manager == null)
        {
            throw new InvalidOperationException("Can't send unregistered CustomRpc");
        }

        if (LocalHandling == RpcLocalHandling.Before)
        {
            UnsafeHandle(innerNetObject, data);
        }

        var writer = immediately switch
        {
            false => AmongUsClient.Instance.StartRpc(innerNetObject.NetId, CustomRpcManager.CallId, SendOption),
            true => AmongUsClient.Instance.StartRpcImmediately(innerNetObject.NetId, CustomRpcManager.CallId, SendOption, targetClientId),
        };

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

        if (immediately)
        {
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        else
        {
            writer.EndMessage();
        }

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
