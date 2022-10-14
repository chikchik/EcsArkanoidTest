using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using UnityEngine;
using XFlow.Utils;

namespace ServerApp.Server
{
    public class UnreliableChannel : IUnreliableChannel
    {
        private readonly Socket _socket;
        private readonly List<IUnreliableChannel.SubscribeDelegate> _subscribers;
        private readonly Dictionary<EndPoint, UnreliableUserAddress> _addresses;
        
        private bool _isDisposed;

        public UnreliableChannel(Socket socket)
        {
            _socket = socket;
            _subscribers = new List<IUnreliableChannel.SubscribeDelegate>();
            _addresses = new Dictionary<EndPoint, UnreliableUserAddress>();
        }

        public async void Start()
        {
            var buffer = new byte[0xffff];
            
            var anyEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                SocketReceiveMessageFromResult res;
                try
                {
                    res = await _socket.ReceiveMessageFromAsync(buffer, SocketFlags.None, anyEndPoint);
                }
                catch (Exception e)
                {
                    continue;
                }
                
                var remoteEndPoint = res.RemoteEndPoint;

                var id = BitConverter.ToInt32(buffer).ToString();

                UnreliableUserAddress address;
                lock (_addresses)
                {
                    if (_addresses.TryGetValue(remoteEndPoint, out address))
                    {
                    
                    }
                    else
                    {
                        address = new UnreliableUserAddress(id, $"{id}-udp", remoteEndPoint);
                        _addresses.Add(remoteEndPoint, address);
                    }
                }

                
                var data = new ReadOnlyMemory<byte>(buffer, 4, res.ReceivedBytes - 4);
                var arguments = new MessageReceivedArguments(address, data);
                var message = UnreliableChannelMessage.MessageReceived(arguments);
                IUnreliableChannel.SubscribeDelegate[] copy;
                lock (_subscribers)
                    copy = _subscribers.ToArray();

                foreach (var subscriber in copy)
                    await subscriber.Invoke(message);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;
            _socket.Dispose();
            lock(_addresses)
                _addresses.Clear();
            lock (_subscribers)
                _subscribers.Clear();
        }

        public async ValueTask<UnreliableChannelSendResult> SendAsync(IUserAddress userAddress,
            ReadOnlyMemory<byte> message)
        {
            if (_isDisposed)
                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.ChannelIsClosed, null);

            if (!(userAddress is UnreliableUserAddress address))
                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, $"invalid address");

            
            try
            {
                await _socket.SendToAsync(message.ToArray(), SocketFlags.None, address.EndPoint);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Ok, null);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);

                return new UnreliableChannelSendResult(UnreliableChannelSendStatus.Unknown, e.ToString());
            }
        }

        public async ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            lock (_subscribers)
            {
                _subscribers.Add(subscriber);
            }

            return new AnonymousDisposable(async () =>
            {
                lock (_subscribers)
                {
                    _subscribers.Remove(subscriber);
                }
            });
        }
    }
}