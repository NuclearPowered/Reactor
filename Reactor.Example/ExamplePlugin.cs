using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Localization;
using Reactor.Localization.Utilities;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.ImGui;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reactor.Example;

[BepInAutoPlugin("gg.reactor.Example")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin
{
    private static StringNames _helloStringName;

    public override void Load()
    {
        ReactorPingTracker.Register<ExamplePlugin>(ReactorPingTracker.AlwaysShow);

        this.AddComponent<ExampleComponent>();

        _helloStringName = CustomStringName.CreateAndRegister("Hello!");
        LocalizationManager.Register(new ExampleLocalizationProvider());
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
                if (GUILayout.Button("Log CustomStringName"))
                {
                    Logger<ExamplePlugin>.Info(TranslationController.Instance.GetString(_helloStringName));
                }

                if (GUILayout.Button("Log localized string"))
                {
                    Logger<ExamplePlugin>.Info(TranslationController.Instance.GetString((StringNames) 1337));
                }

                if (AmongUsClient.Instance && PlayerControl.LocalPlayer)
                {
                    if (GUILayout.Button("Send ExampleRpc"))
                    {
                        var playerName = PlayerControl.LocalPlayer.Data.PlayerName;
                        Rpc<ExampleRpc>.Instance.Send(new ExampleRpc.Data($"Send: from {playerName}"), ackCallback: () =>
                        {
                            Logger<ExamplePlugin>.Info("Got an acknowledgement for example rpc");
                        });

                        if (!AmongUsClient.Instance.AmHost)
                        {
                            Rpc<ExampleRpc>.Instance.SendTo(AmongUsClient.Instance.HostId, new ExampleRpc.Data($"SendTo: from {playerName} to host"));
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
