using System;
using BepInEx.IL2CPP;

namespace Reactor.Networking
{
    public abstract class PlayerCustomRpc<TPlugin, TData> : CustomRpc<TPlugin, PlayerControl, TData> where TPlugin : BasePlugin
    {
        protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public void Send(TData data, bool immediately = false, Action? ackCallback = null)
        {
            Send(PlayerControl.LocalPlayer, data, immediately, ackCallback);
        }

        public void SendTo(int targetId, TData data, Action? ackCallback = null)
        {
            SendTo(PlayerControl.LocalPlayer, targetId, data, ackCallback);
        }
    }

    public abstract class PlayerCustomRpc<TPlugin> : CustomRpc<TPlugin, PlayerControl> where TPlugin : BasePlugin
    {
        protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public void Send(bool immediately = false, Action? ackCallback = null)
        {
            Send(PlayerControl.LocalPlayer, immediately, ackCallback);
        }

        public void SendTo(int targetId, Action? ackCallback = null)
        {
            SendTo(PlayerControl.LocalPlayer, targetId, ackCallback);
        }
    }
}
