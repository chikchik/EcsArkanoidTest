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
            var buffer = new ArraySegment<byte>(new byte[65000]);

            try
            {
                var anyEndPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    var res = await _socket.ReceiveMessageFromAsync(buffer, SocketFlags.None, anyEndPoint);

                    var received = new byte[res.ReceivedBytes];
                    Array.Copy(buffer.Array, buffer.Offset, received, 0, res.ReceivedBytes);

                    
                    var remoteEndPoint = res.RemoteEndPoint;

                    var id = BitConverter.ToInt32(received[..sizeof(int)]).ToString();
                    var data = received[sizeof(int)..];

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

                    var arguments = new MessageReceivedArguments(address, data);
                    var message = UnreliableChannelMessage.MessageReceived(arguments);
                    IUnreliableChannel.SubscribeDelegate[] copy;
                    lock (_subscribers)
                        copy = _subscribers.ToArray();

                    foreach (var subscriber in copy)
                        await subscriber.Invoke(message);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                await DisposeAsync();
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
                Console.WriteLine(e);

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