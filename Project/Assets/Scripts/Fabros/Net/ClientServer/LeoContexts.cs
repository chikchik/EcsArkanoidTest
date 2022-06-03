using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Serializer;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer
{
    public class LeoContexts
    {
        public ComponentsPool Pool { get; }
        /*
         * позволяет сохранять в отдельный файл игровые данные и сравнивать состояния миров между собой
        */
        public SyncLog SyncLog { get; }
        public List<UserInput> Inputs { get; private set; } = new List<UserInput>();
        
        
        private readonly Action<EcsWorld, int, UserInput> applyInput;
        private readonly string hashDir;
        private readonly bool writeHashes;

        //private EcsWorld inputWorld;

        public LeoContexts(string hashDir, ComponentsPool pool, SyncLog syncLog,
            Action<EcsWorld, int, UserInput> applyInput)
        {
            this.hashDir = hashDir;
            this.applyInput = applyInput;

            Pool = pool;
            SyncLog = syncLog;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            writeHashes = Directory.Exists(hashDir);
#endif
        }


        public static void InitNewWorld(EcsWorld world, TickrateConfigComponent cfg)
        {
            world.AddUnique(cfg);
            world.AddUnique<TickComponent>().Value = new Tick(0);
            world.AddUnique<PendingInputComponent>().data = new UserInput[0];
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
            Inputs = Inputs.Where(input => input.time >= time).ToList();
        }

        private string DumpEntity(int entity, EcsWorld world)
        {
            var raw = world.GetRawEntities();
            
            var str = $"entity #{entity} gen={raw[entity].Gen}\n";
            object[] list = null;

            for (int i = 0; i < Pool.Components.Count; ++i)
            {
                var spool = Pool.Components[i];
                var pool = world.GetPoolByType(spool.GetComponentType());
                if (pool == null)
                    continue;
                var tp = spool.GetComponentType();
                var fields = tp.GetFields();
                
                if (!pool.Has(entity))
                    continue;
                    

                var component = pool.GetReadRaw(entity);
                var componentStr = "";
                for (int f = 0; f < fields.Length; ++f)
                {
                    var name = fields[f];
                    var field = tp.GetField(name.Name);
                    
                    var val = field.GetValue(component);
                    if (field.FieldType == typeof(float))
                    {
                        float fl = Convert.ToSingle(val);
                        val = fl.ToString("F2", CultureInfo.InvariantCulture.NumberFormat);
                    }

                    componentStr += $"    {name.Name} = {val}\n";
                }
                
                if (componentStr.Length > 1)
                {
                    componentStr = componentStr.Remove(componentStr.Length - 2, 2);
                }
                
                str += $"  {tp.Name}\n{componentStr}\n";
            }
            

            return str;
        }
        
        public string DumpWorld(EcsWorld world)
        {
            int[] entities = null;
            int count = world.GetAllEntities(ref entities);
            string str = "";
            for (int i = 0; i < count; ++i)
            {
                int entity = entities[i];
                str += $"{DumpEntity(entity, world)}\n\n";
            }

            return str;
        }

        public void Tick(EcsSystems systems, EcsWorld inputWorld, EcsWorld world, UserInput[] inputs, bool writeToLog)
        {
            //обновляем мир 1 раз
            
            var time = GetCurrentTick(world);
            if (writeToLog) SyncLog.WriteLine($"tick {time.Value} ->");

            ProcessUserInput(inputWorld, world, inputs);

            var strStateDebug = "";
            if (writeHashes)
            {
                strStateDebug += DumpWorld(inputWorld);
                strStateDebug += "\n\n\n";
            }
            
            
            //тик мира
            systems.ChangeDefaultWorld(world);
            systems.Run();


            if (!writeToLog)
                return;

            time = GetCurrentTick(world);

            //отладочный код, который умеет писать в файлы hash мира и его diff
            //var empty = WorldUtils.CreateWorld("empty", Pool);
            //var dif = WorldUtils.BuildDiff(Pool, empty, world, true);
            
            
            

            if (writeHashes)
            {
                //var str = JsonUtility.ToJson(dif, true);
                var strWorldDebug = DumpWorld(world);
                var hash = CreateMD5(strWorldDebug);


                SyncLog.WriteLine($"<- tick {time.Value}\n");
                //SyncLog.WriteLine($"hash: {hash}\n");

                var str = strStateDebug + "\n>>>>>>\n" +  strWorldDebug;
                var tick = time.Value.ToString("D4");
                
                using (var file = new StreamWriter($"{hashDir}/{hash}-{tick}-{world.GetDebugName()}.txt"))
                {
                    file.Write(str);
                }
                
                using (var file = new StreamWriter($"{hashDir}/{tick}-{world.GetDebugName()}-{hash}.txt"))
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

        public Tick GetNextTick(EcsWorld w)
        {
            return w.GetUnique<TickComponent>().Value + w.GetUnique<TickDeltaComponent>().Value;
        }

        public TickrateConfigComponent GetConfig(EcsWorld world)
        {
            //конфиг сервера
            return world.GetUnique<TickrateConfigComponent>();
        }

        public void ProcessUserInput(EcsWorld inputWorld, EcsWorld world, UserInput[] inputData = null)
        {
            if (inputData == null)
                inputData = Inputs.ToArray();

            if (inputData.Length == 0)
                return;

            //вызывается с клиента и сервера
            //и применяет ввод только в нужный тик

            var time = GetCurrentTick(world);
            var nextTick = time + world.GetUnique<TickDeltaComponent>().Value;
            for (var n = 0; n < inputData.Length; ++n)
            {
                var input = inputData[n];
                if (input.time < time)
                    continue;
                if (input.time >= nextTick)
                    break;                
                applyInput(inputWorld, input.player, input);
            }
        }
    }
}