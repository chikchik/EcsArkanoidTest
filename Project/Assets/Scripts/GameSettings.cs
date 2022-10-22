using System;
using System.Net;
using UnityEngine;

namespace Game
{
    public class GameSettings : ScriptableObject
    {
        [Serializable]
        public class ConnectionHost
        {
            public string Address;
            public int tcpPort;
            public int udpPort;
            
            public bool IsIp => IPAddress.TryParse(Address, out _);
        }
        
        public bool MultiPlayer = true;

        public ConnectionHost[] UdpHosts;
        public int SelectedUdpHostIndex;
    }
}
