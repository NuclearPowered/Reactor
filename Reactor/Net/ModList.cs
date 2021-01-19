using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using Hazel;

namespace Reactor.Net
{
    public static class ModList
    {
        public static void Serialize(MessageWriter writer, ISet<Mod> mods)
        {
            writer.WritePacked(mods.Count);

            foreach (var mod in mods)
            {
                writer.Write(mod.Id);
                writer.Write(mod.Version);
                writer.Write((byte) mod.Side);
            }
        }

        public static ISet<Mod> GetCurrent()
        {
            return IL2CPPChainloader.Instance.Plugins
                .Select(plugin => new Mod(
                    plugin.Key,
                    plugin.Value.Metadata.Version.ToString(),
                    plugin.Value.Instance.GetType().GetCustomAttribute<ReactorPluginSideAttribute>()?.Side ?? PluginSide.Both
                ))
                .ToHashSet();
        }
    }
}
