using System;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;

namespace Reactor;

public static class PluginSingleton<T> where T : BasePlugin
{
    private static T? _instance;

    public static T Instance
    {
        get => _instance ??= IL2CPPChainloader.Instance.Plugins.Values.Select(x => x.Instance).OfType<T>().Single();
        set
        {
            if (_instance == value) return;
            if (_instance != null) throw new InvalidOperationException($"Instance for {typeof(T)} is already set");
            _instance = value;
        }
    }

    internal static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, _, plugin) =>
        {
            typeof(PluginSingleton<>).MakeGenericType(plugin.GetType())
                .GetField(nameof(_instance), BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, plugin);
        };
    }
}
