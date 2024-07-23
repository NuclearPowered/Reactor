using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Reactor.Debugger.Window.Tabs;

internal sealed class GameTab : BaseTab
{
    public override string Name => "Game";

    public override void OnGUI()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (!localPlayer)
        {
            return;
        }

        var amHost = AmongUsClient.Instance.AmHost;

        var position = localPlayer.transform.position;
        GUILayout.Label($"Position: ({position.x:F3}, {position.y:F3})");

        var physics = localPlayer.MyPhysics;
        GUILayout.Label($"Speed: {physics.Speed}");
        physics.Speed = GUILayout.HorizontalSlider(physics.Speed, 0, 25);

        localPlayer.Collider.enabled = GUILayout.Toggle(localPlayer.Collider.enabled, "Collisions");

        if (ShipStatus.Instance)
        {
            if (amHost && GUILayout.Button("Force game end"))
            {
                ShipStatus.Instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
            }

            if (MeetingHud.Instance)
            {
                if (GUILayout.Button("Stop the meeting"))
                {
                    MeetingHud.Instance.RpcClose();
                }
            }
            else
            {
                if (GUILayout.Button("Call a meeting"))
                {
                    localPlayer.CmdReportDeadBody(null);
                }
            }

            if (TutorialManager.InstanceExists)
            {
                var newIsImpostor = GUILayout.Toggle(localPlayer.Data.Role.IsImpostor, "Is Impostor");
                if (localPlayer.Data.Role.IsImpostor != newIsImpostor)
                {
                    localPlayer.RpcSetRole(newIsImpostor ? RoleTypes.Impostor : RoleTypes.Crewmate);
                }

                if (GUILayout.Button("Spawn a dummy"))
                {
                    SpawnDummy();
                }
            }
        }
    }

    private static void SpawnDummy()
    {
        var playerControl = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
        var playerId = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

        var data = GameData.Instance.AddDummy(playerControl);
        AmongUsClient.Instance.Spawn(data);
        AmongUsClient.Instance.Spawn(playerControl);
        playerControl.isDummy = true;

        playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
        playerControl.GetComponent<DummyBehaviour>().enabled = true;
        playerControl.NetTransform.enabled = false;

        playerControl.SetName($"{TranslationController.Instance.GetString(StringNames.Dummy)} {playerId}");

        var color = (byte) (playerId % Palette.PlayerColors.Length);
        playerControl.SetColor(color);
        playerControl.SetHat(HatManager.Instance.allHats[playerId % HatManager.Instance.allHats.Count].ProdId, playerControl.Data.DefaultOutfit.ColorId);
        playerControl.SetPet(HatManager.Instance.allPets[playerId % HatManager.Instance.allPets.Count].ProdId);
        playerControl.SetSkin(HatManager.Instance.allSkins[playerId % HatManager.Instance.allSkins.Count].ProdId, color);
        playerControl.SetVisor(HatManager.Instance.allVisors[playerId % HatManager.Instance.allVisors.Count].ProdId, color);
        playerControl.SetNamePlate(HatManager.Instance.allNamePlates[playerId % HatManager.Instance.allNamePlates.Count].ProdId);
        data.PlayerLevel = playerId;

        data.RpcSetTasks(new Il2CppStructArray<byte>(0));
    }
}
