using System.IO;

namespace Reactor.Debugger.AutoJoin.Messages;

internal static class Extensions
{
    public static void Write<T>(this BinaryWriter writer, in T message) where T : IMessage<T>
    {
        writer.Write((byte) T.Type);
        message.Serialize(writer);
    }
}
