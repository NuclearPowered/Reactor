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

    private static readonly Il2CppSystem.Action<float> _setMainMenuPositionFromAspect = (Action<float>) (aspect =>
    {
        if (Text == null) return;
        var pos = new Vector3(-1.2287f * aspect + 10.9f, -0.57f, 4.5f);
        Text.transform.position = pos;
    });

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

            Text = gameObject.AddComponent<TextMeshPro>();
            Text.font = originalText.font;
            Text.fontMaterial = originalText.fontMaterial;
            Text.UpdateFontAsset();
            Text.overflowMode = TextOverflowModes.Overflow;
            Text.fontSize = 2;
            Text.outlineWidth = 0.1f;
            Text.enableWordWrapping = false;
            Text.alignment = TextAlignmentOptions.TopLeft;

            if (scene.name == "MainMenu")
            {
                ResolutionManager.add_ResolutionChanged(_setMainMenuPositionFromAspect);
                _setMainMenuPositionFromAspect.Invoke(Screen.width / (float) Screen.height);
            }
            else
            {
                ResolutionManager.remove_ResolutionChanged(_setMainMenuPositionFromAspect);
                var aspectPosition = gameObject.AddComponent<AspectPosition>();
                var distanceFromEdge = new Vector3(10.33f, 2.55f, -1);
                if (originalAspectPosition.Alignment == AspectPosition.EdgeAlignments.LeftTop)
                {
                    distanceFromEdge.y += 0.2f;
                    distanceFromEdge.x -= 0.2f;
                }
                else if (AccountManager.Instance.isActiveAndEnabled)
                {
                    distanceFromEdge.y += 0.575f;
                }

                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
                aspectPosition.DistanceFromEdge = distanceFromEdge;
                aspectPosition.AdjustPosition();
            }
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
