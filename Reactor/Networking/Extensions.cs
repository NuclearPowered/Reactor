using Hazel;
using InnerNet;

namespace Reactor.Networking;

public static class Extensions
{
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

    public static void DisconnectWithReason(this InnerNetClient innerNetClient, string reason)
    {
        innerNetClient.LastCustomDisconnect = reason;
        innerNetClient.HandleDisconnect(innerNetClient.LastDisconnectReason = DisconnectReasons.Custom, reason);
    }
}
