using BepInEx.Unity.IL2CPP.Utils;
using Reactor.Debugger.AutoJoin;
using Reactor.Debugger.Utilities;
using UnityEngine;

namespace Reactor.Debugger.Window.Tabs;

internal sealed class AutoJoinTab : BaseTab
{
    public override string Name => "AutoJoin";

    public override void OnGUI()
    {
        if (AutoJoinConnectionManager.Server is { } server)
        {
            GUILayout.Label($"Hosting ({server.Clients.Count} clients)");

            if (GUILayout.Button("Stop"))
            {
                server.Dispose();
            }

            if (AmongUsClient.Instance)
            {
                if (GUILayout.Button("Create an online game"))
                {
                    AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoCreateOnlineGame());
                }

                if (GUILayout.Button("Create a local game"))
                {
                    AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoCreateLocalGame());
                }

                if (
                    AmongUsClient.Instance.AmConnected &&
                    AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay &&
                    GUILayout.Button("Join me")
                )
                {
                    server.SendJoinMe(AmongUsClient.Instance);
                }
            }
        }
        else if (AutoJoinConnectionManager.Client is { } client)
        {
            GUILayout.Label("Connected");

            if (GUILayout.Button("Disconnect"))
            {
                AutoJoinConnectionManager.Client = null;
                client.Dispose();
            }

            if (GUILayout.Button("Request join"))
            {
                client.SendRequestJoin();
            }
        }
        else
        {
            if (GUILayout.Button("Connect"))
            {
                AutoJoinConnectionManager.StartOrConnect();
            }
        }
    }
}
