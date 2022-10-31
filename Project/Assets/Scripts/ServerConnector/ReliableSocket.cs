using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Utils;
using XFlow.P2P;

public class ReliableSocket : BaseSocket
{
    public ReliableSocket(int userId) : base(userId)
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task Connect(IPAddress address, int port)
    {
        Socket.Connect(new IPEndPoint(address, port));
        await Socket.SendAsync(BitConverter.GetBytes(UserId), SocketFlags.None);
    }

    public override async Task Run()
    {
        var socket = Socket;
        var tempBuffer = new byte[16000];
        var buffer = new List<byte>();
        int pos = 0;
            
            
        while (true)
        {
            await SocketUtils.ReceiveAsync(socket, pos, 4, buffer, tempBuffer);
            SocketUtils.GetSlice(buffer, ref pos, 4, ref tempBuffer);
            var size = BitConverter.ToInt32(tempBuffer);
            await SocketUtils.ReceiveAsync(socket, pos, size, buffer, tempBuffer);

            SocketUtils.GetSlice(buffer, ref pos, size, ref tempBuffer);
            var readOnly = new ReadOnlyMemory<byte>(tempBuffer, 0, size);
            
            foreach (var subscriber in Subscribers)
                await subscriber.Invoke(SocketMessage.Message(readOnly));
            
            buffer.Clear();
            pos = 0;
        }
    }

    public override async ValueTask<SocketSendResult> SendAsync(ReadOnlyMemory<byte> message)
    {
        if (isDisposed)
            return new SocketSendResult(SocketSendResultType.SocketClosed, null);
        try
        {
            var header = BitConverter.GetBytes(message.Length);
            await SocketUtils.SendAsync(Socket, header);
            await SocketUtils.SendAsync(Socket, message);
            return new SocketSendResult(SocketSendResultType.Ok, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new SocketSendResult(SocketSendResultType.Unknown, e.ToString());
        }
    }
}