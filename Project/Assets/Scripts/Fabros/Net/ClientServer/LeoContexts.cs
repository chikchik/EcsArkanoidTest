using System;
using System.IO;
using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Tick.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer
{
    public class LeoContexts
    {
        //public ComponentsCollection Components { get; }

        //compatibility with old code
        //public ComponentsCollection Pool => Components;
        /*
         * позволяет сохранять в отдельный файл игровые данные и сравнивать состояния миров между собой
        */

         
        
        private readonly string hashDir;
        private readonly bool writeHashes;

        public LeoContexts(string hashDir)
        {
            this.hashDir = hashDir;

            //Components = components;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            writeHashes = Directory.Exists(hashDir);
            if (writeHashes && (hashDir.Contains("temp") || hashDir.Contains("tmp")))
            {
                Debug.Log("writeHashes");
                var files = Directory.GetFiles(hashDir);
                Array.ForEach(files, file => {
                    File.Delete(file);
                });
            }
#endif
        }
    }
}