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
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((_, _) =>
        {
            var original = UnityEngine.Object.FindObjectOfType<VersionShower>();
            if (!original)
                return;

            var gameObject = new GameObject("ReactorVersion " + Guid.NewGuid());
            gameObject.transform.parent = original.transform.parent;

            var aspectPosition = gameObject.AddComponent<AspectPosition>();

            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;

            var originalAspectPosition = original.GetComponent<AspectPosition>();
            var originalPosition = originalAspectPosition.DistanceFromEdge;
            originalPosition.y = 0.15f;
            originalAspectPosition.DistanceFromEdge = originalPosition;
            originalAspectPosition.AdjustPosition();

            var position = originalPosition;
            position.x += 10.075f - 0.1f;
            position.y += 2.75f - 0.15f;
            position.z -= 1;
            aspectPosition.DistanceFromEdge = position;

            aspectPosition.AdjustPosition();

            Text = gameObject.AddComponent<TextMeshPro>();
            Text.fontSize = 2;

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
