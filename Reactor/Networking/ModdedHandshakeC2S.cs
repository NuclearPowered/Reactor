using Hazel;

namespace Reactor.Networking
{
    public static class ModdedHandshakeC2S
    {
        public static void Serialize(MessageWriter writer, int modCount)
        {
            writer.Write((byte) ReactorProtocolVersion.Initial);
            writer.WritePacked(modCount);
        }
    }
}
