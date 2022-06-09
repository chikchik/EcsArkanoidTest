using Game.ClientServer;
using Game.UI;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private Joystick joystickPrefab;

        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromComponentsOn(GameObject.Find("Main Camera")).AsSingle();
            Container.Bind<Global>().FromComponentsOn(GameObject.Find("Global")).AsSingle();
            Container.Bind<Objectives>().AsSingle().NonLazy();

            //var world = new EcsWorld("main");
            Container.Bind<EcsWorld>().FromInstance(new EcsWorld("main")).AsCached();
            Container.Bind<EcsWorld>().WithId("input").FromInstance(new EcsWorld("input")).AsCached();

            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<Client.UI>().AsSingle().NonLazy();


            Container.Bind<PlayerInputService>().AsSingle();
        }
    }
}