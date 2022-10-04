using System;
using Il2CppSystem.Collections.Generic;
using Reactor.Localization.Utilities;

namespace Reactor.Localization.Providers;

public class HardCodedLocalizationProvider : LocalizationProvider
{
    internal static Dictionary<StringNames, CustomStringName> Strings = new();
    
    public override int Priority => HarmonyLib.Priority.Low;

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
