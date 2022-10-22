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

            var settings = Resources.Load<GameSettings>("GameSettings");
            Container.Bind<GameSettings>().FromInstance(settings).AsSingle();

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