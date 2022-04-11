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

            var world = new EcsWorld("mainWorld");
            Container.Bind<EcsWorld>().FromInstance(world).AsSingle();

            var ui = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(ui).AsSingle();
            //Container.Bind<UnityEcsClient>().FromComponentsOn(GameObject.Find("Client")).AsSingle();

            InstallPlayerInput(ui);
        }

        private void InstallPlayerInput(MainUI mainUI)
        {
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