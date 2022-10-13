using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities;

namespace Reactor.Localization.Providers;

/// <summary>
/// Utility for adding hard-coded localization.
/// </summary>
public sealed class HardCodedLocalizationProvider : LocalizationProvider
{
    internal static readonly Dictionary<StringNames, string> Strings = new();

    /// <summary>
    /// Adds a custom, hard-coded translation for a <see cref="StringNames"/>.
    /// </summary>
    /// <param name="stringName">The <see cref="StringNames"/>.</param>
    /// <param name="value">The text.</param>
    public static void Register(StringNames stringName, string value)
    {
        if (Strings.ContainsKey(stringName))
        {
            Warning($"Registering StringName {stringName} that already exists");
        }

        Strings[stringName] = value;
    }

    /// <inheritdoc/>
    public override int Priority => ReactorPriority.Low;

    /// <inheritdoc/>
    public override bool TryGetText(StringNames stringName, [NotNullWhen(true)] out string? result)
    {
        if (!Strings.ContainsKey(stringName))
        {
            result = null;
            return false;
        }

        result = Strings[stringName];
        return true;
    }
}
