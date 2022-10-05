using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;

public class UnreliableSocket : ISocket
{
    private readonly Socket _socket;

    private readonly List<ISocket.SubscribeDelegate> _subscribers = new List<ISocket.SubscribeDelegate>();

    public UnreliableSocket()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public void Connect(IPAddress address, int port)
    {
        _socket.Connect(new IPEndPoint(address, port));
    }

    public async Task Run()
    {
        var rcvBytes = new byte[64000];
        var rcvBuffer = new ArraySegment<byte>(rcvBytes);

        while (true)
        {
            var rcvResult = await _socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

            var msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult).ToArray();
            var message = SocketMessage.Message(msgBytes);

            foreach (var subscriber in _subscribers)
                await subscriber.Invoke(message);
        }
    }

    public async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        await _socket.SendToAsync(message.ToArray(), SocketFlags.None, _socket.RemoteEndPoint);

        return new SocketSendResult();
    }

    public async ValueTask<IAsyncDisposable> SubscribeAsync(ISocket.SubscribeDelegate subscriber)
    {
        _subscribers.Add(subscriber);

        return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
    }

    public async ValueTask CloseAsync()
    {
        _socket.Disconnect(false);
        _socket.Dispose();
    }
}