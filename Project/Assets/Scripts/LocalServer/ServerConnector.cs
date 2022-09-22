using System;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Models.V1;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using Gaming.Facade.Sockets;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    private SocketImpl _socket;

    private ISocket _containerSocket;

    public async void Start()
    {
        _socket = new SocketImpl();
        await _socket.SubscribeAsync(OnMessageReceive);

        var url = new Uri("https://dev.containers.xfin.net");
        GamingServices.V1.Configure(
            configuration => configuration.WithServerUrl(url)
                .WithAppDataPath(Application.persistentDataPath),
            Console.WriteLine);
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

    public async void ConnectToContainer()
    {
        var containerId = ContainerId.Parse("06f988be-52d8-461d-8283-03d280e2b1a5");
        await GamingServices.V1
            .ReliableConnectToContainer(containerId)
            .HandleResultAsync(
                async socket =>
                {
                    _containerSocket = socket;
                    await _containerSocket.SubscribeAsync(OnMessageReceive);
                },
                async () => Debug.Log("container not found"),
                async status => Debug.LogError($"invalid container status = {status.Type}"),
                async exception => Debug.LogError($"unexpected error ={exception}"));
    }

    public async void SendToContainer()
    {
        await _containerSocket.SendAsync(Encoding.Unicode.GetBytes("server container"));
    }

    private async ValueTask OnMessageReceive(SocketMessage message)
    {
        var data = message.GetMessage();
        if (data.HasValue)
            Debug.Log($"receive {Encoding.Unicode.GetString(data.Value.Array)}");
        else
            Debug.Log($"No data received");
    }
}