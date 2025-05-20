using System;
using BepInEx.Unity.IL2CPP;
using Hazel;
using InnerNet;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Base type for custom rpc's but typed with generics.
/// </summary>
/// <typeparam name="TPlugin">The type of the plugin the the rpc is attached to.</typeparam>
/// <typeparam name="TInnerNetObject">The type of the <see cref="InnerNetObject"/>.</typeparam>
/// <typeparam name="TData">The type of the rpc data.</typeparam>
public abstract class CustomRpc<TPlugin, TInnerNetObject, TData> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject
{
    /// <inheritdoc />
    protected CustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    /// <inheritdoc cref="UnsafeCustomRpc.UnsafePlugin"/>
    public TPlugin Plugin => (TPlugin) UnsafePlugin;

    /// <inheritdoc />
    public override Type InnerNetObjectType => typeof(TInnerNetObject);

    /// <inheritdoc cref="UnsafeCustomRpc.UnsafeWrite" />
    public abstract void Write(MessageWriter writer, TData? data);

    /// <inheritdoc cref="UnsafeCustomRpc.UnsafeRead" />
    public abstract TData Read(MessageReader reader);

    /// <inheritdoc cref="UnsafeCustomRpc.UnsafeHandle" />
    public abstract void Handle(TInnerNetObject innerNetObject, TData? data);

    /// <inheritdoc />
    public override void UnsafeWrite(MessageWriter writer, object? data)
    {
        Write(writer, (TData?) data);
    }

    /// <inheritdoc />
    public override object? UnsafeRead(MessageReader reader)
    {
        return Read(reader);
    }

    /// <inheritdoc />
    public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
    {
        Handle((TInnerNetObject) innerNetObject, (TData?) data);
    }

    /// <summary>
    /// Sends this custom rpc on the specified <paramref name="innerNetObject"/> with the specified <paramref name="data"/>.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void Send(InnerNetObject innerNetObject, TData data, Action? ackCallback = null)
    {
        UnsafeSend(innerNetObject, data, ackCallback: ackCallback);
    }

    /// <summary>
    /// Sends this custom rpc on the specified <paramref name="innerNetObject"/> with the specified <paramref name="data"/> to the specified target.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="targetClientId">Target client id, defaults to broadcast.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void SendTo(InnerNetObject innerNetObject, int targetClientId, TData data, Action? ackCallback = null)
    {
        UnsafeSend(innerNetObject, data, targetClientId, ackCallback);
    }
}

/// <summary>
/// Base type for custom rpc's but typed with generics and without any data.
/// </summary>
/// <typeparam name="TPlugin">The type of the plugin the the rpc is attached to.</typeparam>
/// <typeparam name="TInnerNetObject">The type of the <see cref="InnerNetObject"/>.</typeparam>
public abstract class CustomRpc<TPlugin, TInnerNetObject> : UnsafeCustomRpc where TPlugin : BasePlugin where TInnerNetObject : InnerNetObject
{
    /// <inheritdoc />
    protected CustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    /// <inheritdoc cref="UnsafeCustomRpc.UnsafePlugin"/>
    public TPlugin Plugin => (TPlugin) UnsafePlugin;

    /// <inheritdoc />
    public override Type InnerNetObjectType => typeof(TInnerNetObject);

    /// <summary>
    /// Handles the rpc.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> the rpc is being handled on.</param>
    public abstract void Handle(TInnerNetObject innerNetObject);

    /// <inheritdoc />
    public override void UnsafeWrite(MessageWriter writer, object? data)
    {
    }

    /// <inheritdoc />
    public override object? UnsafeRead(MessageReader reader)
    {
        return null;
    }

    /// <inheritdoc />
    public override void UnsafeHandle(InnerNetObject innerNetObject, object? data)
    {
        Handle((TInnerNetObject) innerNetObject);
    }

    /// <summary>
    /// Sends this custom rpc on the specified <paramref name="innerNetObject"/>.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void Send(InnerNetObject innerNetObject, Action? ackCallback = null)
    {
        UnsafeSend(innerNetObject, ackCallback);
    }

    /// <summary>
    /// Sends this custom rpc on the specified <paramref name="innerNetObject"/> to the specified target.
    /// </summary>
    /// <param name="innerNetObject">The <see cref="InnerNetObject"/> to send the rpc on.</param>
    /// <param name="targetClientId">Target client id, defaults to broadcast.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void SendTo(InnerNetObject innerNetObject, int targetClientId, Action? ackCallback = null)
    {
        UnsafeSend(innerNetObject, null, targetClientId, ackCallback);
    }
}
