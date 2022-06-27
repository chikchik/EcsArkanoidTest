using System.Collections.Generic;
using BestHTTP.WebSocket;
using Cysharp.Threading.Tasks;
using Fabros.P2P;

namespace Game.Fabros.Net.Client.Socket
{
    public class UnitySocket
    {
        private readonly List<Message> received = new List<Message>();
        private readonly WebSocket socket;

        public UnitySocket(WebSocket socket)
        {
            this.socket = socket;

            socket.OnBinary += (s, msg) => { received.Add(new Message {buffer = msg}); };
        }

        public async UniTask<Message> AsyncWaitMessage()
        {
            while (true)
            {
                var rec = PopMessage();
                if (rec != null)
                    return rec;
                await UniTask.WaitForEndOfFrame();
            }
        }

        public void Send(byte[] body)
        {
            socket.Send(body);
        }

        public Message PopMessage()
        {
            if (received.Count == 0) return null;

            var packet = received[0];
            received.RemoveAt(0);
            return packet;
        }

        public void Close()
        {
            socket.Close();
        }

        public class Message
        {
            public byte[] buffer;
        }
    }
}