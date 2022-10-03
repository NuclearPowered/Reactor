using Hazel;

namespace Reactor.Networking.Serialization;

internal static class ReactorHeader
{
    private const ulong MAGIC = 0x72656163746f72; // "reactor" in ascii, 7 bytes

    public static int Size => sizeof(ulong);

    public static void Read(MessageReader reader, out ulong magic, out byte version)
    {
        var value = reader.ReadUInt64();
        magic = value >> 8;
        version = (byte) (value & 0xFF);
    }

    public static bool Read(MessageReader reader)
    {
        Read(reader, out var magic, out var version);
        return magic == MAGIC && version == (int) ReactorProtocolVersion.Latest;
    }

    public static void Write(MessageWriter writer)
    {
        var version = (byte) ReactorProtocolVersion.Latest;
        var value = (MAGIC << 8) | version;
        writer.Write(value);
    }
}
