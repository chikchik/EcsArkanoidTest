using System;
using System.IO;
using System.Text;
using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer
{
    public class SyncWorldLogger : IWorldLogger
    {
        struct LoggerState
        {
            public int tick;
            public bool inTick;
            public int len;
        }

        private StringBuilder sb = new StringBuilder(256);
        
        public void Log(EcsWorld world, string str)
        {
            var state = GetState(world);
            var offset = state.inTick ? "    " : "";
            var txt = $"{offset}{str}";
            LogRaw(world, txt);
        }
        
        private void LogRaw(EcsWorld world, string str)
        {
#if false
            var worldName = world.GetDebugName();

            try
            {
                lock (world)
                {
                    using (var file = File.AppendText($"{Config.TMP_HASHES_PATH}/{worldName}.log"))
                    {
                        file.WriteLine(str);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            
            //var tx = $"{worldName}:{world.GetTick()}) {str}";
#endif
        }

        ref LoggerState GetState(EcsWorld world)
        {
            return ref world.GetOrCreateUniqueRef<LoggerState>();
        }
        
        public void BeginTick(EcsWorld world, int tick)
        {
            GetState(world).tick = tick;
            GetState(world).inTick = true;
            var b2 = world.GetUnique<Box2DWorldComponent>().WorldReference.ToInt64();
            LogRaw(world, $"tick {tick} [");//b2:0x{b2:X8}"); 
        }
        
        public void EndTick(EcsWorld world, int tick)
        {
            GetState(world).inTick = false;
            LogRaw(world, $"] {tick}\n");
        }
    }

    public static class SyncWorldLoggerExt
    {
        public static SyncWorldLogger GetSyncLogger(this EcsWorld world)
        {
            return WorldLoggerExt.logger as SyncWorldLogger;
        }
    }
}