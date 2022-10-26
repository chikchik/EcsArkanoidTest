using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class CommonEcsGameSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool MultiPlayer = true;
        [Serializable]
        public class Container
        {
            public string ContainerId;
        }

        [Serializable]
        public class Host
        {
            public string Hostname = "127.0.0.1";
            public int TcpPort = 12001;
            public int UdpPort = 12002;
        }

        [Serializable]
        private class ContainerWrapper : Container
        {
            [field: SerializeField] [HideInInspector] public int Id;
        }

        [Serializable]
        private class HostWrapper : Host
        {
            [field: SerializeField] [HideInInspector] public int Id;
        }


        public Container SelectedContainer
        {
            get { return _containers.FirstOrDefault(c => c.Id == _selectedId); }
        }

        public Host SelectedHost
        {
            get { return _hosts.FirstOrDefault(c => c.Id == _selectedId); }
        }

        [field: SerializeField] private List<ContainerWrapper> _containers = new();
        [field: SerializeField] private List<HostWrapper> _hosts = new();

        [field: SerializeField] [HideInInspector] private int _selectedId;

        public List<string> GetServerNames()
        {
            List<string> names = new List<string>();
            int index = 1;
            for (int i = 0; i < _containers.Count; i++)
            {
                names.Add($"{_containers[i].ContainerId}");
                index++;
            }

            for (int i = 0; i < _hosts.Count; i++)
            {
                var host = _hosts[i];
                names.Add($"{host.Hostname} : {host.TcpPort} ({host.UdpPort})");
                index++;
            }

            return names;
        }

        public void SelectServerByIndex(int index)
        {
            if (index < _containers.Count)
            {
                if (_containers[index].Id == 0) {
                    _containers[index].Id = Random.Range(int.MinValue, int.MaxValue);
                }
                _selectedId = _containers[index].Id;
            }
            else
            {
                index -= _containers.Count;
                if (_hosts[index].Id == 0) {
                    _hosts[index].Id = Random.Range(int.MinValue, int.MaxValue);
                }
                _selectedId = _hosts[index].Id;
            }
        }

        public int GetSelectedServer()
        {
            var container = _containers.FirstOrDefault(c => c.Id == _selectedId);
            if (container != null) {
                return _containers.IndexOf(container);
            }

            var host = _hosts.FirstOrDefault(c => c.Id == _selectedId);
            if (host != null) {
                return _containers.Count + _hosts.IndexOf(host);
            }

            return 0;
        }
       
        public void OnBeforeSerialize()
        {
            for (int i = _containers.Count - 1; i >= 0; i--)
            {
                if (_containers.Count(c => c.Id == _containers[i].Id) > 1)
                {
                    _containers[i].Id = Random.Range(int.MinValue, int.MaxValue);
                }
            }

            for (int i = _hosts.Count - 1; i >= 0; i--)
            {
                if (_hosts.Count(c => c.Id == _hosts[i].Id) > 1)
                {
                    _hosts[i].Id = Random.Range(int.MinValue, int.MaxValue);
                }
            }
        }
        
        public void OnAfterDeserialize() { }
    }
}
