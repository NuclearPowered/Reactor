using System;
using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using InnerNet;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Reactor.Debugger
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [ReactorPluginSide(PluginSide.ClientOnly)]
    public class DebuggerPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.debugger";

        public Harmony Harmony { get; } = new Harmony(Id);
        public DebuggerComponent Component { get; private set; }

        public override void Load()
        {
            RegisterInIl2CppAttribute.Register();

            var gameObject = new GameObject(nameof(DebuggerPlugin)).DontDestroy();
            Component = gameObject.AddComponent<DebuggerComponent>();

            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat((int) byte.MaxValue, byte.MaxValue).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(1, 4).ToArray();

            Harmony.PatchAll();
        }

        [RegisterInIl2Cpp]
        public class DebuggerComponent : MonoBehaviour
        {
            [HideFromIl2Cpp]
            public bool DisableGameEnd { get; set; }

            [HideFromIl2Cpp]
            public DragWindow TestWindow { get; }

            public DebuggerComponent(IntPtr ptr) : base(ptr)
            {
                TestWindow = new DragWindow(new Rect(20, 20, 0, 0), "Debugger", () =>
                {
                    GUILayout.Label("Name: " + SaveManager.PlayerName, new Il2CppReferenceArray<GUILayoutOption>(0));
                    DisableGameEnd = GUILayout.Toggle(DisableGameEnd, "Disable game end", new Il2CppReferenceArray<GUILayoutOption>(0));

                    if (ShipStatus.Instance && AmongUsClient.Instance.AmHost)
                    {
                        if (GUILayout.Button("Force game end", new Il2CppReferenceArray<GUILayoutOption>(0)))
                        {
                            ShipStatus.Instance.enabled = false;
                            ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                        }

                        if (GUILayout.Button("Call a meeting", new Il2CppReferenceArray<GUILayoutOption>(0)))
                        {
                            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                        }
                    }

                    if (TutorialManager.InstanceExists && PlayerControl.LocalPlayer)
                    {
                        var data = PlayerControl.LocalPlayer.Data;

                        var newIsImpostor = GUILayout.Toggle(data.IsImpostor, "Is Impostor", new Il2CppReferenceArray<GUILayoutOption>(0));
                        if (data.IsImpostor != newIsImpostor)
                        {
                            if (newIsImpostor)
                            {
                                PlayerControl.LocalPlayer.RpcSetInfected(new[]
                                {
                                    data
                                });
                            }
                            else
                            {
                                PlayerControl.LocalPlayer.RemoveInfected();
                            }
                        }

                        if (GUILayout.Button("Spawn a dummy", new Il2CppReferenceArray<GUILayoutOption>(0)))
                        {
                            var playerControl = Instantiate(TutorialManager.Instance.PlayerPrefab);
                            var i = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();
                            GameData.Instance.AddPlayer(playerControl);
                            AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                            playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                            playerControl.GetComponent<DummyBehaviour>().enabled = true;
                            playerControl.NetTransform.enabled = false;
                            playerControl.SetName($"{TranslationController.Instance.GetString(StringNames.Dummy, Array.Empty<Il2CppSystem.Object>())} {i}");
                            playerControl.SetColor((byte) (i % Palette.PlayerColors.Length));
                            playerControl.SetHat(i % (uint) HatManager.Instance.AllHats.Count, playerControl.Data.ColorId);
                            playerControl.SetPet(i % (uint) HatManager.Instance.AllPets.Count);
                            playerControl.SetSkin(i % (uint) HatManager.Instance.AllSkins.Count);
                            GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
                        }
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
}
