using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Reactor.Utilities;
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

    private static void SetMainMenuPositionFromAspect(float aspectRatio)
    {
        if (Text == null) return;
        var pos = new Vector3(-1.2287f * aspectRatio + 10.9f, -0.57f, 4.5f);
        Text.transform.position = pos;
    }

    private static readonly ResolutionManager.ResolutionChangedHandler _resolutionChangedHandler = (Action<float, int, int, bool>) ((aspectRatio, _, _, _) =>
    {
        SetMainMenuPositionFromAspect(aspectRatio);
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
                ResolutionManager.add_ResolutionChanged(_resolutionChangedHandler);
                SetMainMenuPositionFromAspect(Screen.width / (float) Screen.height);
            }
            else
            {
                ResolutionManager.remove_ResolutionChanged(_resolutionChangedHandler);
                var aspectPosition = gameObject.AddComponent<AspectPosition>();
                var distanceFromEdge = new Vector3(10.13f, 2.55f, -1);
                if (originalAspectPosition.Alignment == AspectPosition.EdgeAlignments.LeftTop)
                {
                    distanceFromEdge.y += 0.2f;
                }
                else if (AccountManager.Instance.isActiveAndEnabled)
                {
                    distanceFromEdge.x += 0.2f;
                    distanceFromEdge.y += 0.575f;
                }

                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
                aspectPosition.DistanceFromEdge = distanceFromEdge;
                aspectPosition.AdjustPosition();
            }

            UpdateText();
        }));
    }

    /// <summary>
    /// Updates <see cref="Text"/> with reactor version and fires <see cref="TextUpdated"/>.
    /// </summary>
    public static void UpdateText()
    {
        if (Text == null) return;
        Text.text = "Reactor " + Version.Parse(ReactorPlugin.Version).WithoutBuild();
        Text.text += "\nBepInEx " + Paths.BepInExVersion.WithoutBuild();
        Text.text += "\nMods: " + IL2CPPChainloader.Instance.Plugins.Count;

        var creditsText = ReactorCredits.GetText(ReactorCredits.Location.MainMenu);
        if (creditsText != null)
        {
            Text.text += "\n" + creditsText;
        }

        TextUpdated?.Invoke(Text);
    }
}
