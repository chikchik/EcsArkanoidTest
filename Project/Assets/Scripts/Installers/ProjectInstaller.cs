using Zenject;

namespace Game.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //Container.Bind<GameSettings>().FromComponentOn(GameObject.Find("[SETUP]")).AsSingle();
            //Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            //Container.Bind<DevPanel>().AsSingle().NonLazy();
        }
    }
}