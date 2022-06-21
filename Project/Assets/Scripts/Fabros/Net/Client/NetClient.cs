using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.Client.Systems;
using Fabros.EcsModules.Box2D.ClientServer;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.Client.Socket;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Ecs.Systems;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Fabros.Net.Client
{
    public class NetClient
    {
        public Action<EcsWorld, List<int>> DeleteEntitiesAction;
        public Action<EcsWorld> InitWorldAction;
        public Action ConnectedAction;


        public EcsWorld MainWorld { get; }
        public EcsWorld InputWorld { get; }
        public EcsWorld ServerWorld { get; private set; }

        public LeoContexts Leo { get; }

        public bool Connected { get; private set; }


        private float lastUpdateTime { get; set; }
        private EcsSystems clientSystems;
        private EcsSystems serverSystems;
        
        private readonly int playerID;

        private Stats stats = new Stats();
        
        public UnitySocket Socket { get; private set; }

        private float stepMult = 1f;
        private float stepOffset = 0.001f;
        

        private IEcsSystemsFactory systemsFactory;

        private HGlobalWriter writer = new HGlobalWriter();

        public NetClient(
            EcsWorld world, 
            [Inject (Id = "input")]EcsWorld inputWorld,
            ComponentsCollection pool, 
            IEcsSystemsFactory systemsFactory)
        {
            Application.targetFrameRate = 60;

            this.InputWorld = inputWorld;
            
            this.systemsFactory = systemsFactory;

            Leo = new LeoContexts(Config.TMP_HASHES_PATH,
                pool,
                new SyncLog(Config.SYNC_LOG_PATH), inputWorld);


            //генерируем случайный id игрока с которым нас будет ассоциировать сервер
            playerID = Random.Range(1000, 9999);

            MainWorld = world;
            /*
             * это нужно чтоб GEN на клиенте у entity отличался от серверного и в в момент применении дифа удалялся,
             * а не считался своим из-за совпадающих Gen с серверным
             */
            MainWorld.SetDefaultGen(15000);
            MainWorld.AddUnique<MainPlayerIdComponent>().value = playerID;
            
            stats.delaysHistory.Add(0);
            stats.delaysHistory.Add(0);
            stats.delaysHistory.Add(0);
        }


        public void Start()
        {
            //произвольный код для подключения к серверу
            //особой важности не имеет после подкл к сокету вызывается asyncmain
            var hello = new Packet {hello = new Hello {Text = "hello"}, playerID = playerID, hasHello = true};

            var url = $"{Config.url}/{playerID}";
            var connection = new WebSocketConnection(hello, url, P2P.ADDR_SERVER);
            connection.OnConnected += () =>
            {
                Socket = connection.ExtractSocket();
                AsyncMain(connection.Response);
            };
            connection.Start();
        }
        
        public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                return;
            foreach (var item in source) 
                action(item);
        }

        private void Sim(int delay)
        {
            Profiler.BeginSample("NetClientSim");
            //Debug.Log($"world\n{LeoDebug.e2s(serverWorld)}");

            //удаляем гарантированно устаревший ввод от игрока
            Leo.FilterInputs(Leo.GetCurrentTick(ServerWorld) - 10);

            stats.lastClientTick = Leo.GetCurrentTick(MainWorld).Value;
            stats.lastReceivedServerTick = Leo.GetCurrentTick(ServerWorld).Value;
            //проматываем в будущее серверный мир


            //if (leo.GetCurrentTick(serverWorld) < leo.GetCurrentTick(currentWorld))
            
            
            if (delay != -999)
            {
                var prevDelay1 = stats.delaysHistory[stats.delaysHistory.Count - 1];
                var prevDelay2 = stats.delaysHistory[stats.delaysHistory.Count - 2];
                var prevDelay3 = stats.delaysHistory[stats.delaysHistory.Count - 2];

                if (delay > 0)
                {
                    //игра опережает сервер
                    if (delay > 1)
                    {
                        //delay=1 не считаем за лаг, это ок
                        stats.oppLags++;
                    }

                    stepOffset = 0.001f * delay;
                    
                    
                    if (delay == 1)
                    {
                        stepOffset = 0;
                    }
                }

                if (delay < 0 )
                {
                    //инпут клиента отстает от сервера
                    stepOffset = 0.001f * delay;

                    if (delay == -1 && prevDelay1 == 0 && prevDelay2 == 0 && prevDelay3 == 0)
                    {
                        //микролаг?
                        stepOffset /= 2;
                    }

                    stats.lags++;
                }

                if (delay == 0)
                {
                    //все равно чуть ускорим клиента, чтоб delay стремился быть > 0  и до 1 доходил чаще чем до -1
                    stepOffset = -0.0005f;
                }
                
                
                stats.delaysHistory.Add(delay);
                if (stats.delaysHistory.Count > Stats.HISTORY_LEN)
                    stats.delaysHistory.RemoveAt(0);
            }

            //stepOffset = 0;
            stepMult = 1;

            var iterations = 0;

            Leo.SyncLog.WriteLine("sync begin");
            
            
            var copyServerWorld = WorldUtils.CopyWorld(Leo.Pool, ServerWorld);
            copyServerWorld.SetDebugName("copy");
            copyServerWorld.AddUnique<TickDeltaComponent>() = MainWorld.GetUnique<TickDeltaComponent>();
       
            Box2DServices.ReplicateBox2D(ServerWorld, copyServerWorld);
            
            var copyServerSystems = new EcsSystems(copyServerWorld);
            copyServerSystems.AddWorld(InputWorld, "input");
            
            systemsFactory.AddNewSystems(copyServerSystems,
                new IEcsSystemsFactory.Settings{client = false, server = false});
            
            copyServerSystems.Init();
            
            

            var serverTick = Leo.GetCurrentTick(copyServerWorld);
            var clientTick = Leo.GetCurrentTick(MainWorld);
            
            string debug = $"{ServerWorld.GetUnique<TickComponent>().Value.Value}";
            
            //Debug.Log($"pr srv:{Leo.GetCurrentTick(ServerWorld).Value} client: {Leo.GetCurrentTick(MainWorld).Value}");
            Profiler.BeginSample("SimServerWorld");
            stats.simTicksTotal = 0;
            while (Leo.GetCurrentTick(copyServerWorld) < Leo.GetCurrentTick(MainWorld))
            {
                Leo.Tick(copyServerSystems, InputWorld, copyServerWorld, Config.SyncDataLogging, debug);
                stats.simTicksTotal++;
                if (iterations > 500)
                {
                    Debug.LogWarning(
                        $"too much iterations {serverTick} -> {clientTick}, {clientTick - serverTick}");
                    break;
                }

                iterations++;
            }
            Profiler.EndSample();
            //Debug.Log("pr end");
            
            

            Leo.SyncLog.WriteLine("sync end\n");

            var dif2 = WorldDiff.BuildDiff(Leo.Pool, MainWorld, copyServerWorld);
            
            DeleteEntitiesAction(MainWorld, dif2.RemovedEntities);
            
            if (copyServerWorld.GetTick() != MainWorld.GetTick())
                Debug.LogError($"ticks not equal {copyServerWorld.GetTick()} != {MainWorld.GetTick()}");
            
            dif2.ApplyChanges(MainWorld);
            
            Profiler.BeginSample("replicate2");
            Box2DServices.ReplicateBox2D(copyServerWorld, MainWorld);
            Profiler.EndSample();
            
            Box2DDebugViewSystem.ReplaceBox2D(MainWorld);
            
            
            copyServerSystems.Destroy();
            Profiler.EndSample();
        }
        private async UniTask<string> AsyncMain(Packet packet)
        {
            Leo.Pool.RemapOrder(packet.hello.Components);
            while (!packet.hasWelcomeFromServer)
            {
                var msg = await Socket.AsyncWaitMessage();
                packet = P2P.ParseResponse<Packet>(msg.buffer);
            }
            
            
                
            if (!Application.isPlaying)
                throw new Exception("async next step for stopped application");

            //получили состояние мира с сервера
            var dif = WorldDiff.FromJsonString(Leo.Pool, packet.WorldUpdate.difStr);


            //InputWorld = new EcsWorld("input");
            InitWorldAction(MainWorld);

            clientSystems = new EcsSystems(MainWorld);
            clientSystems.AddWorld(InputWorld, "input");
            
            systemsFactory.AddNewSystems(clientSystems, new IEcsSystemsFactory.Settings{client = true, server = false});

            dif.ApplyChanges(MainWorld);

            MainWorld.AddUnique<TickDeltaComponent>() = new TickDeltaComponent
                {Value = new TickDelta(1, MainWorld.GetUnique<TickrateConfigComponent>().clientTickrate)};

            clientSystems.Init();


            ServerWorld = WorldUtils.CopyWorld(Leo.Pool, MainWorld);
            ServerWorld.SetDebugName("rsrv");
            ServerWorld.AddUnique<TickDeltaComponent>() = MainWorld.GetUnique<TickDeltaComponent>();


            serverSystems = new EcsSystems(ServerWorld);
            serverSystems.AddWorld(InputWorld, "input");
            serverSystems.Add(new Box2DSystem(Config.POSITION_ITERATIONS, Config.VELOCITY_ITERATIONS, 
                new Vector2(0,0), true, false, true));
            
            serverSystems.Add(new Box2DWriteStateToComponentsSystem());
            //после того как нам пришло что-то от сервера старый инпут можно спокойно удалять
            serverSystems.Add(new DeleteOutdatedInputEntitiesSystem());

            //SystemsAndComponents.AddSystems(Leo.Pool, serverSystems, false, false);
            serverSystems.Init();

            Debug.Log($"world\n{LeoDebug.e2s(MainWorld)}");


            Connected = true;
            Debug.Log("client started");

            ConnectedAction?.Invoke();

            lastUpdateTime = Time.realtimeSinceStartup;
            
           // for (int i = 0; i < 300; ++i)
            //    Leo.Tick(clientSystems, InputWorld, MainWorld, Leo.Inputs.ToArray(), Config.SyncDataLogging);
            
            try
            {
                while (true)
                {
                    int delay = 0;
                    bool updated = false;
                    while (true)
                    {
                        var msg = Socket.PopMessage();
                        if (msg == null)
                        {
                            break;
                        }

                        updated = true;
                        packet = P2P.ParseResponse<Packet>(msg.buffer);

                        if (!Application.isPlaying) throw new Exception("async next step for stopped application");

                        //stats.diffSize = msg.buffer.Length;

                        //if (Input.GetKey(KeyCode.A))
                        //     break;

                        var byteArrayDifCompressed = Convert.FromBase64String(packet.WorldUpdate.difBinary);
                        stats.diffSize = byteArrayDifCompressed.Length;
                        var byteArrayDif = P2P.Decompress(byteArrayDifCompressed);
                        dif = WorldDiff.FromByteArray(Leo.Pool, byteArrayDif);

                        /*
                         * если клиент создал сам entity с такими же id, то их надо удалить прежде чем применять dif
                         * иначе может получиться так, что останется висеть какой-то чужой view component 
                         */
                        ForEach(dif.CreatedEntities, entity =>
                        {
                            if (!MainWorld.IsEntityAliveInternal(entity))
                                return;

                            if (entity.EntityHas<LocalEntityComponent>(MainWorld))
                                return;

                            if (entity.EntityHasComponent<TransformComponent>(MainWorld))
                            {
                                var go = entity.EntityGetComponent<TransformComponent>(MainWorld).Transform.gameObject;
                                Object.Destroy(go);
                            }

                            MainWorld.DelEntity(entity);
                        });


                        delay = packet.WorldUpdate.delay;

                        //применяем diff к прошлому миру полученному от сервера
                        dif.ApplyChanges(ServerWorld);
                        serverSystems.Run();
                    }

                    if (updated)
                        Sim(delay);
                    //else

                    await UniTask.WaitForEndOfFrame();
                    //WorldMono.log.write($"got update from server, {leo.getTime(world)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            await UniTask.WaitForEndOfFrame();
            return "";
        }


        public EcsWorld GetWorld()
        {
            return MainWorld;
        }

        public int GetPlayerID()
        {
            return playerID;
        }

        public Tick GetNextInputTick()
        {
            //эти расчеты имеют смысл когда на клиенте и сервере разный tickrate
            //например 20 на сервере, 60 на клиенте
            //тогда сервер за один свой апдейт прибавляет +60/20 = +3 тика
            //клиент же по +1
            //потому надо сделать snap значения ввода чтоб оно попало на корректный серверный тик


            return Leo.GetCurrentTick(MainWorld);
            var dt = Leo.GetConfig(MainWorld).clientTickrate / Leo.GetConfig(MainWorld).serverTickrate;
            var a = Leo.GetCurrentTick(MainWorld).Value / dt + 1;
            var tick = new Tick(a * dt);
            return tick;
        }

        public LeoContexts GetContexts()
        {
            return Leo;
        }

        public void Update()
        {
            if (!Connected)
                return;

            //выполняем один тик и применяем инпуты

            var deltaTime = MainWorld.GetDeltaSeconds();
            deltaTime *= stepMult;
            deltaTime += stepOffset;

            Tick(deltaTime, () =>
            {
                //если давно ничего не приходило от сервера, то и обновлять игру уже нет смысла
                //if (Leo.GetCurrentTick(MainWorld).Value > stats.lastReceivedServerTick + 200)
                //    return;
                    
                //leo.ApplyUserInput(world);
                SendPing(Leo.GetCurrentTick(MainWorld).Value);
                Leo.Tick(clientSystems, InputWorld, MainWorld, Config.SyncDataLogging, "");
                
            });

            //stepMult = 1;
            //stepOffset = 0;
        }

        private void SendPing(int currentTick)
        {
            if (currentTick % 5 != 0)
                return;
            
            writer.Reset();
            writer.WriteByteArray(P2P.ADDR_SERVER.Address, false);
            writer.WriteInt32(0xff);
            writer.WriteInt32(playerID);
            writer.WriteInt32(currentTick);
            writer.WriteInt32(Leo.Pool.GetComponent(typeof(PingComponent)).GetId());
                
            Socket.Send(writer.CopyToByteArray());
        }


        public void OnDestroy()
        {
            Leo?.SyncLog?.Close();
        }

        public void Tick(float deltaTime, Action action)
        {
            //хелпер чтоб не обновляться чаще раз в секунду чем заданный tickrate
            var iteration = 0;
            while (true)
            {
                var tm = Time.unscaledTime;
                if (lastUpdateTime + deltaTime > tm)
                    break;

                if (iteration > 200)
                {
                    Debug.LogWarning("too much iterations");
                    break;
                }

                lastUpdateTime += deltaTime;
                action();
                iteration++;
            }
        }

        public void AddPlayerInput(IInputComponent inputComponent)
        {
            /*
            var input = new UserInput();
            input.Tick = GetNextInputTick();
            input.PlayerID = playerID;
            input.Component = inputComponent;
            
            if (Leo.Inputs.Count > 100)
            {
                throw new Exception("wtf leo size");
            }
            
            Leo.Inputs.Add(input);
            */
        }

        public void OnGUI()
        {
            if (!Connected)
                return;

            GUILayout.BeginVertical();
            GUILayout.Label($"main entities {MainWorld.GetAllEntitiesCount()}");
            GUILayout.Label($"input entities {InputWorld.GetAllEntitiesCount()}");
            GUILayout.Label($"playerID {playerID}");
            //GUILayout.Label($"future {futureTicks}");
            GUILayout.Label($"lags {stats.lags} vs {stats.oppLags}");
            GUILayout.Label($"stepOffset {stepOffset}");
            GUILayout.Label($"stepMult {stepMult}");
            GUILayout.Label($"currentWorldTick {Leo.GetCurrentTick(MainWorld)}");
            GUILayout.Label($"serverWorldTick {Leo.GetCurrentTick(ServerWorld)}");

            GUILayout.Label($"lastReceivedServerTick {stats.lastReceivedServerTick}");
            GUILayout.Label($"lastClientTick {stats.lastClientTick}");
            GUILayout.Label($"delta {stats.lastClientTick - stats.lastReceivedServerTick}");
            GUILayout.Label($"simTicksTotal {stats.simTicksTotal}");


            var history = "";
            foreach (var i in stats.delaysHistory)
            {
                history += $"{i.ToString()}".PadLeft(4);
            }
            
            GUILayout.Label($"history {history}");

            GUILayout.Label($"diffSize {stats.diffSize}");

            var size = MainWorld.GetAllocMemorySizeInBytes() / 1024;
                
            GUILayout.Label($"EcsWorld size {size} kb");

            //WorldMono.OnGui(currentWorld);
            GUILayout.EndVertical();
        }
    }
}