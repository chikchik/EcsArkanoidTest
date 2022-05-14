using System;
using System.IO;
using System.IO.Compression;
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

        public ClientAddr Address;
        public bool IsDone;
        public bool IsError;
        public Packet Request;
        public Packet Response;
        public string Url;
        
        private WebSocket socket;

        public WebSocketConnection(Packet request, string url, ClientAddr address)
        {
            this.Address = address;
            this.Url = url;
            this.Request = request;
        }

        public event Event OnConnected;
        public event Event OnError;

        private void OnOpen(WebSocket s)
        {
            Debug.Log("ws opened");

            socket.Send(P2P.BuildRequest(Address, Request));

            if (Address == P2P.ADDR_BROADCAST)
            {
                //если мы на сервере, то не нужно ожидать какое-то входящее сообщение
                Completed(false);
            }
        }

        private void Completed(bool wasError)
        {
            if (IsDone)
            {
                Debug.LogError("wtf? not possible");
                return;
            }

            IsDone = true;
            IsError = wasError;

            if (wasError)
                Debug.Log("connection error");

            try
            {
                if (IsError)
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

                Response = P2P.ParseResponse<Packet>(data);
                Completed(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnMessage(WebSocket ws, string str)
        {
            throw new Exception("not implemented");
            /*
            Debug.Log("onMessage " + str);
            response = JsonUtility.FromJson<Packet>(str);
            Completed(false);
            */
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
            Debug.Log($"Connection start {Url}");
            //Server.prettyPrint("Connection request", request);
            socket = new WebSocket(new Uri(Url));
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
    }
}