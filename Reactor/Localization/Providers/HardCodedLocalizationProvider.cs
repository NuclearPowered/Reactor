using System;
using Il2CppSystem.Collections.Generic;
using Reactor.Localization.Utilities;
using Reactor.Utilities;

namespace Reactor.Localization.Providers;

public sealed class HardCodedLocalizationProvider : LocalizationProvider
{
    internal static Dictionary<StringNames, CustomStringName> Strings = new();

    public override int Priority => ReactorPriority.Low;

    public override bool CanHandle(StringNames stringName)
    {
        return Strings.ContainsKey(stringName);
    }

    public override string GetText(StringNames stringName)
    {
        if (!Strings.ContainsKey(stringName)) throw new InvalidOperationException("StringName not found: " + stringName);
        return Strings[stringName].Value;
    }
}
