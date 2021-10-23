using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Reactor.Patches
{
    public static class FreeNamePatch
    {
        public static void Initialize()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
            {
                if (!scene.name.Equals("MMOnline")) return;
                if (!TryMoveObjects()) return;
                
                var editName = DestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen;
                var nameText = Object.Instantiate(editName.nameText.gameObject);

                nameText.transform.localPosition += Vector3.up * 2.2f;

                var textBox = nameText.GetComponent<TextBoxTMP>();
                textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
                textBox.outputText.transform.position = nameText.transform.position;
                textBox.outputText.fontSize = 4f;
                
                textBox.OnChange.AddListener((Action) (() => {
                    SaveManager.PlayerName = textBox.text;
                }));
                textBox.OnEnter = textBox.OnFocusLost = textBox.OnChange;
                
                textBox.Pipe.GetComponent<TextMeshPro>().fontSize = 4f;
            }));
        }

        private static bool TryMoveObjects()
        {
            var toMove = new List<string>
            {
                "HostGameButton",
                "FindGameButton",
                "JoinGameButton"
            };

            var offset = Vector3.up;
            var gameObjects = toMove.Select(x => GameObject.Find("NormalMenu/" + x)).ToList();
            if (gameObjects.Contains(null)) return false;
            
            gameObjects.ForEach(x => {
                x.transform.position = offset;
                offset += Vector3.down * 1.5f;
            });

            return true;
        }
    }
}
