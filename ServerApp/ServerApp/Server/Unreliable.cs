using System;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class Unreliable : IUnreliableChannel
    {
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask<UnreliableChannelSendResult> SendAsync(IUserAddress userAddress, ReadOnlyMemory<byte> message)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IAsyncDisposable> SubscribeAsync(IUnreliableChannel.SubscribeDelegate subscriber)
        {
            throw new NotImplementedException();
        }
    }
}