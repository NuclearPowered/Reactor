using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class FreeNamePatch
{
    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CheckAndRegenerateName))]
    [HarmonyPrefix]
    public static bool DontRegenerateNames()
    {
        return false;
    }

    public static void Initialize()
    {
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (!scene.name.Equals("MMOnline", StringComparison.Ordinal)) return;
            if (!TryMoveObjects()) return;

            var editName = DestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
            var nameText = Object.Instantiate(editName.nameText.gameObject);

            nameText.transform.localPosition += Vector3.up * (AccountManager.Instance.isActiveAndEnabled ? 1.85f : 2.2f);

            var textBox = nameText.GetComponent<TextBoxTMP>();
            textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
            textBox.outputText.transform.position = nameText.transform.position;

            textBox.OnChange.AddListener((Action) (() =>
            {
                DataManager.Player.Customization.Name = textBox.text;
            }));
            textBox.OnEnter = textBox.OnFocusLost = textBox.OnChange;
        }));
    }

    private static bool TryMoveObjects()
    {
        var toMove = new List<string>
        {
            "HostGameButton",
            "FindGameButton",
            "JoinGameButton",
        };

        var yStart = Vector3.up * (AccountManager.Instance.isActiveAndEnabled ? 0.9f : 1.1f);
        var yOffset = Vector3.down * 1.5f;

        var gameObjects = toMove.Select(x => GameObject.Find("NormalMenu/Buttons/" + x)).ToList();
        if (gameObjects.Any(x => x == null)) return false;

        for (var i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].transform.position = yStart + (yOffset * i);
        }

        return true;
    }
}
