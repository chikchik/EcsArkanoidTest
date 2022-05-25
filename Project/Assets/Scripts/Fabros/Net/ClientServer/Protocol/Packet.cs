using System;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    /* это протокол для прототипа, для простоты в нем нет оптимизаций, содержит лишние поля и прочий мусор */
    [Serializable]
    public class Packet
    {
        public int playerID;

        public UserInput input;

        public WorldUpdateProto WorldUpdate;
        public bool hasWorldUpdate;
        public bool hasWelcomeFromServer;

        public Hello hello;
        public bool hasHello;
        public bool isPing;
    }
}