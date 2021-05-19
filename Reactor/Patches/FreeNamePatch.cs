using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reactor.Patches
{
    internal static class FreeNamePatch
    {
        [HarmonyPatch(typeof(SetNameText), nameof(SetNameText.Start))]
        public static class NameInputPatch
        {
            public static void Postfix(SetNameText __instance)
            {
                if (!__instance)
                {
                    return;
                }

                var nameText = __instance.gameObject;

                var textBox = nameText.AddComponent<TextBoxTMP>();
                textBox.Background = nameText.GetComponentInChildren<SpriteRenderer>();
                textBox.OnChange = textBox.OnEnter = textBox.OnFocusLost = new Button.ButtonClickedEvent();
                textBox.characterLimit = 10;

                var textMeshPro = nameText.GetComponentInChildren<TextMeshPro>();
                textBox.outputText = textMeshPro;
                textBox.SetText(SaveManager.PlayerName);

                textBox.OnChange.AddListener((Action) (() =>
                {
                    SaveManager.PlayerName = textBox.text;
                }));

                var pipeGameObject = GameObject.Find("Pipe");
                if (!pipeGameObject)
                {
                    return;
                }

                var pipe = UnityEngine.Object.Instantiate(pipeGameObject, textMeshPro.transform);
                pipe.GetComponent<TextMeshPro>().fontSize = 4f;
                textBox.Pipe = pipe.GetComponent<MeshRenderer>();
            }
        }
    }
}
