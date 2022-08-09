using System;
using System.Collections.Generic;
using System.Threading;
using Contracts.XFlow.Container;
using XFlow.Container;
using XFlow.Server;

namespace ServerApp2
{
    internal class Program
    {
        static void Main(string[] args_)
        {
            var args = new Dictionary<string, string>();
            args[ContainerConfigParams.ROOM] = $"{Config.ROOM_A}{Config.ROOM_B}";
            var factory = new ContainerFactory();
            var container = new RunningContainer(".", args, factory);
            container.Start();
            while (true)
            {
                Thread.Sleep(100);
            }
            
        }
    }
}
