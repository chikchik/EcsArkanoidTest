using System;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class SimpleHost : IHost
    {
        public ILoggerFactory LoggerFactory { get; }
        public IChannelProvider ChannelProvider { get; }
        
        public IFileSystem FileSystem { get; }

        public SimpleHost(ILoggerFactory loggerFactory, IChannelProvider channelProvider)
        {
            LoggerFactory = loggerFactory;
            ChannelProvider = channelProvider;
        }

        public Task ShutdownAsync(Exception? exception = null)
        {
            throw new NotImplementedException();
        }
    }
}