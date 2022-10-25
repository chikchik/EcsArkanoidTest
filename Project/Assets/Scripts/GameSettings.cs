using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameSettings : ScriptableObject
    {
        [Serializable]
        public class HostAddress
        {
            public string Address;

            public bool IsContainer => Guid.TryParse(Address, out _);
        }
        
        [Serializable]
        public class IpHostAddress : HostAddress
        {
            public int TcpPort;
            public int UdpPort;
        }
        
        public bool MultiPlayer = true;

        [field: SerializeField] private List<HostAddress> _containerHosts = new ();
        [field: SerializeField] private List<IpHostAddress> _ipHosts = new();
        [field: SerializeField] [HideInInspector] private int _savedHostIndex;

        public List<string> GetHostsNames()
        {
            List<string> names = new List<string>();
            int index = 0;
            for (int i = 0; i < _containerHosts.Count; i++)
            {
                names.Add($"{index}: {_containerHosts[i].Address}");
                index++;
            }
            
            for (int i = 0; i < _ipHosts.Count; i++)
            {
                names.Add($"{index}: {_ipHosts[i].Address}");
                index++;
            }

            if (_savedHostIndex >= names.Count) {
                _savedHostIndex = names.Count - 1;
            }
            
            return names;
        }

        public void SaveHost(int index)
        {
            _savedHostIndex = index;
        }

        public HostAddress GetHostAddress()
        {
            var index = _savedHostIndex;
            if (index >= _containerHosts.Count)
            {
                index -= _containerHosts.Count;
                return _ipHosts[index];
            }

            return _containerHosts[index];
        }
    }
}
