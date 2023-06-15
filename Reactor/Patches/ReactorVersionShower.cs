using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Version = SemanticVersioning.Version;

namespace Reactor.Patches;

/// <summary>
/// Shows the reactor version on the main menu.
/// </summary>
public static class ReactorVersionShower
{
    /// <summary>
    /// Gets the <see cref="TextMeshPro"/> text.
    /// </summary>
    public static TextMeshPro? Text { get; private set; }

    /// <summary>
    /// Occurs when <see cref="Text"/> is updated.
    /// </summary>
    public static event Action<TextMeshPro>? TextUpdated;

    internal static void Initialize()
    {
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            var original = UnityEngine.Object.FindObjectOfType<VersionShower>();
            if (!original)
                return;

            var originalAspectPosition = original.GetComponent<AspectPosition>();
            var originalText = original.GetComponentInChildren<TextMeshPro>();

            var gameObject = new GameObject("ReactorVersion");

            var aspectPosition = gameObject.AddComponent<AspectPosition>();

            if (scene.name == "MainMenu")
            {
                aspectPosition.Alignment = AspectPosition.EdgeAlignments.Left;
                aspectPosition.DistanceFromEdge = new Vector3(5f, 1.55f, 2f);
            }
            else
            {
                var distanceFromEdge = new Vector3(1, 0.4f, -1);
                if (originalAspectPosition.Alignment == AspectPosition.EdgeAlignments.LeftTop)
                {
                    distanceFromEdge += new Vector3(0.05f, 0.15f, 0);
                }
                else if (AccountManager.Instance.isActiveAndEnabled)
                {
                    distanceFromEdge += new Vector3(0.2f, 0.6f, 0);
                }

                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
                aspectPosition.DistanceFromEdge = distanceFromEdge;
            }

            aspectPosition.AdjustPosition();

            Text = gameObject.AddComponent<TextMeshPro>();
            Text.font = originalText.font;
            Text.fontMaterial = originalText.fontMaterial;
            Text.UpdateFontAsset();
            Text.alignment = TextAlignmentOptions.TopLeft;
            Text.autoSizeTextContainer = true;
            Text.fontSize = 2;
            Text.outlineWidth = 0.1f;

            UpdateText();
        }));
    }

    private static string ToStringWithoutBuild(Version version)
    {
        return $"{version.Major}.{version.Minor}.{version.Patch}{(version.PreRelease == null ? string.Empty : $"-{version.PreRelease}")}";
    }

    /// <summary>
    /// Updates <see cref="Text"/> with reactor version and fires <see cref="TextUpdated"/>.
    /// </summary>
    public static void UpdateText()
    {
        if (Text == null) return;
        Text.text = "Reactor " + ReactorPlugin.Version;
        Text.text += "\nBepInEx " + ToStringWithoutBuild(Paths.BepInExVersion);
        Text.text += "\nMods: " + IL2CPPChainloader.Instance.Plugins.Count;
        TextUpdated?.Invoke(Text);
    }

    [HarmonyPatch(typeof(FreeWeekendShower), nameof(FreeWeekendShower.Start))]
    private static class FreeWeekendShowerPatch
    {
        public static bool Prefix(FreeWeekendShower __instance)
        {
            __instance.Output.Destroy();
            return false;
        }
    }
}
