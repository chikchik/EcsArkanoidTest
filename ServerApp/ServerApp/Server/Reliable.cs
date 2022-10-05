using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.P2P;

namespace ServerApp.Server
{
    public class Reliable : IReliableChannel
    {
        private readonly Socket _socket;

        private readonly List<IReliableChannel.SubscribeDelegate> _subscribers;

        private readonly Dictionary<IUserAddress, Socket> _connections;

        public Reliable(Socket socket)
        {
            _socket = socket;
            _connections = new Dictionary<IUserAddress, Socket>();
            _subscribers = new List<IReliableChannel.SubscribeDelegate>();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var kvp in _connections)
            {
                kvp.Value.Disconnect(false);
                kvp.Value.Dispose();
            }

            _socket.Dispose();
            _subscribers.Clear();
            _connections.Clear();
        }

        public async void Start()
        {
            while (true)
            {
                var newSocket = await _socket.AcceptAsync();

                var buffer = new ArraySegment<byte>(new byte[64000]);
                var received = await newSocket.ReceiveAsync(buffer, SocketFlags.None);

                if (received != sizeof(long) + sizeof(int))
                {
                    Console.WriteLine($"<12");
                    continue;
                }

                var id = BitConverter.ToInt32(buffer[sizeof(long)..]);
                Console.WriteLine($"new connection {id}");

                var address = new UserAddress(id.ToString());
                _connections.Add(address, newSocket);

                await SendMessage(ReliableChannelMessage.UserConnected(new UserConnectedArguments(address)));

                StartReceiving(newSocket, address);
            }
        }

        private async void StartReceiving(Socket socket, IUserAddress address)
        {
            try
            {
                const int packetSizeHeader = sizeof(long);

                var dataStream = new Queue<byte>();
                var rcvBuffer = new ArraySegment<byte>(new byte[64000]);

                var nextPacketSize = -1L;

                var messages = new List<byte[]>();

                while (true)
                {
                    var receivedLength = await socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

                    if (receivedLength == 0)
                    {
                        await SendMessage(
                            ReliableChannelMessage.UserDisconnected(new UserDisconnectedArguments(address)));
                        await SendMessage(ReliableChannelMessage.ChannelClosed(new ChannelClosedArguments()));

                        socket.Dispose();
                        _connections.Remove(address);

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
                        var arguments = new MessageReceivedArguments(address, message);
                        await SendMessage(ReliableChannelMessage.MessageReceived(arguments));
                    }

                    messages.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async ValueTask<ReliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            try
            {
                await _connections[userAddress].SendAsync(P2P.PackMessage(message.ToArray()), SocketFlags.None);

                return new ReliableChannelSendResult(ReliableChannelSendStatus.Ok, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IReliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }

        private async Task SendMessage(ReliableChannelMessage message)
        {
            foreach (var subscriber in _subscribers)
                await subscriber.Invoke(message);
        }
    }
}