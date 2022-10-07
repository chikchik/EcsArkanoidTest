using System.Diagnostics;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;

namespace XFlow.Server
{
    public class ContainerFactory : IContainerFactory
    {
        public async ValueTask<IContainer> StartContainerAsync(ContainerStartingContext context)
        {
            var sw = Stopwatch.StartNew();
            var c = new Container(context);
            await c.Start();
            context.Host.LoggerFactory.System.Log(LogLevel.Information,
                $"Container started time={sw.ElapsedMilliseconds}ms");
            return c;
        }
    }
}