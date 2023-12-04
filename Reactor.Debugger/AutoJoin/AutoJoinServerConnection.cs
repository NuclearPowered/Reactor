using System;
using System.IO;
using System.IO.Pipes;
using Reactor.Debugger.AutoJoin.Messages;

namespace Reactor.Debugger.AutoJoin;

internal sealed class AutoJoinServerConnection : AutoJoinConnection
{
    public AutoJoinServerConnection(NamedPipeServerStream pipe) : base(pipe)
    {
    }

    protected override void Handle(BinaryReader reader, MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.RequestJoinGame:
            {
                Handle(RequestJoinGameMessage.Deserialize(reader));
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }

    private void Handle(in RequestJoinGameMessage message)
    {
        if (AmongUsClient.Instance.AmConnected)
        {
            Write(JoinGameMessage.From(AmongUsClient.Instance));
        }
    }
}
