using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Contracts.XFlow.Container.Host;
using UnityEngine;

namespace XFlow.Server
{
    public struct UdpPacket
    {
        public EndPoint EndPoint;
        public byte[] Data;
    }
    
    public class UDPServer: IDisposable
    {
        private const int _listenPort = 11000;
        private List<UdpPacket> _packets = new List<UdpPacket>();
        private Socket _socket;
        private Thread _thread;

        public Socket Socket => _socket;

        public UDPServer()
        {
        }
        /*
        private static void StartListener()
        {
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            //listener.Connect()

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    // listener.Send()

                    Console.WriteLine($"Received broadcast from {groupEP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
        */

        public UdpPacket? GetPacket()
        {
            lock (_packets)
            {
                if (_packets.Count == 0)
                    return null;
                var data = _packets[0];
                _packets.RemoveAt(0);
                return data;
            }
        }

        private void Serve()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var addr = new IPEndPoint(IPAddress.Any, 12345);

                _socket.Bind(addr);

                //EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                var data = new byte[16192];

                while (true)
                {
                    var packet = new UdpPacket();
                    packet.EndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int res = _socket.ReceiveFrom(data, ref packet.EndPoint);
                    packet.Data = new byte[res];
                    Array.Copy(data, packet.Data, res);
                    lock (_packets)
                    {
                        _packets.Add(packet);
                    }

                    //socket.SendTo(copy, (IPEndPoint)remote);
                }
            }
            catch (Exception e)
            {
                
            }
        }

        public void Run(CancellationToken token)
        {
            Debug.Log("UDPServer.Run");
            //this.token = token;
            _thread = new Thread(Serve);
            _thread.Start();
        }

        public void Dispose()
        {
            Debug.Log("UDPServer.Dispose");
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                _socket.Close();
            }
        }
    }
}