using System;
using BepInEx;
using BepInEx.IL2CPP;
using Reactor.Networking;
using UnityEngine;

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
                if (Input.GetKeyDown(KeyCode.F3) && AmongUsClient.Instance && PlayerControl.LocalPlayer)
                {
                    var name = PlayerControl.LocalPlayer.Data.PlayerName;
                    Rpc<ExampleRpc>.Instance.Send(new ExampleRpc.Data($"Send: from {name}"));

                    if (!AmongUsClient.Instance.AmHost)
                    {
                        Rpc<ExampleRpc>.Instance.SendTo(AmongUsClient.Instance.HostId, new ExampleRpc.Data($"SendTo: from {name} to host"));
                    }
                }
            }
        }
    }
}
