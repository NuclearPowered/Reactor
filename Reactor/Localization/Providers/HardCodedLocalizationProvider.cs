using System;
using System.Collections.Generic;
using Reactor.Localization.Utilities;
using Reactor.Utilities;

namespace Reactor.Localization.Providers;

/// <summary>
/// Adds localization for strings registered through <see cref="CustomStringName.Register"/>.
/// </summary>
public sealed class HardCodedLocalizationProvider : LocalizationProvider
{
    internal static readonly Dictionary<StringNames, CustomStringName> Strings = new();

    /// <inheritdoc/>
    public override int Priority => ReactorPriority.Low;

    /// <inheritdoc/>
    public override bool CanHandle(StringNames stringName)
    {
        return Strings.ContainsKey(stringName);
    }

    /// <inheritdoc/>
    public override string GetText(StringNames stringName, SupportedLangs language)
    {
        if (!Strings.ContainsKey(stringName)) throw new InvalidOperationException("StringName not found: " + stringName);
        return Strings[stringName].Value;
    }
}
