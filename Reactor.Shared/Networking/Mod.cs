#nullable enable

using System;

namespace Reactor.Networking;

public class Mod
{
    public Mod(uint netId, string id, string version, PluginSide side)
    {
        NetId = netId;
        Id = id;
        Version = version;
        Side = side;
    }

    public uint NetId { get; }
    public string Id { get; }
    public string Version { get; }
    public PluginSide Side { get; }

    protected bool Equals(Mod other)
    {
        return Id == other.Id && Version == other.Version;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Mod) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version);
    }
}
