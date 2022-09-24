using System;
using System.Net;
using System.Threading;
using Fabros.EcsModules.Mech.ClientServer;
using Game.Client;
using Game.ClientServer;
using Game.ClientServer.Services;
using Game.Dev;
using Game.Ecs.Client.Components;
using Game.State;
using Game.UI;
using Game.UI.Mono;
using Game.UIView;
using Game.View;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Inventory.Client.Demo.Interfaces;
using XFlow.Modules.Inventory.Client.Demo.Services;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.States;
using XFlow.Net.Client;
using XFlow.Net.ClientServer;
using XFlow.P2P;
using Zenject;

namespace Game.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameSettings settings;

        private CancellationTokenSource _cancellationTokenSource;
        
        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromComponentsOn(GameObject.Find("Main Camera")).AsSingle();
            
            var global = GameObject.Find("Global").GetComponent<Global>();
            Container.Bind<Global>().FromInstance(global).AsSingle();

            Container.Bind<CharacterView>().FromInstance(global.CharacterPrefab).AsSingle();
            Container.Bind<BulletView>().FromInstance(global.BulletPrefab).AsSingle();
            Container.Bind<ParticleSystem>().FromInstance(global.FireParticles).AsSingle();
            Container.Bind<IInventoryStaticData>().FromInstance(global.InventoryStaticData).AsSingle();
            
            Container.Bind<FootprintView>().WithId("left").FromInstance(global.LeftFootprintPrefab).AsCached();
            Container.Bind<FootprintView>().WithId("right").FromInstance(global.RightFootprintPrefab).AsCached();

            Container.Bind<Objectives>().AsSingle().NonLazy();

            //var world = new EcsWorld("main");
            Container.Bind<EcsWorld>().FromInstance(new EcsWorld("main")).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Input).FromInstance(new EcsWorld(EcsWorlds.Input)).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Event).FromInstance(new EcsWorld(EcsWorlds.Event)).AsCached();
            Container.Bind<EcsWorld>().WithId(EcsWorlds.Dead).FromInstance(new EcsWorld(EcsWorlds.Dead)).AsCached();
            
            
            //Container.Bind
            Container.Bind<CopyToDeadWorldListener>().AsSingle();
            
            Container.Bind<EntityDestroyedListener>().AsSingle();
            
            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<UI.UI>().AsSingle().NonLazy();
            Container.Bind<Canvas>().WithId("HpViewCanvas").FromInstance(global.HpViewCanvas).AsSingle();
            

            Container.Bind<HpView>().FromInstance(global.HpViewPrefab).AsSingle();
            Container.Bind<HpViewManager>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<PlayerControlService>().AsSingle();
            Container.Bind<ClientServerServices>().AsSingle();

            _cancellationTokenSource = new CancellationTokenSource();
            Container.Bind<CancellationToken>().FromInstance(_cancellationTokenSource.Token).AsCached();

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
            
           
            
            //register states and view
            Container.Bind<MechInfoView>().FromInstance(global.MechInfoView).AsSingle();
             
            Container.Bind<States>().AsSingle();
            Container.Bind<RootState>().AsSingle();
            Container.Bind<MechInfoState>().AsSingle();
            Container.Bind<InventoryOpenedState>().AsSingle();
            


            Container.Bind<NetClient>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsSystemsFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsViewSystemsFactory>().AsSingle();
            
            var collection = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponents(collection);
            
            Container.Bind<ComponentsCollection>().FromInstance(collection).AsSingle();
            
            Container.Bind<GameSettings>().FromComponentOn(GameObject.Find("[SETUP]")).AsSingle();
            Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            Container.Bind<DevPanel>().AsSingle().NonLazy();
            
            Container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            Container.Bind<MechService>().AsSingle();

            Container.Bind<AssetInstantiator>().AsSingle();
            Container.BindInterfacesAndSelfTo<MyInventoryService>().AsSingle();
            Container.Bind<InventoryFactory>().AsSingle();

            if (settings.MultiPlayer)
            {
                Container.Bind<IInputService>().To<ClientInputService>()
                    .AsSingle().WhenInjectedInto<PlayerControlService>();
                
                var comp = settings.gameObject.AddComponent<UnityEcsClient>();
                Container.QueueForInject(comp);
            }
            else
            {
                var comp = settings.gameObject.AddComponent<UnityEcsSinglePlayer>();
                Container.QueueForInject(comp);
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _cancellationTokenSource.Cancel();
        }
    }
}