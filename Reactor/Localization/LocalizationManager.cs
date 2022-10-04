using System.Collections.Generic;
using System.Linq;

namespace Reactor.Localization;

public static class LocalizationManager
{
    internal static List<LocalizationProvider> Providers = new();

    public static void Register(LocalizationProvider provider)
    {
        Providers.Add(provider);
    }

    public static void Unregister(LocalizationProvider provider)
    {
        Providers.Remove(provider);
    }
    
    public static void UnregisterAllByType<T>() where T : LocalizationProvider
    {
        Providers.RemoveAll(x => x is T);
    }

    internal static bool TryGetText(StringNames stringNames, out string text)
    {
        foreach (var provider in Providers.OrderByDescending(p => p.Priority))
        {
            if (provider.CanHandle(stringNames))
            {
                text = provider.GetText(stringNames);
                return true;
            }
        }

        text = string.Empty;
        return false;
    }
}
