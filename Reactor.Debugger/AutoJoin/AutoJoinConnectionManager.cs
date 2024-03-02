using HarmonyLib;
using InnerNet;

namespace Reactor.Debugger.AutoJoin;

internal static class AutoJoinConnectionManager
{
    public static AutoJoinConnectionListener? Server { get; set; }
    public static AutoJoinClientConnection? Client { get; set; }

    public static void StartOrConnect()
    {
        if (AutoJoinConnectionListener.TryStart(out var connectionListener))
        {
            Info("Started an AutoJoin session");
            Server = connectionListener;
            Server.Stopped += () =>
            {
                if (Server == connectionListener)
                {
                    Server = null;
                }
            };
        }
        else if (AutoJoinClientConnection.TryConnect(out var client))
        {
            Info("Connected to an AutoJoin session");
            Client = client;
            client.Disconnected += () =>
            {
                if (Client == client)
                {
                    Client = null;

                    Info("Disconnected from an AutoJoin session, trying to reconnect");
                    StartOrConnect();
                }
            };
        }
        else
        {
            Error("Failed to start or connect to an AutoJoin session");
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Start))]
    public static class ConnectPatch
    {
        public static void Postfix(InnerNetClient __instance)
        {
            if (!DebuggerConfig.JoinGameOnStart.Value) return;

            if (__instance.TryCast<AmongUsClient>() is { } amongUsClient)
            {
                if (Server != null)
                {
                    amongUsClient.StartCoroutine(amongUsClient.CoCreateOnlineGame());
                }
                else if (Client is { } client)
                {
                    client.SendRequestJoin();
                }
            }
        }
    }
}
