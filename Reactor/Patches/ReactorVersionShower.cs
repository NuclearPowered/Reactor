using System;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
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
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, loadMode) =>
            {
                var original = UnityEngine.Object.FindObjectOfType<VersionShower>();
                if (!original)
                    return;

                var gameObject = UnityEngine.Object.Instantiate(original.gameObject);
                gameObject.name = "ReactorVersion";

                var versionShower = gameObject.GetComponent<VersionShower>();
                if (versionShower)
                {
                    versionShower.enabled = false;
                }

                var aspectPosition = gameObject.GetComponent<AspectPosition>();

                var position = aspectPosition.DistanceFromEdge;
                position.y += 0.2f;
                aspectPosition.DistanceFromEdge = position;

                aspectPosition.AdjustPosition();

                Text = gameObject.GetComponentInChildren<TextRenderer>();
                UpdateText();
            }));
        }

        public static void UpdateText()
        {
            Text.Text = "Reactor " + typeof(ReactorPlugin).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Text.Text += "\nBepInEx: " + Paths.BepInExVersion;
            Text.Text += "\nMods: " + Preloader.Chainloader.Plugins.Count;
            TextUpdated?.Invoke(Text);
        }
    }
}
