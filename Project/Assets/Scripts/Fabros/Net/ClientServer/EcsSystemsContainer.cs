using System;
using System.Collections.Generic;
using Flow.EcsLite;
using Game.Fabros.Net.ClientServer;
using Zenject;

namespace Game.Fabros.Net.ClientServer
{
    public class EcsSystemsContainer
    {
        private enum Target
        {
            Any,
            Client,
            Server
        }

        private struct RegisteredSystem
        {
            public Type SystemClassType;
            public Target Target;
        }
        
        private List<RegisteredSystem> systems = new List<RegisteredSystem>();
        private DiContainer container;
        
        public EcsSystemsContainer(DiContainer container)
        {
            this.container = container.CreateSubContainer();
        }
        
        public void RegisterClient<T>()
        {
            container.Bind<T>().AsTransient();
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Client,
            });
        }
        
        public void RegisterServer<T>()
        {
            container.Bind<T>().AsTransient();
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Server,
            });
        }
        
        public void Register<T>()
        {
            container.Bind<T>().AsTransient();
            
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Any
            });
        }
        
        public IEcsSystem[] CreateNewSystems(EcsWorld world, bool createClientSystems, bool createServerSystems)
        {
            var result = new List<IEcsSystem>();
            //override global EcsWorld to world from EcsSystems
            container.Rebind<EcsWorld>().ToSelf().FromInstance(world).AsTransient();
            
            foreach (var system in this.systems)
            {
                if (system.Target != Target.Any)
                {
                    var add =
                        createClientSystems && system.Target == Target.Client ||
                        createServerSystems && system.Target == Target.Server;

                    if (!add)
                        continue;
                }

                var sys = container.Instantiate(system.SystemClassType) as IEcsSystem;
                result.Add(sys);   
            }
            
            return result.ToArray();
        }
    }
}