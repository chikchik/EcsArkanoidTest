using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using UnityEngine;

public class SocketImpl : ISocket
{
    private Socket _socket;

    private readonly List<ISocket.SubscribeDelegate> _subscribers = new List<ISocket.SubscribeDelegate>();

    public SocketImpl(SocketType type, ProtocolType protocolType)
    {
        _socket = new Socket(AddressFamily.InterNetwork, type, protocolType);
    }

    public void Connect(IPAddress address, int port)
    {
        _socket.Connect(new IPEndPoint(address, port));
    }

    public async Task Run()
    {
        try
        {
            var rcvBytes = new byte[64000];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            while (true)
            {
                var rcvResult = await _socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult).ToArray();
                var message = SocketMessage.Message(msgBytes);

                foreach (var subscriber in _subscribers)
                    await subscriber.Invoke(message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        try
        {
            if (_socket.ProtocolType == ProtocolType.Udp)
                await _socket.SendToAsync(message.ToArray(), SocketFlags.None, _socket.RemoteEndPoint);
            else
                await _socket.SendAsync(message, SocketFlags.None);

            return new SocketSendResult();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return new SocketSendResult();
    }

    public async ValueTask<IAsyncDisposable> SubscribeAsync(ISocket.SubscribeDelegate subscriber)
    {
        _subscribers.Add(subscriber);

        return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
    }

    public async ValueTask CloseAsync()
    {
        _socket.Disconnect(false);
        _socket.Dispose();
    }

    private class AnonymousDisposable : IAsyncDisposable
    {
        private readonly Func<Task> _callback;

        public AnonymousDisposable(Func<Task> callback)
        {
            _callback = callback;
        }

        public async ValueTask DisposeAsync()
        {
            await _callback();
        }
    }
}