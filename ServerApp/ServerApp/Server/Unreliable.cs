using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.Utils;

namespace ServerApp.Server
{
    public class Unreliable : IUnreliableChannel
    {
        private readonly Socket _socket;

        private readonly List<IUnreliableChannel.SubscribeDelegate> _subscribers;

        private readonly Dictionary<IUserAddress, EndPoint> _connections;

        public Unreliable(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IUnreliableChannel.SubscribeDelegate>();
            _connections = new Dictionary<IUserAddress, EndPoint>();
        }

        public async void Start()
        {
            var rcvBytes = new byte[64000];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            var reader = new HGlobalReader();
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    var res = await _socket.ReceiveMessageFromAsync(rcvBuffer, SocketFlags.None, endPoint);

                    byte[] msgBytes = new byte[res.ReceivedBytes];
                    Array.Copy(rcvBuffer.Array, rcvBuffer.Offset, msgBytes, 0, res.ReceivedBytes);

                    reader.Init(msgBytes);
                    var id = reader.ReadInt32().ToString();
                    var data = msgBytes[reader.GetPosition()..];
                    Console.WriteLine($"new udp message id={id}, sizeLeft ={data.Length}");

                    var address = _connections.Keys.FirstOrDefault(address => address.UserId == id);
                    if (address == null)
                    {
                        address = new UserAddress(id);
                        _connections.Add(address, res.RemoteEndPoint);
                    }

                    var arguments = new MessageReceivedArguments(address, data);
                    var message = UnreliableChannelMessage.MessageReceived(arguments);

                    foreach (var subscriber in _subscribers)
                        await subscriber.Invoke(message);
                }
            }
            finally
            {
                DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _socket.Dispose();
            _subscribers.Clear();
        }

        public async ValueTask<UnreliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            if (!_connections.ContainsKey(userAddress))
                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, $"Address not found");

            try
            {
                await _socket.SendToAsync(message.ToArray(), SocketFlags.None, _connections[userAddress]);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, e.ToString());
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
    }
}