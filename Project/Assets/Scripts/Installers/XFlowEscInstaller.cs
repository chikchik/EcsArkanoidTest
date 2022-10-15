using System.Net;
using System.Threading;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Net.Client;
using XFlow.Net.ClientServer;
using XFlow.P2P;
using Zenject;

namespace Game.Installers
{
    public class XFlowEscInstaller : MonoInstaller
    {
        [SerializeField] 
        private GameSettings settings;

        private CancellationTokenSource _cancellationTokenSource;

        public sealed override void InstallBindings()
        {
            Container.Bind<EcsWorld>().FromInstance(new EcsWorld(EcsWorlds.Main)).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Input).FromInstance(new EcsWorld(EcsWorlds.Input)).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Event).FromInstance(new EcsWorld(EcsWorlds.Event)).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Dead).FromInstance(new EcsWorld(EcsWorlds.Dead)).AsCached();
            
            _cancellationTokenSource = new CancellationTokenSource();
            Container.Bind<CancellationToken>().FromInstance(_cancellationTokenSource.Token).AsCached();

            var collection = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponents(collection);
            Container.Bind<ComponentsCollection>().FromInstance(collection).AsSingle();
            
            Container.Bind<GameSettings>().FromInstance(settings).AsSingle();

            if (settings.MultiPlayer)
            {
                var udpHost = settings.UdpHosts[settings.SelectedUdpHostIndex];
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(udpHost.Address), udpHost.Port);
                Container.Bind<IPEndPoint>().WithId("udp").FromInstance(ipEndPoint).AsCached();

                if (settings.OverrideDefaultServerRoom)
                {
                    var url = $"{P2P.DEV_SERVER_WS}/{settings.OverrideRoom}";
                    Container.Bind<string>().WithId("serverUrl").FromInstance(url).AsCached();
                }
                else
                {
                    Container.Bind<string>().WithId("serverUrl").FromInstance(Config.URL).AsCached();    
                }
            
                Container.Bind<string>().WithId("tmpHashesPath").FromInstance(Config.TMP_HASHES_PATH).AsCached();

                Container.Bind<NetClient>().AsSingle();
            }
            else
            {
                // Container.Bind<SingleGame>().AsSingle();
            }
            
            DoInstallBindings(settings);
        }
        protected virtual void DoInstallBindings(GameSettings gameSettings) {}
        
        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _cancellationTokenSource.Cancel();
        }
        
    }
}