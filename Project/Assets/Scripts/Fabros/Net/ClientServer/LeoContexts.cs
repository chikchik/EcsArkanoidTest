using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fabros.Ecs;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
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


        public void FilterInputs(Tick time)
        {
            Inputs = Inputs.Where(input => input.time >= time).ToList();
        }


        public void Tick(EcsSystems systems, EcsWorld inputWorld, EcsWorld world, UserInput[] inputs, bool writeToLog)
        {
            //обновляем мир 1 раз
            
            var time = GetCurrentTick(world);
            if (writeToLog) SyncLog.WriteLine($"tick {time.Value} ->");

            ProcessUserInput(inputWorld, world, inputs);


            //тик мира
            systems.ChangeDefaultWorld(world);
            systems.Run();


            if (!writeToLog)
                return;

            time = GetCurrentTick(world);

            //отладочный код, который умеет писать в файлы hash мира и его diff
            var empty = WorldUtils.CreateWorld("empty", Pool);
            var dif = WorldUtils.BuildDiff(Pool, empty, world);

            var str = JsonUtility.ToJson(dif, true);
            var hash = global::Fabros.Ecs.Utils.Utils.CreateMD5(str);


            SyncLog.WriteLine($"<- tick {time.Value}\n");
            //SyncLog.WriteLine($"hash: {hash}\n");

            if (writeHashes)
                using (var file = new StreamWriter($"{hashDir}/{hash}.txt"))
                {
                    file.Write(str);
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