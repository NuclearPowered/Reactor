using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP;

namespace Reactor.Networking;

public static class ModList
{
    private static Dictionary<string, Mod>? _mapById;
    private static Dictionary<uint, Mod>? _mapByNetId;

    public static ISet<Mod>? Current { get; private set; }

    public static Mod GetById(string id)
    {
        return _mapById![id];
    }

    public static Mod GetByNetId(uint netId)
    {
        return _mapByNetId![netId];
    }

    internal static ISet<Mod> Update()
    {
        var i = (uint) 0;

        Current = IL2CPPChainloader.Instance.Plugins.Values
            .OrderByDescending(x => x.Metadata.GUID == ReactorPlugin.Id)
            .Select(plugin => new Mod(
                i++,
                plugin.Metadata.GUID,
                plugin.Metadata.Version.Clean(),
                ReactorPluginSideAttribute.GetPluginSide(plugin.Instance.GetType())
            ))
            .ToHashSet();

        _mapById = Current.ToDictionary(mod => mod.Id, mod => mod);
        _mapByNetId = Current.ToDictionary(mod => mod.NetId, mod => mod);

        return Current;
    }
}
