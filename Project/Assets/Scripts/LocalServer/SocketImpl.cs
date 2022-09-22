using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.Facade.Sockets;
using UnityEngine;

public class SocketImpl : ISocket
{
    private Socket _socket;

    private readonly List<ISocket.SubscribeDelegate> _subscribers = new List<ISocket.SubscribeDelegate>();

    public SocketImpl()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect()
    {
        _socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12121));
        Debug.Log($"Connected = {_socket.Connected}");
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

    public async Task<SocketSendResult> SendAsync(ArraySegment<byte> message)
    {
        try
        {
            var data = Encoding.Unicode.GetBytes("123123");
            Debug.Log($"Send = {data.Length}");
            var res = await _socket.SendAsync(message, SocketFlags.None);

            return new SocketSendResult();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return new SocketSendResult();
    }

    public async Task<IAsyncDisposable> SubscribeAsync(ISocket.SubscribeDelegate subscriber)
    {
        _subscribers.Add(subscriber);

        return new AnonymousDisposable(async () => _subscribers.Remove(subscriber));
    }

    public async Task CloseAsync()
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