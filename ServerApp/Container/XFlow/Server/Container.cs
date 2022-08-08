using Contracts.XFlow.Container;
using Contracts.XFlow.Container.Host;

namespace XFlow.Server
{
    public class Container: IContainer
    {
        private Main program;
    
        IHost host;

        public Container(IHost host)
        {
            this.host = host;
        }
    
        public void Init()
        {
            program = new Main(host.GetConfig(), host.GetLogger());
        }

        public void Start()
        {
            program.Run();
        }


        public void Stop()
        {
            program.Stop();
        }

        public string GetInfo()
        {
            return program.GetInfo();
        }
    }
}