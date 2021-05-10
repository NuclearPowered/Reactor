using System;
using BepInEx;
using BepInEx.IL2CPP;
using Reactor.Extensions;
using Reactor.Networking;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor.Example
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class ExamplePlugin : BasePlugin
    {
        public const string Id = "gg.reactor.Example";

        public override void Load()
        {
            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);

            var gameObject = new GameObject(nameof(ReactorPlugin)).DontDestroy();
            gameObject.AddComponent<ExampleComponent>().Plugin = this;
        }

        [RegisterInIl2Cpp]
        public class ExampleComponent : MonoBehaviour
        {
            [HideFromIl2Cpp]
            public ExamplePlugin Plugin { get; internal set; }

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
