using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Reactor.Debugger.AutoJoin.Messages;

namespace Reactor.Debugger.AutoJoin;

internal abstract class AutoJoinConnection : IDisposable
{
    private readonly BinaryWriter _writer;

    protected AutoJoinConnection(PipeStream pipe)
    {
        _writer = new BinaryWriter(pipe);

        Task.Run(() =>
        {
            try
            {
                var reader = new BinaryReader(pipe);

                while (pipe.IsConnected)
                {
                    try
                    {
                        var messageType = (MessageType) reader.ReadByte();
                        Debug($"Received {messageType}");
                        Handle(reader, messageType);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                }

                pipe.Dispose();
                Disconnected?.Invoke();
            }
            catch (Exception e)
            {
                Error(e);
            }
        });
    }

    public event Action? Disconnected;

    protected abstract void Handle(BinaryReader reader, MessageType messageType);

    public void Write<T>(in T message) where T : IMessage<T>
    {
        Debug($"Writing {message}");
        _writer.Write(message);
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
