using Hazel;

namespace Reactor.Networking.Messages;

internal static class ModdedHandshakeS2C
{
    public static void Serialize(MessageWriter writer, string serverName, string serverVersion, int pluginCount)
    {
        writer.Write(serverName);
        writer.Write(serverVersion);
        writer.WritePacked(pluginCount);
    }

    public static void Deserialize(MessageReader reader, out string serverName, out string serverVersion, out int pluginCount)
    {
        serverName = reader.ReadString();
        serverVersion = reader.ReadString();
        pluginCount = reader.ReadPackedInt32();
    }
}
