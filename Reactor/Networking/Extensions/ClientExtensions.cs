using Hazel;
using InnerNet;

namespace Reactor.Networking.Extensions;

/// <summary>
/// Provides extension methods for <see cref="InnerNetClient"/>.
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Kicks a client with id equal to <paramref name="targetClientId"/> with a <paramref name="reason"/>.
    /// </summary>
    /// <param name="innerNetClient">The <see cref="InnerNetClient"/> to send the message from.</param>
    /// <param name="targetClientId">The target client's id.</param>
    /// <param name="reason">The kick reason.</param>
    public static void KickWithReason(this InnerNetClient innerNetClient, int targetClientId, string reason)
    {
        var writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(Tags.GameDataTo);
        writer.Write(innerNetClient.GameId);
        writer.WritePacked(targetClientId);
        {
            writer.StartMessage(byte.MaxValue);
            writer.Write((byte) ReactorGameDataFlag.KickWithReason);
            writer.Write(reason);
            writer.EndMessage();
        }
        writer.EndMessage();
        innerNetClient.SendOrDisconnect(writer);
        writer.Recycle();
    }

    /// <summary>
    /// Disconnects a local <see cref="InnerNetClient"/> with a custom reason.
    /// </summary>
    /// <param name="innerNetClient">The <see cref="InnerNetClient"/> to disconnect.</param>
    /// <param name="reason">The custom reason to disconnect with.</param>
    public static void DisconnectWithReason(this InnerNetClient innerNetClient, string reason)
    {
        innerNetClient.LastCustomDisconnect = reason;
        innerNetClient.HandleDisconnect(innerNetClient.LastDisconnectReason = DisconnectReasons.Custom, reason);
    }
}
