using Contracts.XFlow.Container;
using Contracts.XFlow.Container.Host;

namespace XFlow.Server
{
    public class ContainerFactory: IContainerFactory
    {
        public IContainer Create(IHost host)
        {
            return new Container(host.GetConfig(), host.GetLogger());
        }
    }
}