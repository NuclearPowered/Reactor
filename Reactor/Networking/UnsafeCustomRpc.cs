using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using InnerNet;

namespace Reactor.Networking
{
    public abstract class UnsafeCustomRpc
    {
        internal static Dictionary<IntPtr, Action> AckCallbacks = new Dictionary<IntPtr, Action>();

        internal CustomRpcManager? Manager { get; set; }
        protected internal virtual bool IsSingleton => true;

        public uint Id { get; }
        public BasePlugin UnsafePlugin { get; }
        public string PluginId { get; }

        public abstract Type InnerNetObjectType { get; }

        public virtual SendOption SendOption => SendOption.Reliable;
        public abstract RpcLocalHandling LocalHandling { get; }

        protected UnsafeCustomRpc(BasePlugin plugin, uint id)
        {
            UnsafePlugin = plugin;
            PluginId = MetadataHelper.GetMetadata(plugin).GUID;
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
                true => AmongUsClient.Instance.StartRpcImmediately(innerNetObject.NetId, CustomRpcManager.CallId, SendOption, targetClientId)
            };

            var pluginNetId = ModList.GetById(PluginId).NetId;
            writer.WritePacked(pluginNetId);
            writer.WritePacked(Id);

            writer.StartMessage(0);
            UnsafeWrite(writer, data);
            writer.EndMessage();

            if (ackCallback != null)
            {
                AckCallbacks.Add(writer.Pointer, ackCallback);
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

        [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.Send))]
        private static class MessageAckPatch
        {
            public static bool Prefix(UdpConnection __instance, MessageWriter msg)
            {
                if (msg.SendOption == SendOption.Reliable && AckCallbacks.Remove(msg.Pointer, out var ackCallback))
                {
                    __instance.ReliableSend((byte) msg.SendOption, msg.ToByteArray(false), ackCallback);
                    return false;
                }

                return true;
            }
        }
    }
}
