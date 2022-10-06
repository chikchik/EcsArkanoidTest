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
            context.Host.LoggerFactory.System.Log(LogLevel.Critical,$"Container initialization {sw.ElapsedMilliseconds}");
            var sw2 = Stopwatch.StartNew();
            await c.Start();
            context.Host.LoggerFactory.System.Log(LogLevel.Critical,$"Container Start {sw2.ElapsedMilliseconds}");
            context.Host.LoggerFactory.System.Log(LogLevel.Critical,$"Container full start {sw.ElapsedMilliseconds}");
            sw.Stop();
            sw2.Stop();
            return c;
        }
    }
}