using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;

namespace Reactor.Networking
{
    public static class ModList
    {
        public static ISet<Mod> Current { get; private set; }

        internal static void Update()
        {
            var i = (uint) 0;

            Current = IL2CPPChainloader.Instance.Plugins.Values
                .OrderByDescending(x => x.Metadata.GUID == ReactorPlugin.Id)
                .Select(plugin => new Mod(
                    i++,
                    plugin.Metadata.GUID,
                    plugin.Metadata.Version.Clean(),
                    plugin.Instance.GetType().GetCustomAttribute<ReactorPluginSideAttribute>()?.Side ?? PluginSide.Both
                ))
                .ToHashSet();
        }
    }
}
