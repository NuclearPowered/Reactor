using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;

namespace Reactor
{
    public static class PluginSingleton<T> where T : BasePlugin
    {
        private static T? _instance;

        public static T Instance => _instance ??= IL2CPPChainloader.Instance.Plugins.Values.Select(x => x.Instance).OfType<T>().Single();

        internal static void Initialize()
        {
            ChainloaderHooks.PluginLoad += plugin =>
            {
                typeof(PluginSingleton<>).MakeGenericType(plugin.GetType())!
                    .GetField(nameof(_instance), BindingFlags.Static | BindingFlags.NonPublic)!
                    .SetValue(null, plugin);
            };
        }
    }
}
