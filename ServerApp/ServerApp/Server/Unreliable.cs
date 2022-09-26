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
                    var rcvResult = await _socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult).ToArray();
                    var arguments = new MessageReceivedArguments(null, msgBytes);
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
            // todo fix address
            await _socket.SendToAsync(message.ToArray(), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));

            return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
    }
}