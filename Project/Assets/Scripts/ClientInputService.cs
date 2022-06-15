using System;
using Fabros.Ecs.ClientServer.Serializer;
using Fabros.Ecs.Utils;
using Fabros.P2P;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Input.Proto;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game
{
    /**
     * 
     */
    public class ClientInputService: IInputService
    {
        private NetClient client;
        private HGlobalWriter writer = new HGlobalWriter();

        public ClientInputService(NetClient client)
        {
            this.client = client;
        }
        
        
        public void Input(EcsWorld inputWorld, int playerID, int tick, IInputComponent inputComponent)
        {
            if (tick != client.GetNextInputTick().Value)
            {
                Debug.LogError($"{tick} != {client.GetNextInputTick().Value}");
            }

            writer.Reset()
                .WriteByteArray(P2P.ADDR_SERVER.Address)
                .WriteT(0xff)
                .WriteT(playerID)
                .WriteT(client.GetNextInputTick().Value);

            if (inputComponent is PingComponent)
            {
                writer.WriteInt32(0);
            }

            if (inputComponent is InputActionComponent a)
            {
                writer.WriteInt32(1);
                writer.WriteT(a);
            }

            if (inputComponent is InputMoveDirectionComponent b)
            {
                writer.WriteInt32(2);
                writer.WriteT(b);
            }

            if (inputComponent is InputMoveToPointComponent c)
            {
                writer.WriteInt32(3);
                writer.WriteT(c);
            }

            if (inputComponent is InputShotComponent d)
            {
                writer.WriteInt32(4);
                writer.WriteT(d);
            }

            var array = writer.CopyToByteArray();

            client.Socket.Send(array);
        
        }
    }
}