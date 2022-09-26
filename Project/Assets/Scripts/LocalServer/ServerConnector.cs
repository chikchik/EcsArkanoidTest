using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Models.V1;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using Gaming.Facade.Sockets;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    private SocketImpl _tcpSocket;
    private SocketImpl _udpSocket;

    private ISocket _containerTCPSocket;
    private ISocket _containerUDPSocket;

    public async void Start()
    {
        _tcpSocket = new SocketImpl(SocketType.Stream, ProtocolType.Tcp);
        await _tcpSocket.SubscribeAsync(OnMessageReceive);
        _udpSocket = new SocketImpl(SocketType.Dgram, ProtocolType.Udp);
        await _udpSocket.SubscribeAsync(OnMessageReceive);

        var url = new Uri("https://dev.containers.xfin.net");
        GamingServices.V1.Configure(
            configuration => configuration.WithServerUrl(url)
                .WithAppDataPath(Application.persistentDataPath),
            Console.WriteLine);
    }

    public void Connect()
    {
        _tcpSocket.Connect(IPAddress.Parse("127.0.0.1"), 12121);
        _tcpSocket.Run();
        _udpSocket.Connect(IPAddress.Parse("127.0.0.1"), 12345);
        _udpSocket.Run();
    }

    public async void Send()
    {
        await _tcpSocket.SendAsync(new ArraySegment<byte>(Encoding.Unicode.GetBytes("message tcp")));
    }

    public async void SendUDP()
    {
        await _udpSocket.SendAsync(new ArraySegment<byte>(Encoding.Unicode.GetBytes("message udp")));
    }

    public async void ConnectToContainer()
    {
        var containerId = ContainerId.Parse("06f988be-52d8-461d-8283-03d280e2b1a5");
        await GamingServices.V1
            .ReliableConnectToContainer(containerId)
            .HandleResultAsync(
                async socket =>
                {
                    _containerTCPSocket = socket;
                    await _containerTCPSocket.SubscribeAsync(OnMessageReceive);
                },
                async () => Debug.Log("container not found"),
                async status => Debug.LogError($"invalid container status = {status.Type}"),
                async exception => Debug.LogError($"unexpected error ={exception}"));

        await GamingServices.V1
            .UnreliableConnectToContainer(containerId)
            .HandleResultAsync(
                async socket =>
                {
                    _containerUDPSocket = socket;
                    await _containerTCPSocket.SubscribeAsync(OnMessageReceive);
                },
                async () => Debug.Log("container not found"),
                async status => Debug.LogError($"invalid container status = {status.Type}"),
                async exception => Debug.LogError($"unexpected error ={exception}"));
    }

    public async void SendToContainer()
    {
        await _containerTCPSocket.SendAsync(Encoding.Unicode.GetBytes("server container tcp"));
    }

    public async void SendToContainerUDP()
    {
        await _containerUDPSocket.SendAsync(Encoding.Unicode.GetBytes("server container udp"));
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