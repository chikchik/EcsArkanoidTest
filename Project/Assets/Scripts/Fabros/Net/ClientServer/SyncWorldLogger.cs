using Fabros.Ecs.ClientServer.Utils;
using Flow.EcsLite;
using Game.ClientServer;
using System;
using System.IO;

namespace Game.Fabros.Net.ClientServer
{
    public class SyncWorldLogger : IWorldLogger
    {
        struct LoggerState
        {
            public int tick;
            public bool inTick;
        }

        public void Log(EcsWorld world, string str)
        {
            var state = GetState(world);
            var offset = state.inTick ? "    " : "";
            var txt = $"{offset}{str}";
            LogRaw(world, txt);
        }
        
        private void LogRaw(EcsWorld world, string str)
        {
#if true
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
            //var b2 = world.GetUnique<Box2DWorldComponent>().WorldReference.ToInt64();
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