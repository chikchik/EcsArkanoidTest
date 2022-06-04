using Game.PlayerInput;
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
            Container.Bind<EcsWorld>().FromInstance(new EcsWorld("main")).AsSingle();
            Container.Bind<EcsInputWorld>().AsSingle();

            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();

            Container.Bind<Client.UI>().AsSingle().NonLazy();
            
            //Container.Bind<UnityEcsClient>().FromComponentsOn(GameObject.Find("Client")).AsSingle();

            if (Application.isEditor ||
                Application.platform is RuntimePlatform.OSXPlayer or RuntimePlatform.WindowsPlayer)
            {
                Container.BindInterfacesAndSelfTo<PlayerInput.PlayerInput>().FromInstance(new DesktopInput())
                    .AsSingle();
                
            }
            else if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer)
            {
                var joystick = Instantiate(joystickPrefab, mainUI.transform);
                Container.BindInterfacesAndSelfTo<PlayerInput.PlayerInput>()
                    .FromInstance(new MobileInput(joystick, mainUI))
                    .AsSingle();
            }
        }
    }
}