using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities;

namespace Reactor.Localization.Providers;

/// <summary>
/// Utility for adding hard-coded localization.
/// </summary>
public sealed class HardCodedLocalizationProvider : LocalizationProvider
{
    private static readonly Dictionary<StringNames, string> _strings = new();

    /// <summary>
    /// Adds a custom, hard-coded translation for a <see cref="StringNames"/>.
    /// </summary>
    /// <param name="stringName">The <see cref="StringNames"/>.</param>
    /// <param name="value">The text.</param>
    public static void Register(StringNames stringName, string value)
    {
        if (_strings.ContainsKey(stringName))
        {
            Warning($"Registering StringName {stringName} that already exists");
        }

        _strings[stringName] = value;
    }

    /// <inheritdoc/>
    public override int Priority => ReactorPriority.Low;

    /// <inheritdoc/>
    public override bool TryGetText(StringNames stringName, out string? result)
    {
        return _strings.TryGetValue(stringName, out result);
    }
}
