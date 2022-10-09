#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Reactor.Networking;

/// <summary>
/// Represents a Reactor.Protocol mod.
/// </summary>
public readonly struct Mod : IEquatable<Mod>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Mod"/> struct.
    /// </summary>
    /// <param name="id">The id of the mod.</param>
    /// <param name="version">The version of the mod.</param>
    /// <param name="flags">The flags of the mod.</param>
    /// <param name="name">The name of the mod.</param>
    public Mod(string id, string version, ModFlags flags, string? name = null)
    {
        Id = id;
        Version = version;
        Flags = flags;
        Name = name;
    }

    /// <summary>
    /// Gets the id of the mod.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the version of the mod.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the flags of the mod.
    /// </summary>
    public ModFlags Flags { get; }

    /// <summary>
    /// Gets the name of the mod.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets a value indicating whether the mod is required on all clients.
    /// </summary>
    public bool IsRequiredOnAllClients => (Flags & ModFlags.RequireOnAllClients) != 0;

    /// <inheritdoc />
    public bool Equals(Mod other)
    {
        return Id == other.Id && Version == other.Version;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Mod other && Equals(other);
    }

    /// <inheritdoc />
    public static bool operator ==(Mod left, Mod right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(Mod left, Mod right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version);
    }

    /// <inheritdoc />
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
