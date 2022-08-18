using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameSettings : MonoBehaviour
    {
        [Serializable]
        public class UdpHost
        {
            public string Address;
            public int Port;
        }
        
        public bool MultiPlayer = true;
        public bool OverrideDefaultServerRoom = false;
        public string OverrideRoom;

        public UdpHost[] UdpHosts;
        [Range(0, 3)]
        public int SelectedUdpHostIndex;
    }
}
