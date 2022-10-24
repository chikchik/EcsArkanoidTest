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
            public string address;

            public bool IsContainer {
                get { return Guid.TryParse(address, out _); }
            }
        }
        
        [Serializable]
        public class IpHostAddress : HostAddress
        {
            public int tcpPort;
            public int udpPort;
        }
        
        public bool MultiPlayer = true;

        [SerializeField]
        private List<HostAddress> _containerHosts;
        [SerializeField]
        private List<IpHostAddress> _ipHosts;
        [SerializeField]
        private int _savedHostIndex;

        public List<string> GetHostsNames()
        {
            List<string> names = new List<string>();
            int index = 1;
            for (int i = 0; i < _containerHosts.Count; i++)
            {
                names.Add($"{index}: {_containerHosts[i].address}");
                index++;
            }
            
            for (int i = 0; i < _ipHosts.Count; i++)
            {
                names.Add($"{index}: {_ipHosts[i].address}");
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
