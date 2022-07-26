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
using Flow.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer
{
    public class SyncDebugService
    {
        private readonly string hashDir;
        private readonly bool loggingEnabled;

        public SyncDebugService(string hashDir)
        {
            this.hashDir = hashDir;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            if (Directory.Exists(hashDir) && (hashDir.Contains("temp") || hashDir.Contains("tmp")))
            {
                try
                {
                    var files = Directory.GetFiles(hashDir);
                    Array.ForEach(files, file =>
                    {
                        File.Delete(file);
                    });
                    loggingEnabled = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"tmp folder '{hashDir}' exists but something goes wrong {ex}");
                }
            }
#endif
        }

        public SyncWorldLogger CreateLogger()
        {
            if (loggingEnabled)
                return new SyncWorldLogger(hashDir);
            return null;
        }
    }
}