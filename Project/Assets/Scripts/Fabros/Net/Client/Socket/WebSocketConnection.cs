using System;
using System.Text;
using BestHTTP.WebSocket;
using Fabros.P2P;
using Game.Fabros.Net.ClientServer.Protocol;
using UnityEngine;

namespace Game.Fabros.Net.Client.Socket
{
    public class WebSocketConnection
    {
        public delegate void Event();

        public string addr;

        public bool isDone;
        public bool isError;
        public bool isStarted;

        public Packet request;
        public Packet response;

        private WebSocket socket;

        public string url;

        public WebSocketConnection(Packet request, string room, string id, string addr)
        {
            this.addr = addr;
            url = $"wss://rts.oxygine.org/XsZubnMOTHC0JRDTS95S/{room}/{id}";
            //url = $"ws://localhost:9096/XsZubnMOTHC0JRDTS95S/{room}/{id}";
            this.request = request;
        }

        public event Event OnConnected;
        public event Event OnError;

        private void OnOpen(WebSocket s)
        {
            Debug.Log("ws opened");

            socket.Send(P2P.BuildRequest(addr, JsonUtility.ToJson(request)));

            if (addr == P2P.ADDR_BROADCAST)
                //если мы на сервере, то не нужно ожидать какое-то входящее сообщение
                Completed(false);
        }

        private void Completed(bool wasError)
        {
            if (isDone)
            {
                Debug.LogError("wtf? not possible");
                return;
            }

            isDone = true;
            isError = wasError;

            if (wasError)
                Debug.Log("connection error");

            try
            {
                if (isError)
                    OnError?.Invoke();
                else
                    OnConnected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnBinary(WebSocket webSocket, byte[] data)
        {
            try
            {
                Debug.Log("on binary " + data.Length);

                var errAddr = "";
                if (P2P.CheckError(data, out errAddr))
                {
                    Completed(true);
                    return;
                }

                response = Packet2Message(data);
                Completed(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnMessage(WebSocket ws, string str)
        {
            Debug.Log("onMessage " + str);
            response = JsonUtility.FromJson<Packet>(str);
            Completed(false);
        }

        private void onError(WebSocket w, Exception ex)
        {
            if (ex != null)
                Debug.Log("ws error: " + ex.Message);
            else
                Debug.Log("ws error.");

            Completed(true);
        }


        public void Start()
        {
            Debug.Log($"Connection start {url}");
            //Server.prettyPrint("Connection request", request);
            isStarted = true;
            socket = new WebSocket(new Uri(url));
            socket.OnOpen += OnOpen;
            socket.OnBinary += OnBinary;
            socket.OnError += onError;
            socket.OnMessage += OnMessage;

            socket.Open();
        }

        public void Unsubscribe()
        {
            socket.OnOpen -= OnOpen;
            socket.OnBinary -= OnBinary;
            socket.OnError -= onError;
            socket.OnMessage -= OnMessage;
        }

        public UnitySocket ExtractSocket()
        {
            Debug.Log("extractSocket");
            Unsubscribe();
            return new UnitySocket(socket);
        }


        public static Packet Packet2Message(UnitySocket.Message msg)
        {
            return JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(msg.buffer));
        }

        public static Packet Packet2Message(byte[] msg)
        {
            return JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(msg));
        }
    }
}