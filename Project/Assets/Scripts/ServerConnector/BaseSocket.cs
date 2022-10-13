using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;

public abstract class BaseSocket : ISocket
{
    protected Socket Socket;

    protected readonly int UserId;
    
    protected bool isDisposed { get; private set; }

    protected readonly List<ISocket.SubscribeDelegate> Subscribers;

    protected BaseSocket(int userId)
    {
        UserId = userId;
        Subscribers = new List<ISocket.SubscribeDelegate>();
    }

    public virtual async  Task Connect(IPAddress address, int port)
    {
        Socket.Connect(new IPEndPoint(address, port));
    }

    public abstract Task Run();

    public abstract ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message);

    public async ValueTask<IAsyncDisposable> SubscribeAsync(ISocket.SubscribeDelegate subscriber)
    {
        Subscribers.Add(subscriber);

        return new AnonymousDisposable(async () => Subscribers.Remove(subscriber));
    }

    public async ValueTask CloseAsync()
    {
        isDisposed = true;
        //if (Socket.Connected)
        //    Socket.Disconnect(false);
        Socket.Dispose();
    }
}