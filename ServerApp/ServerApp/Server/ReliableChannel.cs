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
using XFlow.P2P;

namespace ServerApp.Server
{
    public class ReliableChannel : IReliableChannel
    {
        private readonly Socket _socket;
        private bool _isDisposed;

        private readonly object _locker = new object();

        private readonly List<IReliableChannel.SubscribeDelegate> _subscribers;

        private readonly Dictionary<IUserAddress, bool> _disposedConnections;
        private readonly List<ReliableUserAddress> _openedConnections = new List<ReliableUserAddress>();

        public ReliableChannel(Socket socket)
        {
            _socket = socket;
            _disposedConnections = new Dictionary<IUserAddress, bool>();
            _subscribers = new List<IReliableChannel.SubscribeDelegate>();
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;
            lock (_openedConnections)
            {
                foreach (var address in _openedConnections)
                {
                    address.Socket.Dispose();
                }
                _openedConnections.Clear();
            }

            /*
            foreach (var kvp in _connections)
            {
                kvp.Value.Disconnect(false);
                kvp.Value.Dispose();
            }*/

            _socket.Dispose();
            _subscribers.Clear();
            //_connections.Clear();
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
                await NotifySubscribers(ReliableChannelMessage.MessageReceived(arguments));
                
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
        
            await NotifySubscribers(ReliableChannelMessage.UserConnected(new UserConnectedArguments(clientAddress)));
            await ClientAsync(clientAddress, pos, buffer, tempBuffer);
            await UserDisconnected(clientAddress);
        }

        private async Task UserDisconnected(ReliableUserAddress address)
        {
            address.Socket.Dispose();
            address.Socket = null;
            lock (_openedConnections)
            {
                _openedConnections.Remove(address);
            }
            await NotifySubscribers(ReliableChannelMessage.UserDisconnected(new UserDisconnectedArguments(address)));
        }

        public async void Start()
        {
            while (true)
            {
                try
                {
                    var newSocket = await _socket.AcceptAsync();
                    _ = Task.Run(async () => { await NewClientAsync(newSocket); });
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    //throw;
                }
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
                await UserDisconnected(address);
                return new ReliableChannelSendResult(ReliableChannelSendStatus.Unknown,
                    $"{address}\n{e}");
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IReliableChannel.SubscribeDelegate subscriber)
        {
            _subscribers.Add(subscriber);

            return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
        }

        private async Task NotifySubscribers(ReliableChannelMessage message)
        {
            for (var i = 0; i < _subscribers.Count; i++)
                await _subscribers[i].Invoke(message);
        }
    }
}