using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.P2P;

public class ReliableSocket : ISocket
{
    private readonly Socket _socket;

    private readonly List<ISocket.SubscribeDelegate> _subscribers = new List<ISocket.SubscribeDelegate>();

    public ReliableSocket()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect(IPAddress address, int port)
    {
        _socket.Connect(new IPEndPoint(address, port));
    }

    public async Task Run()
    {
        const int packetSizeHeader = sizeof(long);

        var dataStream = new Queue<byte>();
        var rcvBuffer = new ArraySegment<byte>(new byte[64000]);

        var nextPacketSize = -1L;

        var messages = new List<byte[]>();

        while (true)
        {
            var receivedLength = await _socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

            if (receivedLength == 0)
            {
                _socket.Dispose();

                break;
            }

            for (var i = rcvBuffer.Offset; i < receivedLength; i++)
                dataStream.Enqueue(rcvBuffer[i]);

            while (true)
            {
                if (nextPacketSize == -1)
                {
                    if (dataStream.Count < packetSizeHeader)
                        break;

                    var packetSizeBuffer = new byte[packetSizeHeader];
                    for (var i = 0; i < packetSizeHeader; i++)
                        packetSizeBuffer[i] = dataStream.Dequeue();
                    nextPacketSize = BitConverter.ToInt64(packetSizeBuffer);
                }

                // already read packet size (packetSizeHeader)
                if (nextPacketSize - packetSizeHeader > dataStream.Count)
                    break;

                var message = new byte[nextPacketSize];
                for (var i = 0; i < nextPacketSize - packetSizeHeader; i++)
                    message[i] = dataStream.Dequeue();
                messages.Add(message);

                nextPacketSize = -1;
            }

            foreach (var message in messages)
            {
                foreach (var subscriber in _subscribers)
                    await subscriber.Invoke(SocketMessage.Message(message));
            }

            messages.Clear();
        }
    }

    public async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        await _socket.SendAsync(P2P.PackMessage(message.ToArray()), SocketFlags.None);

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