using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.P2P;

public class UnreliableSocket : BaseSocket
{
    private IPEndPoint _endPoint;
    public UnreliableSocket(IPEndPoint endPoint, int userId) : base(userId)
    {
        _endPoint = endPoint;
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }
    
    public async  Task Connect()
    {
        
    }
    
    public override async Task Run()
    {
        var rcvBytes = new byte[64000];
        var rcvBuffer = new ArraySegment<byte>(rcvBytes);

        while (true)
        {
            var rcvResult = await Socket.ReceiveFromAsync(rcvBuffer, SocketFlags.None, _endPoint);
            var size = rcvResult.ReceivedBytes;

            var msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(size).ToArray();
            var message = SocketMessage.Message(msgBytes);

            foreach (var subscriber in Subscribers)
                await subscriber.Invoke(message);
        }
    }

    public override async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        if (isDisposed)
            return new SocketSendResult(SocketSendResultType.SocketClosed, null);

        try
        {
            var packet = P2P.Combine(BitConverter.GetBytes(UserId), message.ToArray());

            await Socket.SendToAsync(packet, SocketFlags.None, _endPoint);

            return new SocketSendResult(SocketSendResultType.Ok, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SocketSendResult(SocketSendResultType.Unknown, e.ToString());
        }
    }
}