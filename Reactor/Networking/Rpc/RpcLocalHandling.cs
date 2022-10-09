namespace Reactor.Networking.Rpc;

/// <summary>
/// Specifies how the rpc should be handled locally.
/// </summary>
public enum RpcLocalHandling
{
    /// <summary>
    /// Don't do anything.
    /// </summary>
    None,

    /// <summary>
    /// Invoke Handle before sending.
    /// </summary>
    Before,

    /// <summary>
    /// Invoke Handle after sending.
    /// </summary>
    After,
}
