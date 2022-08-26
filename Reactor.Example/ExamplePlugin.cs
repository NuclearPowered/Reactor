using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Extensions;
using Reactor.Networking;
using Reactor.Networking.MethodRpc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reactor.Example;

[BepInAutoPlugin("gg.reactor.Example")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class ExamplePlugin : BasePlugin
{
    public override void Load()
    {
        this.AddComponent<ExampleComponent>();
    }

    [RegisterInIl2Cpp]
    public class ExampleComponent : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public DragWindow TestWindow { get; }

        public ExampleComponent(IntPtr ptr) : base(ptr)
        {
            TestWindow = new DragWindow(new Rect(60, 20, 0, 0), "Example", () =>
            {
                if (AmongUsClient.Instance && PlayerControl.LocalPlayer)
                {
                    if (GUILayout.Button("Send ExampleRpc"))
                    {
                        var name = PlayerControl.LocalPlayer.Data.PlayerName;
                        Rpc<ExampleRpc>.Instance.Send(new ExampleRpc.Data($"Send: from {name}"), ackCallback: () =>
                        {
                            Logger<ExamplePlugin>.Info("Got an acknowledgement for example rpc");
                        });

                        if (!AmongUsClient.Instance.AmHost)
                        {
                            Rpc<ExampleRpc>.Instance.SendTo(AmongUsClient.Instance.HostId, new ExampleRpc.Data($"SendTo: from {name} to host"));
                        }
                    }

                    if (GUILayout.Button("Send MethodExampleRpc"))
                    {
                        RpcSay(PlayerControl.LocalPlayer, "Hello from method rpc", Random.value, PlayerControl.LocalPlayer);
                    }
                }
            })
            {
                Enabled = false,
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestWindow.Enabled = !TestWindow.Enabled;
            }
        }

        private void OnGUI()
        {
            TestWindow.OnGUI();
        }
    }

    [MethodRpc((uint) CustomRpcCalls.MethodRpcExample)]
    public static void RpcSay(PlayerControl player, string text, float number, PlayerControl testPlayer)
    {
        Logger<ExamplePlugin>.Info($"{player.Data.PlayerName} text: {text} number: {number} testPlayer: {testPlayer.NetId}");
    }
}
