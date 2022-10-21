using System;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
    public class GameSettingsScriptable : ScriptableObject
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

        public string ContainerId;
        public bool IsLocalServer;
    }
}
