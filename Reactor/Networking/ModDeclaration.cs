using Hazel;

namespace Reactor.Networking;

public static class ModDeclaration
{
    public static void Serialize(MessageWriter writer, Mod mod)
    {
        writer.StartMessage(byte.MaxValue);
        writer.Write((byte) ReactorMessageFlags.ModDeclaration);
        writer.WritePacked(mod.NetId);
        writer.Write(mod.Id);
        writer.Write(mod.Version);
        writer.Write((byte) mod.Side);
    }
}
