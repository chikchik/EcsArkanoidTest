using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.UI;
using Game.UI.Mono;
using Flow.EcsLite;
using Game.View;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private Joystick joystickPrefab;
        [SerializeField] private GameSettings settings;

        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromComponentsOn(GameObject.Find("Main Camera")).AsSingle();
            
            var global = GameObject.Find("Global").GetComponent<Global>();
            Container.Bind<Global>().FromInstance(global).AsSingle();
            
            
            Container.Bind<CharacterView>().FromInstance(global.characterPrefab).AsSingle();
            Container.Bind<BulletView>().FromInstance(global.BulletPrefab).AsSingle();
            
            Container.Bind<FootprintView>().WithId("left").FromInstance(global.LeftFootprintPrefab).AsCached();
            Container.Bind<FootprintView>().WithId("right").FromInstance(global.RightFootprintPrefab).AsCached();
            
            
            
            
            Container.Bind<Objectives>().AsSingle().NonLazy();

            //var world = new EcsWorld("main");
            Container.Bind<EcsWorld>().FromInstance(new EcsWorld("main")).AsCached();
            Container.Bind<EcsWorld>().WithId("input").FromInstance(new EcsWorld("input")).AsCached();

            Container.Bind<EntityDestroyedListener>().AsSingle();
            
            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<UI.UI>().AsSingle().NonLazy();

            Container.Bind<PlayerControlService>().AsSingle();
            Container.Bind<ClientServerServices>().AsSingle();
           
            
            //register states and view
            Container.Bind<SimpleDialogView>().FromInstance(global.SimpleDialogView).AsSingle();
             
            Container.Bind<States>().AsSingle();
            Container.Bind<RootState>().AsSingle();
            Container.Bind<SimpleDialogState>().AsSingle();
            


            Container.Bind<NetClient>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsSystemsFactory>().AsSingle();
            
            var collection = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponents(collection);
            
            Container.Bind<ComponentsCollection>().FromInstance(collection).AsSingle();
            
            Container.Bind<GameSettings>().FromComponentOn(GameObject.Find("[SETUP]")).AsSingle();
            Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            Container.Bind<DevPanel>().AsSingle().NonLazy();
            
            Container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            Container.Bind<MechService>().AsSingle();
           
            

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
    }
}