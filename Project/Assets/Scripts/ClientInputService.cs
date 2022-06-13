using System;
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
                .Write(P2P.ADDR_SERVER.Address)
                .Write(0xff)
                .Write(playerID)
                .Write(client.GetNextInputTick().Value);

            if (inputComponent is PingComponent)
            {
                writer.WriteInt32(0);
            }

            if (inputComponent is InputActionComponent a)
            {
                writer.WriteInt32(1);
                writer.Write(a);
            }

            if (inputComponent is InputMoveDirectionComponent b)
            {
                writer.WriteInt32(2);
                writer.Write(b);
            }

            if (inputComponent is InputMoveToPointComponent c)
            {
                writer.WriteInt32(3);
                writer.Write(c);
            }

            if (inputComponent is InputShotComponent d)
            {
                writer.WriteInt32(4);
                writer.Write(d);
            }

            var array = writer.CopyToByteArray();

            client.Socket.Send(array);
        
        }
    }
}