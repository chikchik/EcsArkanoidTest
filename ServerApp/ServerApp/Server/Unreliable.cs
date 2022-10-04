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
            var rcvBytes = new byte[64000];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    var res = await _socket.ReceiveMessageFromAsync(rcvBuffer, SocketFlags.None, endPoint);

                    byte[] msgBytes = new byte[res.ReceivedBytes];
                    Array.Copy(rcvBuffer.Array, rcvBuffer.Offset, msgBytes, 0, res.ReceivedBytes);

                    var arguments = new MessageReceivedArguments(null, msgBytes);
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
            await _socket.SendToAsync(message.ToArray(), SocketFlags.None, IPEndPoint.Parse(userAddress.ConnectionId));

            return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
    }
}