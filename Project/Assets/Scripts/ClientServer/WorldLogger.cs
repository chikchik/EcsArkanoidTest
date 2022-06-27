using System;
using System.IO;
using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer
{
    public class WorldLogger : IWorldLogger
    {
        public void Log(EcsWorld world, string str)
        {
#if true
            var worldName = world.GetDebugName();

            try
            {
                lock (world)
                {
                    using (var file = File.AppendText($"{Config.OTHER_LOGS_PATH}/{worldName}.log"))
                    {
                        var ftext = $"{world.GetTick()}) {str}";
                        file.WriteLine(ftext);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            
            var tx = $"{worldName}:{world.GetTick()}) {str}";
            Debug.Log(tx);
#endif
        }
    }
}