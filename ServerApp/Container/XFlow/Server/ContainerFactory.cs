using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;

namespace XFlow.Server
{
    public class ContainerFactory : IContainerFactory
    {
        public async ValueTask<IContainer> StartContainerAsync(ContainerStartingContext context)
        {
            var c = new SimpleContainer(context);
            await c.Start();
            return c;
        }
    }
}