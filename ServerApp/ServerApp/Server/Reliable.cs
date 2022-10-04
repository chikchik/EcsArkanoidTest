using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class Reliable : IReliableChannel
    {
        private readonly Socket _socket;
        private Socket _listener;

        private readonly List<IReliableChannel.SubscribeDelegate> _subscribers;

        public Reliable(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IReliableChannel.SubscribeDelegate>();
        }

        public async ValueTask DisposeAsync()
        {
            _listener.Dispose();
            _socket.Dispose();
            _subscribers.Clear();
        }

        public async void Start()
        {
            var rcvBytes = new byte[64000];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            _listener = await _socket.AcceptAsync();
            while (true)
            {
                var rcvResult = await _listener.ReceiveAsync(rcvBuffer, SocketFlags.None);

                if (rcvResult == 0)
                    continue;

                byte[] msgBytes = new byte[rcvResult];
                Array.Copy(rcvBuffer.Array, rcvBuffer.Offset, msgBytes, 0, rcvResult);

                var arguments = new MessageReceivedArguments(null, msgBytes);
                var message = ReliableChannelMessage.MessageReceived(arguments);

                foreach (var subscriber in _subscribers)
                    await subscriber.Invoke(message);
            }
        }

        public async ValueTask<ReliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            var token = new CancellationToken();
            await _listener.SendAsync(message, SocketFlags.None, token);

            return new ReliableChannelSendResult(ReliableChannelSendStatus.Ok, null);
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IReliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }
    }
}