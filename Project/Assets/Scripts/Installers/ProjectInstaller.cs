using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Game.ClientServer;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.UI;
using Flow.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
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