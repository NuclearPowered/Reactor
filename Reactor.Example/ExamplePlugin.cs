using System;
using BepInEx;
using BepInEx.IL2CPP;
using Reactor.Extensions;
using Reactor.Networking;
using Reactor.Networking.MethodRpc;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reactor.Example
{
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
            public ExampleComponent(IntPtr ptr) : base(ptr)
            {
            }

            private void Update()
            {
                if (AmongUsClient.Instance && PlayerControl.LocalPlayer)
                {
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        var name = PlayerControl.LocalPlayer.Data.PlayerName;
                        Rpc<ExampleRpc>.Instance.Send(new ExampleRpc.Data($"Send: from {name}"));

                        if (!AmongUsClient.Instance.AmHost)
                        {
                            Rpc<ExampleRpc>.Instance.SendTo(AmongUsClient.Instance.HostId, new ExampleRpc.Data($"SendTo: from {name} to host"));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        RpcSay(PlayerControl.LocalPlayer, "Hello from method rpc", Random.value, PlayerControl.LocalPlayer);
                    }
                }
            }
        }

        [MethodRpc((uint) CustomRpcCalls.MethodRpcExample)]
        public static void RpcSay(PlayerControl player, string text, float number, PlayerControl testPlayer)
        {
            Logger<ExamplePlugin>.Info($"{player.Data.PlayerName} text: {text} number: {number} testPlayer: {testPlayer.NetId}");
        }
    }
}
