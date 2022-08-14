using System;
using BepInEx.IL2CPP;
using Hazel;
using InnerNet;

namespace Reactor.Networking
{
    public abstract class CustomRpc<TPlugin, TInnerNetObject, TData> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject
    {
        protected CustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public TPlugin Plugin => (TPlugin) UnsafePlugin;
        public override Type InnerNetObjectType => typeof(TInnerNetObject);

        public abstract void Write(MessageWriter writer, TData? data);
        public abstract TData Read(MessageReader reader);
        public abstract void Handle(TInnerNetObject innerNetObject, TData? data);

        public override void UnsafeWrite(MessageWriter writer, object? data)
        {
            Write(writer, (TData?) data);
        }

        public override object? UnsafeRead(MessageReader reader)
        {
            return Read(reader);
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
        {
            Handle((TInnerNetObject) innerNetObject, (TData?) data);
        }

        public void Send(InnerNetObject netObject, TData data, bool immediately = false, Action? ackCallback = null)
        {
            UnsafeSend(netObject, data, immediately, ackCallback: ackCallback);
        }

        public void SendTo(InnerNetObject netObject, int targetId, TData data, Action? ackCallback = null)
        {
            UnsafeSend(netObject, data, true, targetId, ackCallback);
        }
    }

    public abstract class CustomRpc<TPlugin, TInnerNetObject> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject
    {
        protected CustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public TPlugin Plugin => (TPlugin) UnsafePlugin;
        public override Type InnerNetObjectType => typeof(TInnerNetObject);

        public abstract void Handle(TInnerNetObject innerNetObject);

        public override void UnsafeWrite(MessageWriter writer, object? data)
        {
        }

        public override object? UnsafeRead(MessageReader reader)
        {
            return null;
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
        {
            Handle((TInnerNetObject) innerNetObject);
        }

        public void Send(InnerNetObject netObject, bool immediately = false, Action? ackCallback = null)
        {
            UnsafeSend(netObject, null, immediately, ackCallback: ackCallback);
        }

        public void SendTo(InnerNetObject netObject, int targetId, Action? ackCallback = null)
        {
            UnsafeSend(netObject, null, true, targetId, ackCallback);
        }
    }
}
