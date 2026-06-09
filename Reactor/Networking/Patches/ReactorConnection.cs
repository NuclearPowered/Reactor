using System.Reflection;
using HarmonyLib;
using InnerNet;
using Reactor.Utilities;

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

    internal string? LastKickReason { get; set; }

    /// <summary>
    /// Gets the current instance of <see cref="ReactorConnection"/>.
    /// </summary>
    public static ReactorConnection? Instance { get; private set; }

    // CoConnect(string) was inlined, so we patch the MoveNext method instead.
    [HarmonyPatch]
    internal static class CoConnectPatch
    {
        public static MethodBase TargetMethod()
        {
            return Il2CppStateMachineWrapper<InnerNetClient>.GetStateMachineMoveNext(nameof(InnerNetClient.CoConnect))!;
        }

        public static void Prefix()
        {
            if (Instance == null)
            {
                Debug("New ReactorConnection created");
                Instance = new ReactorConnection();
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
    internal static class InnerNetClientDisconnectPatch
    {
        public static void Postfix()
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
