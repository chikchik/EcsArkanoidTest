using System;
using System.Text;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    private static SocketImpl _socket;

    public async void Start()
    {
        _socket = new SocketImpl();
        await _socket.SubscribeAsync(async message =>
        {
            var data = message.GetMessage();
            if (data.HasValue)
                Debug.Log($"receive {Encoding.Unicode.GetString(data.Value.Array)}");
            else
                Debug.Log($"No data received");
        });
    }

    public void Connect()
    {
        _socket.Connect();
        _socket.Run();
    }

    public async void Send()
    {
        await _socket.SendAsync(new ArraySegment<byte>(Encoding.Unicode.GetBytes("message")));
    }
}