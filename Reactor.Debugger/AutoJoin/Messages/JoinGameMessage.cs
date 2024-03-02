using System.IO;
using InnerNet;

namespace Reactor.Debugger.AutoJoin.Messages;

internal readonly record struct JoinGameMessage(
    string Address,
    ushort Port,
    int GameCode
) : IMessage<JoinGameMessage>
{
    public static MessageType Type => MessageType.JoinGame;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Address);
        writer.Write(Port);
        writer.Write(GameCode);
    }

    public static JoinGameMessage Deserialize(BinaryReader reader)
    {
        return new JoinGameMessage
        {
            Address = reader.ReadString(),
            Port = reader.ReadUInt16(),
            GameCode = reader.ReadInt32(),
        };
    }

    public static JoinGameMessage From(InnerNetClient innerNetClient)
    {
        return new JoinGameMessage
        {
            Address = innerNetClient.networkAddress,
            Port = (ushort) innerNetClient.networkPort,
            GameCode = innerNetClient.GameId,
        };
    }
}
