using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Reactor.Debugger
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id, "^0.2.0")]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public class DebuggerPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.debugger";

        public Harmony Harmony { get; } = new Harmony(Id);
        public DebuggerComponent Component { get; private set; }

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DebuggerComponent>();

            var gameObject = new GameObject(nameof(DebuggerPlugin)).DontDestroy();
            Component = gameObject.AddComponent<DebuggerComponent>();

            Harmony.PatchAll();

            Extensions.Extensions.once_sceneLoaded((_, _) =>
            {
                Dumping.Dump();
            });
        }

        public class DebuggerComponent : MonoBehaviour
        {
            public bool DisableGameEnd { get; set; }

            public DragWindow TestWindow { get; }

            public DebuggerComponent(IntPtr ptr) : base(ptr)
            {
                TestWindow = new DragWindow(new Rect(20, 20, 120, 120), "Debugger", () =>
                {
                    GUILayout.Label("Name: " + SaveManager.PlayerName, new Il2CppReferenceArray<GUILayoutOption>(0));
                    DisableGameEnd = GUILayout.Toggle(DisableGameEnd, "Disable game end", new Il2CppReferenceArray<GUILayoutOption>(0));

                    if (AmongUsClient.Instance.AmHost && ShipStatus.Instance && GUILayout.Button("Force game end", new Il2CppReferenceArray<GUILayoutOption>(0)))
                    {
                        ShipStatus.Instance.enabled = false;
                        ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    }

                    if (PlayerControl.LocalPlayer)
                    {
                        var position = PlayerControl.LocalPlayer.transform.position;
                        GUILayout.Label($"x: {position.x}", new Il2CppReferenceArray<GUILayoutOption>(0));
                        GUILayout.Label($"y: {position.y}", new Il2CppReferenceArray<GUILayoutOption>(0));
                    }
                })
                {
                    Enabled = false
                };
            }

            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    TestWindow.Enabled = !TestWindow.Enabled;
                }
            }

            private void OnGUI()
            {
                TestWindow.OnGUI();
            }
        }
    }

    // [HarmonyPatch]
    // public static class TracePatch
    // {
    //     public static IEnumerable<MethodBase> TargetMethods()
    //     {
    //         return typeof(AmongUsClient).GetMethods(AccessTools.all).Where(x => x.ReturnType == typeof(Il2CppStructArray<byte>) && !x.GetParameters().Any());
    //     }
    //
    //     public static void Postfix(MethodBase __originalMethod)
    //     {
    //         PluginSingleton<DebuggerPlugin>.Instance.Log.LogInfo(__originalMethod.FullDescription());
    //     }
    // }
}
