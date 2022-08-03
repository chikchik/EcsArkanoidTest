using Contracts;

public class ContainerFactory: IContainerFactory
{
    public IContainer Create(IHost host)
    {
        return new Container();
    }
}