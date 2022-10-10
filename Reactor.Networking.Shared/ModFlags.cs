using System;

namespace Reactor.Networking;

/// <summary>
/// Represents flags of the mod.
/// </summary>
[Flags]
public enum ModFlags : ushort
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// Requires all clients in a lobby to have the mod.
    /// </summary>
    RequireOnAllClients = 1 << 0,

    /// <summary>
    /// Requires the server to have a plugin that handles the mod.
    /// </summary>
    RequireOnServer = 1 << 1,

    /// <summary>
    /// Requires the host of the lobby to have the mod.
    /// </summary>
    RequireOnHost = 1 << 2,
}
