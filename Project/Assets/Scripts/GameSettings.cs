using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public class ContainerAddress
        {
            public string Address;
        }

        [Serializable]
        public class HostAddress
        {
            public string Address;
            public int TcpPort;
            public int UdpPort;
        }

        [Serializable]
        private class ContainerWrapper : ContainerAddress
        {
            [field: SerializeField] [HideInInspector] public int Id;
        }

        [Serializable]
        private class HostWrapper : HostAddress
        {
            [field: SerializeField] [HideInInspector] public int Id;
        }

        public bool MultiPlayer = true;

        public ContainerAddress Container
        {
            get { return _containerAddresses.FirstOrDefault(c => c.Id == _savedId); }
        }

        public HostAddress Host
        {
            get { return _hostAddresses.FirstOrDefault(c => c.Id == _savedId); }
        }

        [field: SerializeField] private List<ContainerWrapper> _containerAddresses = new();
        [field: SerializeField] private List<HostWrapper> _hostAddresses = new();

        [field: SerializeField] [HideInInspector] private int _savedId;

        public List<string> GetHostsNames()
        {
            List<string> names = new List<string>();
            int index = 1;
            for (int i = 0; i < _containerAddresses.Count; i++)
            {
                names.Add($"{index}:{_containerAddresses[i].Address}");
                index++;
            }

            for (int i = 0; i < _hostAddresses.Count; i++)
            {
                names.Add($"{index}:{_hostAddresses[i].Address}");
                index++;
            }

            return names;
        }

        public void SaveHost(int index)
        {
            if (index < _containerAddresses.Count)
            {
                _savedId = _containerAddresses[index].Id;
            }
            else
            {
                index -= _containerAddresses.Count;
                _savedId = _hostAddresses[index].Id;
            }
        }

        public int GetAddressIndex()
        {
            var container = _containerAddresses.FirstOrDefault(c => c.Id == _savedId);
            if (container != null) {
                return _containerAddresses.IndexOf(container);
            }

            var host = _hostAddresses.FirstOrDefault(c => c.Id == _savedId);
            if (host != null) {
                return _containerAddresses.Count + _hostAddresses.IndexOf(host);
            }

            return 0;
        }
       
        public void OnBeforeSerialize()
        {
            for (int i = _containerAddresses.Count - 1; i >= 0; i--)
            {
                if (_containerAddresses.Count(c => c.Id == _containerAddresses[i].Id) > 1)
                {
                    _containerAddresses[i].Id = Random.Range(int.MinValue, int.MaxValue);
                }
            }

            for (int i = _hostAddresses.Count - 1; i >= 0; i--)
            {
                if (_hostAddresses.Count(c => c.Id == _hostAddresses[i].Id) > 1)
                {
                    _hostAddresses[i].Id = Random.Range(int.MinValue, int.MaxValue);
                }
            }
        }
        
        public void OnAfterDeserialize() { }
    }
}
