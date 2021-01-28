using System;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using Reactor.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reactor.Patches
{
    public static class ReactorVersionShower
    {
        public static TextRenderer Text { get; private set; }

        public static event TextUpdatedHandler TextUpdated;

        public delegate void TextUpdatedHandler(TextRenderer text);

        internal static void Initialize()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((_, _) =>
            {
                var original = UnityEngine.Object.FindObjectOfType<VersionShower>();
                if (!original)
                    return;

                TextRendererExtensions.Prefab ??= new TextRendererPrefab(original.gameObject.GetComponentInChildren<TextRenderer>());

                var gameObject = new GameObject("ReactorVersion");
                gameObject.transform.parent = original.transform.parent;

                var aspectPosition = gameObject.AddComponent<AspectPosition>();

                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;

                var position = original.GetComponent<AspectPosition>().DistanceFromEdge;
                position.y += 0.2f;
                aspectPosition.DistanceFromEdge = position;

                aspectPosition.AdjustPosition();

                Text = gameObject.AddTextRenderer();
                Text.scale = 0.65f;

                UpdateText();
            }));
        }

        public static void UpdateText()
        {
            Text.Text = "Reactor " + typeof(ReactorPlugin).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Text.Text += "\nBepInEx: " + Paths.BepInExVersion;
            Text.Text += "\nMods: " + IL2CPPChainloader.Instance.Plugins.Count;
            TextUpdated?.Invoke(Text);
        }
    }
}
