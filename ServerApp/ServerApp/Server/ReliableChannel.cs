using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Utils;
using XFlow.P2P;

namespace ServerApp.Server
{
    public class ReliableChannel : IReliableChannel
    {
        private bool _isDisposed;
        private readonly Socket _socket;
        private readonly List<IReliableChannel.SubscribeDelegate> _subscribers;
        private readonly List<ReliableUserAddress> _openedConnections = new List<ReliableUserAddress>();

        public ReliableChannel(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IReliableChannel.SubscribeDelegate>();
        }

        public async ValueTask DisposeAsync()
        {
            _socket.Dispose();
            
            _isDisposed = true;
            lock (_openedConnections)
            {
                foreach (var address in _openedConnections)
                {
                    address.Socket.Dispose();
                }
                _openedConnections.Clear();
            }

            lock(_subscribers)
                _subscribers.Clear();
        }


        private async Task ClientAsync(ReliableUserAddress address, int pos, List<byte> buffer, byte[] tempBuffer)
        {
            var socket = address.Socket;
            while (true)
            {
                await SocketUtils.ReceiveAsync(socket, pos, 4, buffer, tempBuffer);
                SocketUtils.GetSlice(buffer, ref pos, 4, ref tempBuffer);
                var size = BitConverter.ToInt32(tempBuffer);
                await SocketUtils.ReceiveAsync(socket, pos, size, buffer, tempBuffer);

                SocketUtils.GetSlice(buffer, ref pos, size, ref tempBuffer);
                var readOnly = new ReadOnlyMemory<byte>(tempBuffer, 0, size);
                var arguments = new MessageReceivedArguments(address, readOnly);
                await NotifySubscribersAsync(ReliableChannelMessage.MessageReceived(arguments));
                
                buffer.Clear();
                pos = 0;
            }
        }
        
        private async Task NewClientAsync(Socket socket)
        {
            var tempBuffer = new byte[32000];
            var buffer = new List<byte>();
            int pos = 0;
            await SocketUtils.ReceiveAsync(socket, pos, 4, buffer, tempBuffer);
            SocketUtils.GetSlice(buffer, ref pos, 4, ref tempBuffer);
        
            var id = BitConverter.ToInt32(tempBuffer);
            var clientAddress = new ReliableUserAddress(id.ToString(), $"{id}-tcp", socket);
        
            lock(_openedConnections)
                _openedConnections.Add(clientAddress);
            
            await NotifySubscribersAsync(ReliableChannelMessage.UserConnected(new UserConnectedArguments(clientAddress)));
            await ClientAsync(clientAddress, pos, buffer, tempBuffer);
            await UserDisconnectedAsync(clientAddress);
        }

        private async Task UserDisconnectedAsync(ReliableUserAddress address)
        {
            address.Socket.Dispose();
            address.Socket = null;
            lock (_openedConnections)
            {
                _openedConnections.Remove(address);
            }
            await NotifySubscribersAsync(ReliableChannelMessage.UserDisconnected(new UserDisconnectedArguments(address)));
        }

        public async void Start()
        {
            while (true)
            {
                var newSocket = await _socket.AcceptAsync();
                _ = Task.Run(async () => { await NewClientAsync(newSocket); });
            }
        }

        public async ValueTask<ReliableChannelSendResult> SendAsync(IUserAddress userAddress_,
            ReadOnlyMemory<byte> message)
        {
            if (!(userAddress_ is ReliableUserAddress address))
                return new ReliableChannelSendResult(ReliableChannelSendStatus.Unknown, $"invalid address");
            
            if (_isDisposed)
                return new ReliableChannelSendResult(ReliableChannelSendStatus.ChannelIsClosed, address.ToString());

            try
            {
                var socket = address.Socket;
                var header = BitConverter.GetBytes(message.Length);
                await SocketUtils.SendAsync(socket, header);
                await SocketUtils.SendAsync(socket, message);
                return new ReliableChannelSendResult(ReliableChannelSendStatus.Ok, "");
            }
            catch (Exception e)
            {
                await UserDisconnectedAsync(address);
                return new ReliableChannelSendResult(ReliableChannelSendStatus.Unknown,
                    $"{address}\n{e}");
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IReliableChannel.SubscribeDelegate subscriber)
        {
            lock(_subscribers)
                _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }

        private async Task NotifySubscribersAsync(ReliableChannelMessage message)
        {
            IReliableChannel.SubscribeDelegate[] copy;
            lock (_subscribers)
            {
                copy = _subscribers.ToArray();
            }
            for (var i = 0; i < copy.Length; i++)
                await copy[i].Invoke(message);
        }
    }
}