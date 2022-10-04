using System.Collections.Generic;
using System.Linq;

namespace Reactor.Localization;

public static class LocalizationManager
{
    internal static List<LocalizationProvider> Providers = new();

    public static void Register(LocalizationProvider provider)
    {
        if (!Providers.Contains(provider)) Providers.Add(provider);
    }

    public static void Unregister(LocalizationProvider provider)
    {
        Providers.Remove(provider);
    }
    
    public static void UnregisterAllByType<T>() where T : LocalizationProvider
    {
        Providers.RemoveAll(x => x is T);
    }

    internal static bool TryGetText(StringNames stringName, out string text)
    {
        foreach (var provider in Providers.OrderByDescending(p => p.Priority))
        {
            if (provider.CanHandle(stringName))
            {
                text = provider.GetText(stringName);
                return true;
            }
        }

        text = string.Empty;
        return false;
    }

    internal static bool TryGetStringName(SystemTypes systemType, out StringNames stringName)
    {
        foreach (var provider in Providers.OrderByDescending(p => p.Priority))
        {
            if (provider.CanHandle(systemType))
            {
                stringName = provider.GetStringName(systemType);
                return true;
            }
        }

        stringName = StringNames.NoTranslation;
        return false;
    }
    
    internal static bool TryGetStringName(TaskTypes taskTypes, out StringNames stringName)
    {
        foreach (var provider in Providers.OrderByDescending(p => p.Priority))
        {
            if (provider.CanHandle(taskTypes))
            {
                stringName = provider.GetStringName(taskTypes);
                return true;
            }
        }

        stringName = StringNames.NoTranslation;
        return false;
    }
}
