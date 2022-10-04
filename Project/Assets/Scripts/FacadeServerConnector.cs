using System;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using Gaming.ContainerManager.Models.V1;
using Gaming.Facade;
using Gaming.Facade.Configuration;
using UnityEngine;
using XFlow.Net.Client;

namespace Game
{
    public class FacadeServerConnector : IServerConnector
    {
        private readonly ContainerId _containerId;

        public FacadeServerConnector(string containerId)
        {
            _containerId = ContainerId.Parse(containerId);

            var url = new Uri("https://dev.containers.xfin.net");
            GamingServices.V1.Configure(
                configuration => configuration.WithServerUrl(url).WithAppDataPath(Application.persistentDataPath),
                Debug.Log);
        }

        public async Task<ISocket> GetReliableConnection()
        {
            ISocket channel = null;

            await GamingServices.V1
                .ReliableConnectToContainer(_containerId)
                .HandleResultAsync(
                    async socket => { channel = socket; },
                    async () => Debug.Log("container not found"),
                    async status => Debug.LogError($"invalid container status = {status.Type}"),
                    async exception => Debug.LogError($"unexpected error = {exception}"));

            return channel;
        }

        public async Task<ISocket> GetUnreliableConnection()
        {
            ISocket channel = null;

            await GamingServices.V1
                .UnreliableConnectToContainer(_containerId)
                .HandleResultAsync(
                    async socket => { channel = socket; },
                    async () => Debug.Log("container not found"),
                    async status => Debug.LogError($"invalid container status = {status.Type}"),
                    async exception => Debug.LogError($"unexpected error = {exception}"));

            return channel;
        }
    }
}