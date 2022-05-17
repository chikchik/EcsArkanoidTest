using System;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Protocol
{
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