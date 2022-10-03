using Fabros.EcsModules.Mech.ClientServer;
using Game.Client;
using Game.ClientServer;
using Game.ClientServer.Services;
using Game.Dev;
using Game.State;
using Game.UI;
using Game.UI.Mono;
using Game.UIView;
using Game.View;
using UnityEngine;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Inventory.Client.Demo.Interfaces;
using XFlow.Modules.Inventory.Client.Demo.Services;
using XFlow.Modules.States;
using XFlow.Net.Client;

namespace Game.Installers
{
    public class GameSceneInstallerAlt : XFlowEscInstaller
    {
        protected override void DoInstallBindings(GameSettings gameSettings)
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

            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<UI.UI>().AsSingle().NonLazy();
            Container.Bind<Canvas>().WithId("HpViewCanvas").FromInstance(global.HpViewCanvas).AsSingle();

            Container.Bind<HpView>().FromInstance(global.HpViewPrefab).AsSingle();
            Container.Bind<HpViewManager>().AsSingle().NonLazy();

            //register states and view
            Container.Bind<MechInfoView>().FromInstance(global.MechInfoView).AsSingle();
             
            Container.Bind<States>().AsSingle();
            Container.Bind<RootState>().AsSingle();
            Container.Bind<MechInfoState>().AsSingle();
            Container.Bind<InventoryOpenedState>().AsSingle();

            Container.BindInterfacesAndSelfTo<EcsSystemsFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsViewSystemsFactory>().AsSingle();

            Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            Container.Bind<DevPanel>().AsSingle().NonLazy();
            
            Container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            Container.Bind<MechService>().AsSingle();

            Container.Bind<AssetInstantiator>().AsSingle();
            Container.BindInterfacesAndSelfTo<MyInventoryService>().AsSingle();
            Container.Bind<InventoryFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<EntityDestroyedListener>().AsSingle();

            if (gameSettings.MultiPlayer)
            {
                var comp = gameSettings.gameObject.AddComponent<UnityEcsClient>();
                Container.QueueForInject(comp);
            }
            else
            {
                Container.Bind<SingleGame>().AsSingle();

                var comp = gameSettings.gameObject.AddComponent<UnityEcsSinglePlayer>();
                Container.QueueForInject(comp);
            }

        }
    }
}