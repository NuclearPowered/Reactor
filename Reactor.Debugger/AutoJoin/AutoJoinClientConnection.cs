using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using InnerNet;
using Reactor.Debugger.AutoJoin.Messages;
using Reactor.Utilities;

namespace Reactor.Debugger.AutoJoin;

internal sealed class AutoJoinClientConnection : AutoJoinConnection
{
    public const string PipeName = "Reactor.Debugger.AutoJoin";

    private AutoJoinClientConnection(NamedPipeClientStream pipe) : base(pipe)
    {
    }

    protected override void Handle(BinaryReader reader, MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.JoinGame:
            {
                Handle(JoinGameMessage.Deserialize(reader));
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }

    private static void Handle(in JoinGameMessage message)
    {
        var (address, port, gameCode) = message;

        Dispatcher.Instance.Enqueue(() =>
        {
            var gameCodeText = gameCode == InnerNetServer.LocalGameId ? "<local>" : GameCode.IntToGameName(gameCode);
            Info($"Joining {gameCodeText} on {address}:{port}");

            if (gameCode == InnerNetServer.LocalGameId)
            {
                AmongUsClient.Instance.NetworkMode = NetworkModes.LocalGame;
                AmongUsClient.Instance.GameId = gameCode;
                AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoConnectToGameServer(MatchMakerModes.Client, address, port, null));
            }
            else
            {
                AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameDirect(gameCode, address, port, null));
            }
        });
    }

    public void SendRequestJoin()
    {
        Write(default(RequestJoinGameMessage));
    }

    public static bool TryConnect([NotNullWhen(true)] out AutoJoinClientConnection? client)
    {
        var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        try
        {
            pipeClient.Connect(100);

            client = new AutoJoinClientConnection(pipeClient);
            return true;
        }
        catch (TimeoutException)
        {
            pipeClient.Dispose();

            client = null;
            return false;
        }
    }
}
