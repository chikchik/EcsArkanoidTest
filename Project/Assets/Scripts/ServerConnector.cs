using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using Gaming.ContainerManager.Models.V1;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using UnityEngine;
using XFlow.Net.Client;

namespace Game
{
    public class ServerConnector : IServerConnector
    {
        private readonly bool _connectToLocalServer;

        private readonly ContainerId _containerId;
        private readonly int _tcpPort;
        private readonly int _udpPort;

        public ServerConnector(string containerId)
        {
            _connectToLocalServer = false;
            _containerId = ContainerId.Parse(containerId);

            var url = new Uri("https://dev.containers.xfin.net");
            GamingServices.V1.Configure(
                configuration => configuration.WithServerUrl(url).WithAppDataPath(Application.persistentDataPath),
                Debug.Log);
        }

        public ServerConnector(int tcpPort, int udpPort)
        {
            _connectToLocalServer = true;
            _tcpPort = tcpPort;
            _udpPort = udpPort;
        }

        public async Task<ISocket> GetReliableConnection()
        {
            ISocket channel = null;
            if (_connectToLocalServer)
            {
                var socket = new SocketImpl(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(IPAddress.Parse("127.0.0.1"), _tcpPort);
                socket.Run();

                channel = socket;
            }
            else
            {
                await GamingServices.V1
                    .ReliableConnectToContainer(_containerId)
                    .HandleResultAsync(
                        async socket => { channel = socket; },
                        async () => Debug.Log("container not found"),
                        async status => Debug.LogError($"invalid container status = {status.Type}"),
                        async exception => Debug.LogError($"unexpected error = {exception}"));
            }

            return channel;
        }

        public async Task<ISocket> GetUnreliableConnection()
        {
            ISocket channel = null;
            if (_connectToLocalServer)
            {
                var socket = new SocketImpl(SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(IPAddress.Parse("127.0.0.1"), _udpPort);
                socket.Run();

                channel = socket;
            }
            else
            {
                await GamingServices.V1
                    .UnreliableConnectToContainer(_containerId)
                    .HandleResultAsync(
                        async socket => { channel = socket; },
                        async () => Debug.Log("container not found"),
                        async status => Debug.LogError($"invalid container status = {status.Type}"),
                        async exception => Debug.LogError($"unexpected error = {exception}"));
            }

            return channel;
        }
    }
}