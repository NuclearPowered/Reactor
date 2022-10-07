using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Patches;

namespace Reactor.Networking;

public static class ModList
{
    public static IReadOnlyCollection<Mod> Current { get; private set; } = null!;

    private static readonly Dictionary<string, Mod> _modById = new();
    private static readonly Dictionary<Type, Mod> _modByPluginType = new();

    public static Mod GetById(string id)
    {
        return _modById[id];
    }

    internal static Mod GetByPluginType(Type pluginType)
    {
        return _modByPluginType[pluginType];
    }

    private static readonly Dictionary<uint, Mod> _modByNetId = new();
    private static readonly Dictionary<Mod, uint> _netIdByMod = new();

    internal static Mod GetByNetId(uint netId)
    {
        return _modByNetId[netId];
    }

    internal static uint GetNetId(this Mod mod)
    {
        if (!mod.IsRequiredOnAllClients) throw new ArgumentException("Cannot get a net id for a mod without RequireOnAllClients flag", nameof(mod));
        return _netIdByMod[mod];
    }

    public static Mod ReadMod(this MessageReader reader)
    {
        var netId = reader.ReadPackedUInt32();
        if (netId > 0)
        {
            return GetByNetId(netId);
        }

        var id = reader.ReadString();
        return GetById(id);
    }

    public static void Write(this MessageWriter writer, Mod mod)
    {
        if (mod.IsRequiredOnAllClients)
        {
            writer.WritePacked(mod.GetNetId());
        }
        else
        {
            writer.WritePacked(0);
            writer.Write(mod.Id);
        }
    }

    private static void Refresh()
    {
        if (ReactorConnection.Instance != null) throw new InvalidOperationException("Can't refresh the mod list during a connection");

        Current = IL2CPPChainloader.Instance.Plugins.Values
            .Select(pluginInfo => GetById(pluginInfo.Metadata.GUID))
            .OrderByDescending(x => x.Id == ReactorPlugin.Id)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToHashSet();

        _modByNetId.Clear();
        _netIdByMod.Clear();

        uint netId = 1;
        foreach (var mod in Current)
        {
            if (!mod.IsRequiredOnAllClients) continue;

            _modByNetId[netId] = mod;
            _netIdByMod[mod] = netId;
            netId++;
        }

        var debug = new StringBuilder("Mod list:");
        foreach (var mod in Current)
        {
            debug.AppendLine();
            debug.Append(CultureInfo.InvariantCulture, $" - {mod.Id} version: {mod.Version}, flags: {mod.Flags}");
            if (mod.IsRequiredOnAllClients) debug.Append(CultureInfo.InvariantCulture, $", netId: {mod.GetNetId()}");
        }

        Debug(debug.ToString());
    }

    private static void OnPluginLoad(PluginInfo pluginInfo, BasePlugin plugin)
    {
        var pluginType = plugin.GetType();

        var mod = new Mod(
            pluginInfo.Metadata.GUID,
            pluginInfo.Metadata.Version.Clean(),
            ReactorModFlagsAttribute.GetModFlags(pluginType),
            pluginInfo.Metadata.Name
        );

        _modById[mod.Id] = mod;
        _modByPluginType[pluginType] = mod;
    }

    internal static void Initialize()
    {
        foreach (var existingPlugin in IL2CPPChainloader.Instance.Plugins.Values)
        {
            if (existingPlugin.Instance == null) continue;
            OnPluginLoad(existingPlugin, (BasePlugin) existingPlugin.Instance);
        }

        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, _, plugin) => OnPluginLoad(pluginInfo, plugin);

        IL2CPPChainloader.Instance.Finished += Refresh;
    }
}
