using HarmonyLib;
using InnerNet;

namespace Reactor.Networking.Patches;

/// <summary>
/// Provides information about the reactor protocol state of the current connection.
/// </summary>
public class ReactorConnection
{
    /// <summary>
    /// Gets the syncer.
    /// </summary>
    public Syncer? Syncer { get; internal set; }

    /// <summary>
    /// Gets the current instance of <see cref="ReactorConnection"/>.
    /// </summary>
    public static ReactorConnection? Instance { get; private set; }

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CoConnect), typeof(string))]
        [HarmonyPrefix]
        public static void CoConnect()
        {
            Debug("New ReactorConnection created");
            Instance = new ReactorConnection();
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
        [HarmonyPostfix]
        public static void DisconnectInternalPostfix()
        {
            Debug("ReactorConnection disconnected");
            Instance = null;
        }
    }
}

/// <summary>
/// Specifies who syncs the mod list and handles compatibility.
/// </summary>
public enum Syncer
{
    /// <summary>
    /// A custom region server.
    /// </summary>
    Server,

    /// <summary>
    /// The host of the game.
    /// </summary>
    Host,
}
