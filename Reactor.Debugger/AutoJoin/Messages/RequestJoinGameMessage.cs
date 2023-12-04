using System.IO;

namespace Reactor.Debugger.AutoJoin.Messages;

internal readonly record struct RequestJoinGameMessage : IMessage<RequestJoinGameMessage>
{
    public static MessageType Type => MessageType.RequestJoinGame;

    public void Serialize(BinaryWriter writer)
    {
    }

    public static RequestJoinGameMessage Deserialize(BinaryReader reader)
    {
        return default;
    }
}
