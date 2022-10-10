using System;
using BepInEx.Unity.IL2CPP;

namespace Reactor.Networking.Rpc;

/// <summary>
/// Shorthand for <see cref="CustomRpc{TPlugin,TInnerNetObject,TData}"/> with <see cref="PlayerControl"/> as the TInnerNetObject.
/// </summary>
/// <typeparam name="TPlugin">The type of the plugin the the rpc is attached to.</typeparam>
/// <typeparam name="TData">The type of the rpc data.</typeparam>
public abstract class PlayerCustomRpc<TPlugin, TData> : CustomRpc<TPlugin, PlayerControl, TData> where TPlugin : BasePlugin
{
    /// <inheritdoc />
    protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    /// <summary>
    /// Sends this custom rpc on the local player with the specified <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="immediately">Whether to send it immediately.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void Send(TData data, bool immediately = false, Action? ackCallback = null)
    {
        Send(PlayerControl.LocalPlayer, data, immediately, ackCallback);
    }

    /// <summary>
    /// Sends this custom rpc on the local player with the specified <paramref name="data"/> to the specified target.
    /// </summary>
    /// <param name="targetClientId">Target client id, defaults to broadcast.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void SendTo(int targetClientId, TData data, Action? ackCallback = null)
    {
        SendTo(PlayerControl.LocalPlayer, targetClientId, data, ackCallback);
    }
}

/// <summary>
/// Shorthand for <see cref="CustomRpc{TPlugin,TInnerNetObject}"/> with <see cref="PlayerControl"/> as the TInnerNetObject.
/// </summary>
/// <typeparam name="TPlugin">The type of the plugin the the rpc is attached to.</typeparam>
public abstract class PlayerCustomRpc<TPlugin> : CustomRpc<TPlugin, PlayerControl> where TPlugin : BasePlugin
{
    /// <inheritdoc />
    protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    /// <summary>
    /// Sends this custom rpc on the local player.
    /// </summary>
    /// <param name="immediately">Whether to send it immediately.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void Send(bool immediately = false, Action? ackCallback = null)
    {
        Send(PlayerControl.LocalPlayer, immediately, ackCallback);
    }

    /// <summary>
    /// Sends this custom rpc on the local player to the specified target.
    /// </summary>
    /// <param name="targetClientId">Target client id, defaults to broadcast.</param>
    /// <param name="ackCallback">The callback to invoke when this packet is acknowledged.</param>
    public void SendTo(int targetClientId, Action? ackCallback = null)
    {
        SendTo(PlayerControl.LocalPlayer, targetClientId, ackCallback);
    }
}
