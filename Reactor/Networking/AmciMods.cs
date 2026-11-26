#pragma warning disable CA5350

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Reactor.Networking.Attributes;

namespace Reactor.Networking;

/// <summary>
/// Tracks the AMCI mod registrations of the loaded plugins and drives the vanilla
/// <see cref="CurrentModRegistration"/> integration.
/// </summary>
public static class AmciMods
{
    private static readonly Dictionary<string, Guid> _guidByModId = new();

    /// <summary>
    /// Gets the loaded mod ids and their registered AMCI GUIDs.
    /// </summary>
    public static IReadOnlyDictionary<string, Guid> Registered => _guidByModId;

    /// <summary>
    /// Gets the calculated active AMCI GUID (Single mod GUID or deterministic Composite GUID for multiple mods),
    /// or null when no loaded mod registered one.
    /// </summary>
    public static Guid? Primary { get; private set; }

    /// <summary>
    /// Gets a value indicating whether AMCI is currently active: at least one mod GUID is registered.
    /// </summary>
    public static bool IsEnabled => Primary != null;

    /// <summary>
    /// Applies the current AMCI state to the vanilla client.
    /// </summary>
    public static void Apply()
    {
        var primary = Primary;
        CurrentModRegistration.ModRegistrationGuidString = IsEnabled && primary != null ? primary.Value.ToString() : string.Empty;
    }

    internal static void Initialize()
    {
        foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
        {
            if (pluginInfo.Instance != null)
            {
                Register(pluginInfo, (BasePlugin) pluginInfo.Instance);
            }
        }

        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, _, plugin) => Register(pluginInfo, plugin);

        IL2CPPChainloader.Instance.Finished += () =>
        {
            foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
            {
                if (pluginInfo.Instance != null)
                {
                    Register(pluginInfo, (BasePlugin) pluginInfo.Instance);
                }
            }

            RefreshPrimary();
            Apply();
            Info($"AMCI initialized: registered={_guidByModId.Count} activeGuid={Primary} CurrentModRegistration={CurrentModRegistration.ModRegistrationGuidString}");
        };
    }

    private static void Register(PluginInfo pluginInfo, BasePlugin plugin)
    {
        if (plugin == null)
        {
            return;
        }

        var pluginType = plugin.GetType();
        if (AmciIgnoreAttribute.IsIgnored(pluginType))
        {
            return;
        }

        var guid = AmciModGuidAttribute.GetGuid(pluginType);
        if (guid.HasValue)
        {
            _guidByModId[pluginInfo.Metadata.GUID] = guid.Value;
        }
    }

    private static void RefreshPrimary()
    {
        if (_guidByModId.Count == 0)
        {
            Primary = null;
        }
        else if (_guidByModId.Count == 1)
        {
            Primary = _guidByModId.Values.First();
        }
        else
        {
            Primary = ComputeCompositeGuid(_guidByModId.Values);
        }
    }

    private static Guid ComputeCompositeGuid(IEnumerable<Guid> guids)
    {
        var sortedGuids = guids
            .Distinct()
            .OrderBy(g => g.ToString(), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (sortedGuids.Length == 1)
        {
            return sortedGuids[0];
        }

        var buffer = new byte[sortedGuids.Length * 16];
        for (var i = 0; i < sortedGuids.Length; i++)
        {
            var bytes = sortedGuids[i].ToByteArray();
            Buffer.BlockCopy(bytes, 0, buffer, i * 16, 16);
        }

        var hash = SHA1.HashData(buffer);
        var guidBytes = new byte[16];
        Array.Copy(hash, 0, guidBytes, 0, 16);

        // Conform to RFC 4122 version 4
        guidBytes[6] = (byte) ((guidBytes[6] & 0x0F) | 0x40); // version 4
        guidBytes[8] = (byte) ((guidBytes[8] & 0x3F) | 0x80); // variant RFC 4122

        return new Guid(guidBytes);
    }
}
