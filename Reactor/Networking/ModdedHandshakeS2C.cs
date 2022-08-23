using Hazel;

namespace Reactor.Networking;

public static class ModdedHandshakeS2C
{
    public static void Serialize(MessageWriter writer, string serverName, string serverVersion, int pluginCount)
    {
        writer.StartMessage(byte.MaxValue);
        writer.Write((byte) ReactorMessageFlags.Handshake);
        writer.Write(serverName);
        writer.Write(serverVersion);
        writer.WritePacked(pluginCount);
        writer.EndMessage();
    }

    public static void Deserialize(MessageReader reader, out string serverName, out string serverVersion, out int pluginCount)
    {
        serverName = reader.ReadString();
        serverVersion = reader.ReadString();
        pluginCount = reader.ReadPackedInt32();
    }
}
