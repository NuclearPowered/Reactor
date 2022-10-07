#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Reactor.Networking;

public readonly struct Mod : IEquatable<Mod>
{
    public Mod(string id, string version, ModFlags flags, string? name = null)
    {
        Id = id;
        Version = version;
        Flags = flags;
        Name = name;
    }

    public string Id { get; }
    public string Version { get; }
    public ModFlags Flags { get; }
    public string? Name { get; }

    public bool IsRequiredOnAllClients => (Flags & ModFlags.RequireOnAllClients) != 0;

    public bool Equals(Mod other)
    {
        return Id == other.Id && Version == other.Version;
    }

    public override bool Equals(object? obj)
    {
        return obj is Mod other && Equals(other);
    }

    public static bool operator ==(Mod left, Mod right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Mod left, Mod right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version);
    }

    public override string ToString()
    {
        return $"{Name ?? Id} ({Version})";
    }

    internal static bool Validate(IReadOnlyCollection<Mod> clientMods, IReadOnlyCollection<Mod> hostMods, [NotNullWhen(false)] out string? reason)
    {
        var clientMissing = hostMods.Where(mod => mod.IsRequiredOnAllClients && !clientMods.Contains(mod)).ToArray();
        var hostMissing = clientMods.Where(mod => mod.IsRequiredOnAllClients && !hostMods.Contains(mod)).ToArray();

        if (clientMissing.Any() || hostMissing.Any())
        {
            var message = new StringBuilder();

            if (clientMissing.Any())
            {
                message.Append("You are missing: ");
                message.AppendJoin(", ", clientMissing.Select(x => x.ToString()));
                message.AppendLine();
            }

            if (hostMissing.Any())
            {
                message.Append("Host is missing: ");
                message.AppendJoin(", ", hostMissing.Select(x => x.ToString()));
                message.AppendLine();
            }

            reason = message.ToString();
            return false;
        }

        reason = null;
        return true;
    }
}
