using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.P2P;

public class ReliableSocket : BaseSocket
{
    public ReliableSocket(string userId) : base(userId)
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public override async Task Connect(IPAddress address, int port)
    {
        await base.Connect(address, port);

        var id = Encoding.UTF8.GetBytes(UserId, 0, UserId.Length);
        var packet = P2P.Combine(BitConverter.GetBytes(sizeof(int) + id.Length), id);

        await Socket.SendAsync(packet, SocketFlags.None);
    }

    public override async Task Run()
    {
        const int packetSizeHeader = sizeof(long);

        var dataStream = new Queue<byte>();
        var rcvBuffer = new ArraySegment<byte>(new byte[64000]);

        var nextPacketSize = -1L;

        var messages = new List<byte[]>();

        while (true)
        {
            var receivedLength = await Socket.ReceiveAsync(rcvBuffer, SocketFlags.None);

            if (receivedLength == 0)
            {
                Socket.Dispose();

                break;
            }

            for (var i = rcvBuffer.Offset; i < receivedLength; i++)
                dataStream.Enqueue(rcvBuffer[i]);

            while (true)
            {
                if (nextPacketSize == -1)
                {
                    if (dataStream.Count < packetSizeHeader)
                        break;

                    var packetSizeBuffer = new byte[packetSizeHeader];
                    for (var i = 0; i < packetSizeHeader; i++)
                        packetSizeBuffer[i] = dataStream.Dequeue();
                    nextPacketSize = BitConverter.ToInt64(packetSizeBuffer);
                }

                // already read packet size (packetSizeHeader)
                if (nextPacketSize - packetSizeHeader > dataStream.Count)
                    break;

                var message = new byte[nextPacketSize];
                for (var i = 0; i < nextPacketSize - packetSizeHeader; i++)
                    message[i] = dataStream.Dequeue();
                messages.Add(message);

                nextPacketSize = -1;
            }

            foreach (var message in messages)
            {
                foreach (var subscriber in Subscribers)
                    await subscriber.Invoke(SocketMessage.Message(message));
            }

            messages.Clear();
        }
    }

    public override async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        if (isDisposed)
            return new SocketSendResult(SocketSendResultType.SocketClosed, null);
        try
        {
            await Socket.SendAsync(P2P.PackMessage(message.ToArray()), SocketFlags.None);

            return new SocketSendResult(SocketSendResultType.Ok, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SocketSendResult(SocketSendResultType.Unknown, e.ToString());
        }
    }
}