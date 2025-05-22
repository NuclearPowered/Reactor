using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using InnerNet;
using Reactor.Debugger.AutoJoin.Messages;

namespace Reactor.Debugger.AutoJoin;

internal sealed class AutoJoinConnectionListener : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Mutex _mutex;

    private AutoJoinConnectionListener(Mutex mutex)
    {
        _mutex = mutex;

        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Info("Listening for connections");

                    var pipeServer = new NamedPipeServerStream(
                        AutoJoinClientConnection.PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous
                    );

                    try
                    {
                        await pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        pipeServer.Dispose();
                        break;
                    }

                    Info("Client connected");

                    var client = new AutoJoinServerConnection(pipeServer);
                    Clients.Add(client);
                    client.Disconnected += () =>
                    {
                        Clients.Remove(client);
                        client.Dispose();
                    };
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }

            Info("Stopped");
            Stopped?.Invoke();
        });
    }

    public event Action? Stopped;

    public List<AutoJoinServerConnection> Clients { get; } = new();

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        foreach (var client in Clients)
        {
            client.Dispose();
        }

        Clients.Clear();

        _mutex.Dispose();
    }

    public void SendJoinMe(InnerNetClient innerNetClient)
    {
        var joinGameMessage = JoinGameMessage.From(innerNetClient);

        foreach (var client in Clients)
        {
            client.Write(joinGameMessage);
        }
    }

    public static bool TryStart([NotNullWhen(true)] out AutoJoinConnectionListener? connectionListener)
    {
        var mutex = new Mutex(false, $@"Global\{AutoJoinClientConnection.PipeName}");

        if (mutex.WaitOne(100))
        {
            connectionListener = new AutoJoinConnectionListener(mutex);
            return true;
        }

        mutex.Dispose();

        connectionListener = null;
        return false;
    }
}
