using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactor.Utilities;

/// <summary>
/// Controls the PingTracker.
/// </summary>
public static class ReactorPingTracker
{
    private readonly struct ModIdentifier(string modName, string version, Func<bool>? shouldShow, bool isDevBuild)
    {
        private static string NormalColor => !AmongUsClient.Instance.IsGameStarted ? "#fff" : "#fff7";
        private static string DevColor => !AmongUsClient.Instance.IsGameStarted ? "#f00" : "#f447";

        public string ModName => modName;
        public string Text => $"</noparse><color={(isDevBuild ? DevColor : NormalColor)}><noparse>{ModName} {version}</noparse></color><noparse>";

        public bool ShouldShow() => shouldShow == AlwaysShow || shouldShow();
    }

    private static readonly List<ModIdentifier> _modIdentifiers = [];

    /// <summary>
    /// A special value indicating a mod should always show.
    /// </summary>
    public const Func<bool>? AlwaysShow = null;

    /// <summary>
    /// Registers a mod with the <see cref="ReactorPingTracker"/>, adding it to the list of mods that will be displayed in the PingTracker.
    /// </summary>
    /// <param name="modName">The user-friendly name of the mod. Can contain spaces or special characters.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="shouldShow">
    /// This function will be called every frame to determine if the mod should be displayed or not.
    /// This function should return false if your mod is currently disabled or has no effect on gameplay at the time.
    /// If you want the mod to be displayed at all times, you can set this parameter to <see cref="ReactorPingTracker.AlwaysShow"/>.
    /// </param>
    /// <param name="isDevOrBetaBuild">If this version is a development or beta version. If true, it will display the mod in red in the PingTracker.</param>
    public static void RegisterMod(string modName, string version, Func<bool>? shouldShow, bool isDevOrBetaBuild = false)
    {
        const int MaxLength = 60;

        if (modName.Length + version.Length > MaxLength)
        {
            Error($"Not registering mod \"{modName}\" with version \"{version}\" in {nameof(ReactorPingTracker)} because the combined length of the mod name and version is greater than {MaxLength} characters.");
            return;
        }

        if (modName.Contains("</noparse>", StringComparison.OrdinalIgnoreCase) || version.Contains("</noparse>", StringComparison.OrdinalIgnoreCase))
        {
            Error($"Not registering mod \"{modName}\" with version \"{version}\" in {nameof(ReactorPingTracker)} because it contains the string \"</noparse>\" which is disallowed.");
            return;
        }

        if (_modIdentifiers.Any(m => m.ModName == modName))
        {
            Error($"Mod \"{modName}\" is already registered in {nameof(ReactorPingTracker)}.");
            return;
        }

        _modIdentifiers.Add(new ModIdentifier(modName, version, shouldShow, isDevOrBetaBuild));

        _modIdentifiers.Sort((a, b) => string.Compare(a.ModName, b.ModName, StringComparison.Ordinal));

        if (!isDevOrBetaBuild)
        {
            Info($"Mod \"{modName}\" registered in {nameof(ReactorPingTracker)} with version {version}.");
        }
        else
        {
            Warning($"Mod \"{modName}\" registered in {nameof(ReactorPingTracker)} with DEVELOPMENT/BETA version {version}.");
        }
    }

    internal static string GetPingTrackerText()
    {
        return "<align=center><size=50%><space=3em><noparse>" + string.Join(", ", _modIdentifiers.Where(m => m.ShouldShow()).Select(m => m.Text)) + "</noparse></size></align>";
    }
}
