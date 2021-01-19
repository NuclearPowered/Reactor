using Hazel;

namespace Reactor.Net
{
    public static class ModdedHandshakeS2C
    {
        public static void Serialize(MessageWriter writer, string serverBrand)
        {
            writer.StartMessage(byte.MaxValue);
            writer.Write(serverBrand);
            writer.EndMessage();
        }
    }
}
