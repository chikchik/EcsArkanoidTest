using System;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using Gaming.ContainerManager.Models.V1;

namespace XFlow.Server
{
    public class SimpleContainer : IContainer
    {
        private readonly ContainerStartingContext _context;

        private IReliableChannel _reliableChannel;
        private IAsyncDisposable _reliableChannelSubs;

        private int _messagesReceived = 0;

        public SimpleContainer(ContainerStartingContext context)
        {
            _context = context;
        }

        public async Task Start()
        {
            _reliableChannel = await _context.Host.ChannelProvider.GetReliableChannelAsync();
            _reliableChannelSubs = await _reliableChannel.SubscribeAsync(OnReliableMessageReceived);
        }

        public async ValueTask<ContainerState> GetStateAsync()
        {
            return ContainerState.Empty;
        }

        public async ValueTask<string> GetInfoAsync()
        {
            return _messagesReceived.ToString();
        }

        public async ValueTask StopAsync()
        {
            await _reliableChannel.DisposeAsync();
            await _reliableChannelSubs.DisposeAsync();
        }

        private async ValueTask OnReliableMessageReceived(ReliableChannelMessage message)
        {
            switch (message.Type)
            {
                case ReliableChannelMessageType.UserConnected:
                    var connectedArgs = message.GetUserConnectedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"user connected {connectedArgs.UserAddress}");
                    break;

                case ReliableChannelMessageType.UserDisconnected:
                    var disconnectedArgs = message.GetUserDisconnectedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"user disconnected {disconnectedArgs.UserAddress}");
                    break;

                case ReliableChannelMessageType.MessageReceived:
                    _messagesReceived++;
                    var messageArgs = message.GetMessageReceivedArguments().Value;
                    await _reliableChannel.SendAsync(messageArgs.UserAddress, messageArgs.Message);
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"MessageReceived recCount={_messagesReceived} mes={messageArgs.Message}");
                    break;

                case ReliableChannelMessageType.ChannelClosed:
                    var closedArgs = message.GetChannelClosedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"ChannelClosed {closedArgs}");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}