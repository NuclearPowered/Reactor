using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactor.Patches;

/// <summary>
/// Controls the PingTracker.
/// </summary>
public static class ReactorPingTracker
{
    private struct ModIdentifier
    {
        private static string NormalColor => !AmongUsClient.Instance.IsGameStarted ? "#fff" : "#fff8";
        private static string DevColor => !AmongUsClient.Instance.IsGameStarted ? "#f00" : "#f008";

        public string ModName;
        public string Version;
        public bool IsDevBuild;
        public Func<bool>? ShouldShow;

        public readonly string Text => $"</noparse><color={(IsDevBuild ? DevColor : NormalColor)}><noparse>{ModName} {Version}</noparse></color><noparse>";
    }

    private static readonly List<ModIdentifier> _modIdentifiers =
    [
        new ModIdentifier
        {
            ModName = ReactorPlugin.Name,
            Version = ReactorPlugin.Version,
#if DEBUG
            IsDevBuild = true,
#else
            IsDevBuild = false,
#endif
            ShouldShow = null//() => AmongUsClient.Instance.IsGameStarted,
        }
    ];

    /// <summary>
    /// Registers a mod with the PingTrackerManager, adding it to the list of mods that will be displayed in the PingTracker.
    /// </summary>
    /// <param name="modName">The user-friendly name of the mod. Can contain spaces or special characters.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="isDevOrBetaBuild">If this version is a development or beta version. If true, it will display the mod in red in the PingTracker.</param>
    /// <param name="shouldShow">This function will be called every frame to determine if the mod should be displayed or not. This function should return false if your mod is currently disabled or has no effect on gameplay at the time. If you want the mod to be displayed at all times, set this parameter to null to avoid delegate calls.</param>
    public static void RegisterMod(string modName, string version, bool isDevOrBetaBuild, Func<bool>? shouldShow)
    {
        if (modName.Contains("</noparse>", StringComparison.OrdinalIgnoreCase) || version.Contains("</noparse>", StringComparison.OrdinalIgnoreCase))
        {
            Error($"Not registering mod \"{modName}\" with version \"{version}\" in PingTrackerManager because it contains the string \"</noparse>\" which is disallowed.");
            return;
        }

        if (_modIdentifiers.Any(m => m.ModName == modName))
        {
            Error($"Mod \"{modName}\" is already registered in PingTrackerManager.");
            return;
        }

        _modIdentifiers.Add(new ModIdentifier
        {
            ModName = modName,
            Version = version,
            IsDevBuild = isDevOrBetaBuild,
            ShouldShow = shouldShow,
        });

        _modIdentifiers.Sort((a, b) => string.Compare(a.ModName, b.ModName, StringComparison.Ordinal));

        if (!isDevOrBetaBuild)
        {
            Info($"Mod \"{modName}\" registered in PingTrackerManager with version {version}.");
        }
        else
        {
            Warning($"Mod \"{modName}\" registered in PingTrackerManager with DEVELOPMENT/BETA version {version}.");
        }
    }

    internal static string GetPingTrackerText()
    {
        return "<align=center><size=50%><noparse>                       " + string.Join(", ", _modIdentifiers.Where(m => m.ShouldShow?.Invoke() ?? true).Select(m => m.Text)) + "</noparse></size></align>";
    }
}
