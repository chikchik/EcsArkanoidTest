using Contracts.XFlow.Container;
using Contracts.XFlow.Container.Host;

public class ContainerFactory: IContainerFactory
{
    public IContainer Create(IHost host)
    {
        return new Container(host);
    }
}