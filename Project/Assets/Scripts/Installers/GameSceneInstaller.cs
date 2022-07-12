using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Game.ClientServer;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.UI;
using Game.UI.Mono;
using Leopotam.EcsLite;
using UnityEditor.PackageManager;
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
            Container.Bind<Global>().FromComponentsOn(GameObject.Find("Global")).AsSingle();
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


            Container.Bind<NetClient>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsSystemsFactory>().AsSingle();
            Container.Bind<ComponentsCollection>().FromInstance(SharedComponents.CreateComponentsPool()).AsSingle();


            
            Container.Bind<GameSettings>().FromComponentOn(GameObject.Find("[SETUP]")).AsSingle();
            Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            Container.Bind<DevPanel>().AsSingle().NonLazy();
            

            if (settings.MultiPlayer)
            //if (true)
            {
                Container.Bind<IInputService>().To<ClientInputService>()
                    .AsSingle().WhenInjectedInto<PlayerControlService>();
                
                //Container.Bind<IInputService>().To<ApplyWorldChangesInputService>()
                //    .AsSingle().WhenInjectedInto<NetClient>();
                
                var comp = settings.gameObject.AddComponent<UnityEcsClient>();
                Container.QueueForInject(comp);
            }
            else
            {
                //Container.Bind<IInputService>().To<ApplyWorldChangesInputService>()
                //   .AsSingle().WhenInjectedInto<PlayerControlService>();

                var comp = settings.gameObject.AddComponent<UnityEcsSinglePlayer>();
                Container.QueueForInject(comp);
            }
        }
    }
}