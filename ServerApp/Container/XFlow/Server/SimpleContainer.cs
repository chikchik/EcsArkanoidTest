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

        private IUnreliableChannel _unreliableChannel;
        private IAsyncDisposable _unreliableChannelSubs;

        private int _messagesReceived = 0;

        public SimpleContainer(ContainerStartingContext context)
        {
            _context = context;
        }

        public async Task Start()
        {
            _reliableChannel = await _context.Host.ChannelProvider.GetReliableChannelAsync();
            _reliableChannelSubs = await _reliableChannel.SubscribeAsync(OnReliableMessageReceived);

            _unreliableChannel = await _context.Host.ChannelProvider.GetUnreliableChannelAsync();
            _unreliableChannelSubs = await _unreliableChannel.SubscribeAsync(OnUnreliableMessageReceived);
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
            await _unreliableChannel.DisposeAsync();
            await _unreliableChannelSubs.DisposeAsync();
        }

        private async ValueTask OnUnreliableMessageReceived(UnreliableChannelMessage message)
        {
            switch (message.Type)
            {
                case UnreliableChannelMessageType.MessageReceived:
                    _messagesReceived++;
                    var messageArgs = message.GetMessageReceivedArguments();
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnUnreliableMessageReceived.MessageReceived mesCount={_messagesReceived} mes={messageArgs.Value.Message}");
                    await _unreliableChannel.SendAsync(messageArgs.Value.UserAddress, messageArgs.Value.Message);
                    break;

                case UnreliableChannelMessageType.ChannelClosed:
                    var closedArgs = message.GetChannelClosedArguments();
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnUnreliableMessageReceived.ChannelClosed {closedArgs}");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async ValueTask OnReliableMessageReceived(ReliableChannelMessage message)
        {
            switch (message.Type)
            {
                case ReliableChannelMessageType.UserConnected:
                    var connectedArgs = message.GetUserConnectedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnReliableMessageReceived.UserConnected {connectedArgs.UserAddress.ConnectionId}");
                    break;

                case ReliableChannelMessageType.UserDisconnected:
                    var disconnectedArgs = message.GetUserDisconnectedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnReliableMessageReceived.UserDisconnected {disconnectedArgs.UserAddress.ConnectionId}");
                    break;

                case ReliableChannelMessageType.MessageReceived:
                    _messagesReceived++;
                    var messageArgs = message.GetMessageReceivedArguments().Value;
                    await _reliableChannel.SendAsync(messageArgs.UserAddress, messageArgs.Message);
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnReliableMessageReceived.MessageReceived mesCount={_messagesReceived} mes={messageArgs.Message}");
                    break;

                case ReliableChannelMessageType.ChannelClosed:
                    var closedArgs = message.GetChannelClosedArguments().Value;
                    _context.Host.LoggerFactory.System.Log(LogLevel.Information,
                        $"OnReliableMessageReceived.ChannelClosed {closedArgs}");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}