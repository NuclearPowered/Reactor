using BepInEx.Unity.IL2CPP;

namespace Reactor.Networking;

public abstract class PlayerCustomRpc<TPlugin, TData> : CustomRpc<TPlugin, PlayerControl, TData> where TPlugin : BasePlugin
{
    protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    public void Send(TData data, bool immediately = false)
    {
        Send(PlayerControl.LocalPlayer, data, immediately);
    }

    public void SendTo(int targetId, TData data)
    {
        SendTo(PlayerControl.LocalPlayer, targetId, data);
    }
}

public abstract class PlayerCustomRpc<TPlugin> : CustomRpc<TPlugin, PlayerControl> where TPlugin : BasePlugin
{
    protected PlayerCustomRpc(TPlugin plugin, uint id) : base(plugin, id)
    {
    }

    public void Send(bool immediately = false)
    {
        Send(PlayerControl.LocalPlayer, immediately);
    }

    public void SendTo(int targetId)
    {
        SendTo(PlayerControl.LocalPlayer, targetId);
    }
}
