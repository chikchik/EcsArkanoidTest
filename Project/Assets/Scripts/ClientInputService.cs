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
        private ComponentsCollection collection;

        public ClientInputService(NetClient client, ComponentsCollection collection)
        {
            this.client = client;
            this.collection = collection;
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

            var cm = collection.GetComponent(inputComponent.GetType());
            cm.WriteSingleComponent(writer, inputComponent);

            var array = writer.CopyToByteArray();

            client.Socket.Send(array);
        
        }
    }
}