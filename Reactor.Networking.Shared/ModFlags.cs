using System;

namespace Reactor.Networking;

[Flags]
public enum ModFlags : ushort
{
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
