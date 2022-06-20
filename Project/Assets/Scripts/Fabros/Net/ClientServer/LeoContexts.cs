using System;
using System.IO;
using System.Text;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Tick.Components;
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
        public ComponentsCollection Components { get; }

        //compatibility with old code
        public ComponentsCollection Pool => Components;
        /*
         * позволяет сохранять в отдельный файл игровые данные и сравнивать состояния миров между собой
        */
        public SyncLog SyncLog { get; }
        
        
        private readonly string hashDir;
        private readonly bool writeHashes;
        private EcsWorld inputWorld;

        public LeoContexts(string hashDir, ComponentsCollection components, SyncLog syncLog, EcsWorld inputWorld)
        {
            this.hashDir = hashDir;
            this.inputWorld = inputWorld;

            Components = components;
            SyncLog = syncLog;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            writeHashes = Directory.Exists(hashDir);
            if (writeHashes && (hashDir.Contains("temp") || hashDir.Contains("tmp")))
            {
                var files = Directory.GetFiles(hashDir);
                Array.ForEach(files, file => {
                    File.Delete(file);
                });
            }
#endif
        }


        public static void InitNewWorld(EcsWorld world, TickrateConfigComponent cfg)
        {
            world.AddUnique(cfg);
            world.AddUnique<TickComponent>().Value = new Tick(0);
            //world.AddUnique<PendingInputComponent>().data = new UserInput[0];
        }

        
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public void FilterInputs(Tick time)
        {
            var filter = inputWorld.Filter<InputTickComponent>().End();
            var pool = inputWorld.GetPool<InputTickComponent>();
            foreach (var entity in filter)
            {
                var tick = pool.Get(entity).Tick;
                if (tick < time.Value)
                    inputWorld.DelEntity(entity);
            }
        }

        public void Tick(EcsSystems systems, EcsWorld inputWorld, EcsWorld world, bool writeToLog, string debug="")
        {
            //обновляем мир 1 раз
            
            var currentTick = GetCurrentTick(world);
            if (writeToLog) 
                SyncLog.WriteLine($"tick {currentTick.Value} ->");

            //ProcessUserInput(inputWorld, world, inputs);

            
            var strStateDebug = "";
            if (writeHashes)
            {
                strStateDebug += WorldDumpUtils.DumpWorld(Components, inputWorld);
                strStateDebug += "\n\n\n";
            }
            
            
            //тик мира
            systems.ChangeDefaultWorld(world);
            systems.Run();


            if (!writeToLog)
                return;

            currentTick = GetCurrentTick(world);

            //отладочный код, который умеет писать в файлы hash мира и его diff
            //var empty = WorldUtils.CreateWorld("empty", Pool);
            //var dif = WorldUtils.BuildDiff(Pool, empty, world, true);
            
            
            
            if (writeToLog)
                SyncLog.WriteLine($"<- tick {currentTick.Value}\n");

            if (writeHashes)
            {
                //var str = JsonUtility.ToJson(dif, true);
                var strWorldDebug = WorldDumpUtils.DumpWorld(Components, world);
                var hash = CreateMD5(strWorldDebug);

                //SyncLog.WriteLine($"hash: {hash}\n");

                var str = strStateDebug + "\n>>>>>>\n" +  strWorldDebug;
                var tick = currentTick.Value.ToString("D4");

                //var tt = DateTime.UtcNow.Ticks % 10000000;
                
                using (var file = new StreamWriter($"{hashDir}/{hash}-{tick}-{world.GetDebugName()}-{debug}.txt"))
                {
                    file.Write(str);
                }
                
                using (var file = new StreamWriter($"{hashDir}/{tick}-{world.GetDebugName()}-{hash}-{debug}.txt"))
                {
                    file.Write(str);
                }
            }
        }

        public Tick GetCurrentTick(EcsWorld w)
        {
            return w.GetUnique<TickComponent>().Value;
        }

        
        public Tick GetPrevTick(EcsWorld w)
        {
            return w.GetUnique<TickComponent>().Value - w.GetUnique<TickDeltaComponent>().Value;
        }


        public TickrateConfigComponent GetConfig(EcsWorld world)
        {
            //конфиг сервера
            return world.GetUnique<TickrateConfigComponent>();
        }
    }
}