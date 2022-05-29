using System;
using Cysharp.Threading.Tasks;
using Fabros.Ecs;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
using Game.ClientServer.Box2D;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Systems.Physics;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client.Socket;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Fabros.Net.Client
{
    public class NetClient
    {
        public Action<EcsWorld, int[]> DeleteEntitiesAction;
        public Action<EcsWorld> InitWorldAction;
        public Action ConnectedAction;


        public EcsWorld MainWorld { get; }
        public EcsWorld InputWorld { get; private set; }
        public EcsWorld ServerWorld { get; private set; }

        public LeoContexts Leo { get; }

        public bool Connected { get; private set; }


        private float lastUpdateTime { get; set; }
        private EcsSystems clientSystems;
        private EcsSystems serverSystems;
        
        private readonly int playerID;

        private int prevDelay;

        private Stats stats = new Stats();

        private UnitySocket socket;

        private float stepMult = 1f;
        private float stepOffset = 0.001f;


        public NetClient(EcsWorld world)
        {
            Application.targetFrameRate = 60;

            var pool = SystemsAndComponents.CreateComponentsPool();

            Leo = new LeoContexts(Config.TMP_HASHES_PATH,
                pool,
                new SyncLog(Config.SYNC_LOG_PATH), (inputWorld, playerID, userInput) =>
                {
                    InputService.ApplyInput(inputWorld, playerID, userInput);
                });


            //генерируем случайный id игрока с которым нас будет ассоциировать сервер
            playerID = Random.Range(1000, 9999);

            MainWorld = world;
            MainWorld.AddUnique<MainPlayerIdComponent>().value = playerID;
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
                socket = connection.ExtractSocket();
                AsyncMain(connection.Response);
            };
            connection.Start();
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
                if (delay >= 2) 
                    stepOffset = 0.001f * delay;

                if (delay < 0)
                {
                    stepOffset = 0.001f * delay;
                    stats.lags++;
                }

                prevDelay = delay;
            }

            //stepOffset = 0;
            stepMult = 1;

            var iterations = 0;

            Leo.SyncLog.WriteLine("sync begin");
            
            
            var copyServerWorld = WorldUtils.CopyWorld(Leo.Pool, ServerWorld);
            copyServerWorld.AddUnique<TickDeltaComponent>() = MainWorld.GetUnique<TickDeltaComponent>();
       
            Box2DServices.ReplicateBox2D(ServerWorld, copyServerWorld);
            
            var copyServerSystems = new EcsSystems(copyServerWorld);
            copyServerSystems.AddWorld(InputWorld, "input");
            SystemsAndComponents.AddSystems(Leo.Pool, copyServerSystems, false, false);
            copyServerSystems.Init();
            
            

            var serverTick = Leo.GetCurrentTick(copyServerWorld);
            var clientTick = Leo.GetCurrentTick(MainWorld);
            
            Profiler.BeginSample("SimServerWorld");
            while (Leo.GetCurrentTick(copyServerWorld) < Leo.GetCurrentTick(MainWorld))
            {
                Leo.Tick(copyServerSystems, InputWorld, copyServerWorld, Leo.Inputs.ToArray(), false);
                if (iterations > 500)
                {
                    Debug.LogWarning(
                        $"too much iterations {serverTick} -> {clientTick}, {clientTick - serverTick}");
                    break;
                }

                iterations++;
            }
            Profiler.EndSample();
            
            

            Leo.SyncLog.WriteLine("sync end\n");

            var dif2 = WorldUtils.BuildDiff(Leo.Pool, MainWorld,
                copyServerWorld, false);

            if (dif2.RemovedEntities != null)
                DeleteEntitiesAction(MainWorld, dif2.RemovedEntities);
            WorldUtils.ApplyDiff(Leo.Pool, MainWorld, dif2);
            
            Profiler.BeginSample("replicate2");
            Box2DServices.ReplicateBox2D(copyServerWorld, MainWorld);
            Profiler.EndSample();
            
            copyServerSystems.Destroy();
            Profiler.EndSample();
        }
        private async UniTask<string> AsyncMain(Packet packet)
        {
            Leo.Pool.RemapOrder(packet.hello.Components);
            while (!packet.hasWelcomeFromServer)
            {
                var msg = await socket.AsyncWaitMessage();
                packet = P2P.ParseResponse<Packet>(msg.buffer);
            }
            
            
                
            if (!Application.isPlaying)
                throw new Exception("async next step for stopped application");

            //получили состояние мира с сервера
            var dif = packet.WorldUpdate.dif;


            InputWorld = new EcsWorld("input");
            InitWorldAction(MainWorld);

            clientSystems = new EcsSystems(MainWorld);
            clientSystems.AddWorld(InputWorld, "input");
            SystemsAndComponents.AddSystems(Leo.Pool, clientSystems, true, false);

            WorldUtils.ApplyDiff(Leo.Pool, MainWorld, dif);

            MainWorld.AddUnique<TickDeltaComponent>() = new TickDeltaComponent
                {Value = new TickDelta(1, MainWorld.GetUnique<TickrateConfigComponent>().clientTickrate)};

            clientSystems.Init();


            ServerWorld = WorldUtils.CopyWorld(Leo.Pool, MainWorld);
            ServerWorld.AddUnique<TickDeltaComponent>() = MainWorld.GetUnique<TickDeltaComponent>();


            serverSystems = new EcsSystems(ServerWorld);
            serverSystems.AddWorld(InputWorld, "input");
            serverSystems.Add(new Box2DSystem(true, true, true, true, false));

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
                        var msg = socket.PopMessage();
                        if (msg == null)
                        {
                            break;
                        }

                        updated = true;
                        packet = P2P.ParseResponse<Packet>(msg.buffer);

                        if (!Application.isPlaying) throw new Exception("async next step for stopped application");

                        stats.diffSize = msg.buffer.Length;

                        //if (Input.GetKey(KeyCode.A))
                        //     break;

                        dif = packet.WorldUpdate.dif;

                        /*
                         * если клиент создал сам entity с такими же id, то их надо удалить прежде чем применять dif
                         * иначе может получиться так, что останется висеть какой-то чужой view component 
                         */
                        dif.CreatedEntities.ForEach(entity =>
                        {
                            if (!MainWorld.IsEntityAliveInternal(entity))
                                return;

                            if (entity.EntityHas<LocalEntityComponent>(MainWorld))
                                return;

                            if (entity.EntityHasComponent<GameObjectComponent>(MainWorld))
                            {
                                var go = entity.EntityGetComponent<GameObjectComponent>(MainWorld).GameObject;
                                Object.Destroy(go);
                            }

                            if (entity.EntityHasComponent<FireViewComponent>(MainWorld))
                            {
                                var go = entity.EntityGetComponent<FireViewComponent>(MainWorld).view.gameObject;
                                Object.Destroy(go);
                            }

                            MainWorld.DelEntity(entity);
                        });


                        delay = packet.WorldUpdate.delay;

                        //применяем diff к прошлому миру полученному от сервера
                        WorldUtils.ApplyDiff(Leo.Pool, ServerWorld, dif);
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
                Leo.Tick(clientSystems, InputWorld, MainWorld, Leo.Inputs.ToArray(), Config.SyncDataLogging);


               // if (Leo.GetCurrentTick(MainWorld).Value % 5 == 0)
                {
                    //ping
                    var packet = new Packet();
                    packet.playerID = playerID;
                    packet.input = new UserInput();
                    packet.isPing = true;
                    
                    //packet.input.time = Leo.GetCurrentTick(ServerWorld) + prevDelay;
                    packet.input.time = Leo.GetCurrentTick(MainWorld);
                    
                    if (MainWorld.HasUnique<RootMotionComponent>())
                    {
                        packet.input.player = playerID;
                        packet.input.hasUnitPos = true;
                        packet.input.unitPos = MainWorld.GetUnique<RootMotionComponent>().Position;
                    }


                    var data = P2P.BuildRequest(P2P.ADDR_SERVER, packet);
                    socket.Send(data);
                }
            });

            //stepMult = 1;
            //stepOffset = 0;
        }

        public void AddUserInput(UserInput input)
        {
            input.time = GetNextInputTick();
            input.player = playerID;
            
            var packet = new Packet
            {
                input = input
            };
            packet.playerID = playerID;
            Leo.Inputs.Add(input);

            socket.Send( P2P.BuildRequest(P2P.ADDR_SERVER, packet));
            if (packet.input != null)
                Leo.SyncLog.WriteLine($"send input {packet.input.time}");
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

        public void OnGUI()
        {
            if (!Connected)
                return;

            GUILayout.BeginVertical();
            GUILayout.Label($"entities {MainWorld.GetAllEntitiesCount()}");
            GUILayout.Label($"playerID {playerID}");
            //GUILayout.Label($"future {futureTicks}");
            GUILayout.Label($"lags {stats.lags}");
            GUILayout.Label($"stepOffset {stepOffset}");
            GUILayout.Label($"stepMult {stepMult}");
            GUILayout.Label($"currentWorldTick {Leo.GetCurrentTick(MainWorld)}");
            GUILayout.Label($"serverWorldTick {Leo.GetCurrentTick(ServerWorld)}");

            GUILayout.Label($"lastReceivedServerTick {stats.lastReceivedServerTick}");
            GUILayout.Label($"lastClientTick {stats.lastClientTick}");
            GUILayout.Label($"delta {stats.lastClientTick - stats.lastReceivedServerTick}");
            GUILayout.Label($"prevDelay {prevDelay}");

            GUILayout.Label($"diffSize {stats.diffSize}");

            //WorldMono.OnGui(currentWorld);
            GUILayout.EndVertical();
        }

        public void OnDrawGizmos()
        {
            
        }
    }
}