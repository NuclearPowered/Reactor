using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP;
using Reactor.Patches;
using Reactor.Utilities.Extensions;

namespace Reactor.Utilities;

/// <summary>
/// Provides a way for mods to show their version information in-game.
/// </summary>
public static class ReactorCredits
{
    private readonly struct ModIdentifier(string name, string version, Func<Location, bool>? shouldShow, bool isPreRelease)
    {
        private const string NormalColor = "#fff";
        private const string PreReleaseColor = "#f00";

        public string Name => name;

        public string Text { get; } = $"{name} {version}".EscapeRichText().Color(isPreRelease ? PreReleaseColor : NormalColor);

        public bool ShouldShow(Location location)
        {
            return shouldShow == AlwaysShow || shouldShow(location);
        }
    }

    private static readonly List<ModIdentifier> _modIdentifiers = [];

    /// <summary>
    /// Represents the location of where the credit is shown.
    /// </summary>
    public enum Location
    {
        /// <summary>
        /// In the main menu under Reactor/BepInEx versions.
        /// </summary>
        MainMenu,

        /// <summary>
        /// During game under the ping tracker.
        /// </summary>
        PingTracker,
    }

    /// <summary>
    /// A special value indicating a mod should always show.
    /// </summary>
    public const Func<Location, bool>? AlwaysShow = null;

    /// <summary>
    /// Registers a mod with the <see cref="ReactorCredits"/>, adding it to the list of mods that will be displayed.
    /// </summary>
    /// <param name="name">The user-friendly name of the mod. Can contain spaces or special characters.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="isPreRelease">If this version is a development or beta version. If true, it will display the mod in red.</param>
    /// <param name="shouldShow">
    /// This function will be called every frame to determine if the mod should be displayed or not.
    /// This function should return false if your mod is currently disabled or has no effect on gameplay at the time.
    /// If you want the mod to be displayed at all times, you can set this parameter to <see cref="ReactorCredits.AlwaysShow"/>.
    /// </param>
    public static void Register(string name, string version, bool isPreRelease, Func<Location, bool>? shouldShow)
    {
        const int MaxLength = 60;

        if (name.Length + version.Length > MaxLength)
        {
            Error($"Not registering mod \"{name}\" with version \"{version}\" in {nameof(ReactorCredits)} because the combined length of the mod name and version is greater than {MaxLength} characters.");
            return;
        }

        if (_modIdentifiers.Any(m => m.Name == name))
        {
            Error($"Mod \"{name}\" is already registered in {nameof(ReactorCredits)}.");
            return;
        }

        _modIdentifiers.Add(new ModIdentifier(name, version, shouldShow, isPreRelease));

        _modIdentifiers.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        if (!isPreRelease)
        {
            Info($"Mod \"{name}\" registered in {nameof(ReactorCredits)} with version {version}.");
        }
        else
        {
            Warning($"Mod \"{name}\" registered in {nameof(ReactorCredits)} with DEVELOPMENT/BETA version {version}.");
        }

        ReactorVersionShower.UpdateText();
    }

    /// <summary>
    /// Registers a mod with the <see cref="ReactorCredits"/>, adding it to the list of mods that will be displayed.
    /// </summary>
    /// <typeparam name="T">The BepInEx plugin type to get the name and version from.</typeparam>
    /// <param name="shouldShow"><inheritdoc cref="Register(string,string,bool,System.Func{Location,bool})" path="/param[@name='shouldShow']"/></param>
    public static void Register<T>(Func<Location, bool>? shouldShow) where T : BasePlugin
    {
        var pluginInfo = IL2CPPChainloader.Instance.Plugins.Values.SingleOrDefault(p => p.TypeName == typeof(T).FullName)
                         ?? throw new ArgumentException("Couldn't find the metadata for the provided plugin type", nameof(T));

        var metadata = pluginInfo.Metadata;

        Register(metadata.Name, metadata.Version.WithoutBuild().Clean(), metadata.Version.IsPreRelease, shouldShow);
    }

    internal static string? GetText(Location location)
    {
        var modTexts = _modIdentifiers.Where(m => m.ShouldShow(location)).Select(m => m.Text).ToArray();
        if (modTexts.Length == 0) return null;

        return location switch
        {
            Location.MainMenu => string.Join('\n', modTexts),
            Location.PingTracker => ("<space=3em>" + string.Join(", ", modTexts)).Size("50%").Align("center"),
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null),
        };
    }
}
