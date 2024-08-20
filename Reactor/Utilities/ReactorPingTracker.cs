using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP;
using Reactor.Utilities.Extensions;

namespace Reactor.Utilities;

/// <summary>
/// Controls the PingTracker.
/// </summary>
public static class ReactorPingTracker
{
    private readonly struct ModIdentifier(string name, string version, Func<bool>? shouldShow, bool isPreRelease)
    {
        private const string NormalColor = "#fff";
        private const string PreReleaseColor = "#f00";

        public string Name => name;

        public bool ShouldShow => shouldShow == AlwaysShow || shouldShow();
        public string Text { get; } = $"{name} {version}".EscapeRichText().Color(isPreRelease ? PreReleaseColor : NormalColor);
    }

    private static readonly List<ModIdentifier> _modIdentifiers = [];

    /// <summary>
    /// A special value indicating a mod should always show.
    /// </summary>
    public const Func<bool>? AlwaysShow = null;

    /// <summary>
    /// Registers a mod with the <see cref="ReactorPingTracker"/>, adding it to the list of mods that will be displayed in the PingTracker.
    /// </summary>
    /// <param name="name">The user-friendly name of the mod. Can contain spaces or special characters.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="isPreRelease">If this version is a development or beta version. If true, it will display the mod in red in the PingTracker.</param>
    /// <param name="shouldShow">
    /// This function will be called every frame to determine if the mod should be displayed or not.
    /// This function should return false if your mod is currently disabled or has no effect on gameplay at the time.
    /// If you want the mod to be displayed at all times, you can set this parameter to <see cref="ReactorPingTracker.AlwaysShow"/>.
    /// </param>
    public static void Register(string name, string version, bool isPreRelease, Func<bool>? shouldShow)
    {
        const int MaxLength = 60;

        if (name.Length + version.Length > MaxLength)
        {
            Error($"Not registering mod \"{name}\" with version \"{version}\" in {nameof(ReactorPingTracker)} because the combined length of the mod name and version is greater than {MaxLength} characters.");
            return;
        }

        if (_modIdentifiers.Any(m => m.Name == name))
        {
            Error($"Mod \"{name}\" is already registered in {nameof(ReactorPingTracker)}.");
            return;
        }

        _modIdentifiers.Add(new ModIdentifier(name, version, shouldShow, isPreRelease));

        _modIdentifiers.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        if (!isPreRelease)
        {
            Info($"Mod \"{name}\" registered in {nameof(ReactorPingTracker)} with version {version}.");
        }
        else
        {
            Warning($"Mod \"{name}\" registered in {nameof(ReactorPingTracker)} with DEVELOPMENT/BETA version {version}.");
        }
    }

    /// <summary>
    /// Registers a mod with the <see cref="ReactorPingTracker"/>, adding it to the list of mods that will be displayed in the PingTracker.
    /// </summary>
    /// <typeparam name="T">The BepInEx plugin type to get the name and version from.</typeparam>
    /// <param name="shouldShow"><inheritdoc cref="Register(string,string,bool,System.Func{bool})" path="/param[@name='shouldShow']"/></param>
    public static void Register<T>(Func<bool>? shouldShow) where T : BasePlugin
    {
        var pluginInfo = IL2CPPChainloader.Instance.Plugins.Values.SingleOrDefault(p => p.TypeName == typeof(T).FullName)
                         ?? throw new ArgumentException("Couldn't find the metadata for the provided plugin type", nameof(T));

        var metadata = pluginInfo.Metadata;

        Register(metadata.Name, metadata.Version.WithoutBuild().Clean(), metadata.Version.IsPreRelease, shouldShow);
    }

    internal static string? GetText()
    {
        var mods = _modIdentifiers.Where(m => m.ShouldShow).Select(m => m.Text).ToArray();
        if (mods.Length == 0)
        {
            return null;
        }

        return ("<space=3em>" + string.Join(", ", mods)).Size("50%").Align("center");
    }
}
