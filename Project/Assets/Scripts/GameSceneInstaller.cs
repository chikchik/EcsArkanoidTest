using Game.Client;
using Game.ClientServer;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.UI;
using Leopotam.EcsLite;
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

            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<Client.UI>().AsSingle().NonLazy();

            Container.Bind<MPInputService>().AsSingle();
            Container.Bind<SingleInputService>().AsSingle();
            Container.Bind<PlayerInputService>().AsSingle();

            if (settings.SinglePlayer)
                settings.GetComponent<UnityEcsSinglePlayer>().enabled = true;
            else
                settings.GetComponent<UnityEcsClient>().enabled = true;

            Container.Bind<NetClient>().AsSingle();
        }
    }
}