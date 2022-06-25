using System;
using System.IO;
using System.Text;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer
{
    public static class WorldDebugLogger
    {
        public static void Log(this EcsWorld world, string str)
        {
            
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
            
        }
    }
}