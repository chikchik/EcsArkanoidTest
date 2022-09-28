using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class Unreliable : IUnreliableChannel
    {
        private readonly Socket _socket;

        private readonly List<IUnreliableChannel.SubscribeDelegate> _subscribers;

        public Unreliable(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IUnreliableChannel.SubscribeDelegate>();
        }

        public async void Start()
        {
            var rcvBytes = new byte[1024 * 32];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            try
            {
                while (true)
                {
                    var res = await _socket.ReceiveMessageFromAsync(rcvBuffer, SocketFlags.None,
                        new IPEndPoint(IPAddress.Any, 0));

                    var address = new UserAddress
                    {
                        ConnectionId = res.RemoteEndPoint.ToString()
                    };
                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(res.ReceivedBytes).ToArray();
                    var arguments = new MessageReceivedArguments(address, msgBytes);
                    var message = UnreliableChannelMessage.MessageReceived(arguments);

                    foreach (var subscriber in _subscribers)
                        await subscriber.Invoke(message);
                }
            }
            catch (Exception e)
            {
                throw e;
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
            await _socket.SendToAsync(message.ToArray(), SocketFlags.None, IPEndPoint.Parse(userAddress.ConnectionId));

            return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
        
        private class UserAddress : IUserAddress
        {
            public bool Equals(IUserAddress other)
            {
                return other.ConnectionId == ConnectionId;
            }

            public string UserId { get; }
            public string ConnectionId { get; set; }
        }
    }
}