using System;
using Cysharp.Threading.Tasks;
using Fabros.Ecs;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client.Socket;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Fabros.Net.Client
{
    public class NetClient
    {
        private readonly LeoContexts leo;
        private readonly int playerID;
        private EcsSystems clientSystems;
        private EcsWorld mainWorld;

        public Action<EcsWorld, int[]> DeleteEntities;
        private int diffSize;
        public Action<EcsWorld> InitWorld;
        private EcsWorld inputWorld;
        private int lags;

        private int lastClientTick;
        private int lastReceivedServerTick;

        private float lastServerTime;
        public Action<EcsWorld> LinkUnits;

        private int prevDelay;
        private EcsSystems serverSystems;

        private EcsWorld serverWorld;

        private UnitySocket socket;

        private float stepMult = 1f;
        private float stepOffset = 0.001f;


        
        

        public EcsWorld MainWorld => mainWorld; 
        public LeoContexts Leo => leo;

        public bool Connected { get; private set; }
        
        
        public NetClient(EcsWorld world)
        {
            Application.targetFrameRate = 60;

            var pool = SystemsAndComponents.CreateComponentsPool();

            leo = new LeoContexts(Config.TMP_HASHES_PATH,
                pool,
                new SyncLog(Config.SYNC_LOG_PATH),
                InputService.ApplyInput);


            //генерируем случайный id игрока с которым нас будет ассоциировать сервер
            playerID = Random.Range(1000, 9999);
            
            //создаем пустой мир
            //mainWorld = WorldUtils.CreateWorld("current", leo.Pool);
            //leo.Pool.SetupPools();
            mainWorld = world;
            mainWorld.AddUnique<ClientWorldComponent>();
            mainWorld.AddUnique<MainPlayerIdComponent>().value = playerID;
            mainWorld.SetEventsEnabled<PlayerComponent>();
            
            
        }


        private float lastUpdateTime { get; set; }

        public void Start()
        {
            //произвольный код для подключения к серверу
            //особой важности не имеет после подкл к сокету вызывается asyncmain
            var hello = new Packet {hello = new Hello {text = "hello"}, playerID = playerID, hasHello = true};

            var connection = new WebSocketConnection(hello, Config.ROOM, playerID.ToString(), P2P.ADDR_SERVER);
            connection.OnConnected += () =>
            {
                socket = connection.ExtractSocket();
                AsyncMain(connection.response);
            };
            connection.Start();
        }

        private async UniTask<string> AsyncMain(Packet packet)
        {
            while (!packet.hasWelcomeFromServer)
            {
                var msg = await socket.AsyncWaitMessage();
                packet = WebSocketConnection.Packet2Message(msg);
            }

            if (!Application.isPlaying)
                throw new Exception("async next step for stopped application");

            //получили состояние мира с сервера
            var dif = packet.WorldUpdate.dif;

            

            inputWorld = new EcsWorld("input");
            InitWorld(mainWorld);

            clientSystems = new EcsSystems(mainWorld);
            clientSystems.AddWorld(inputWorld, "input");
            SystemsAndComponents.AddSystems(leo.Pool, clientSystems, true);

            WorldUtils.ApplyDiff(leo.Pool, mainWorld, dif);
            
            mainWorld.AddUnique<TickDeltaComponent>() = new TickDeltaComponent
                {Value = new TickDelta(1, mainWorld.GetUnique<TickrateConfigComponent>().clientTickrate)};
            
            clientSystems.Init();

            
            serverWorld = WorldUtils.CopyWorld(leo.Pool, mainWorld);
            serverWorld.AddUnique<TickDeltaComponent>() = mainWorld.GetUnique<TickDeltaComponent>();
            
            
            serverSystems = new EcsSystems(serverWorld);
            serverSystems.AddWorld(inputWorld, "input");
            SystemsAndComponents.AddSystems(leo.Pool, serverSystems, false);
            serverSystems.Init();

            Debug.Log($"world\n{LeoDebug.e2s(mainWorld)}");

            
            Connected = true;
            Debug.Log("client started");

            try
            {
                while (true)
                {
                    var msg = await socket.AsyncWaitMessage();
                    packet = WebSocketConnection.Packet2Message(msg);

                    if (!Application.isPlaying) throw new Exception("async next step for stopped application");

                    diffSize = msg.buffer.Length;

                    //if (Input.GetKey(KeyCode.A))
                    //     break;

                    dif = packet.WorldUpdate.dif;

                    /*
                 * если клиент создал сам entity с такими же id, то их надо удалить прежде чем применять dif
                 * иначе может получиться так, что останется висеть какой-то чужой view component 
                 */
                    dif.CreatedEntities.ForEach(entity =>
                    {
                        if (!mainWorld.IsEntityAliveInternal(entity))
                            return;
                        if (entity.EntityHasComponent<GameObjectComponent>(mainWorld))
                        {
                            var go = entity.EntityGetComponent<GameObjectComponent>(mainWorld).GameObject;
                            Object.Destroy(go);
                        }

                        if (entity.EntityHasComponent<FireViewComponent>(mainWorld))
                        {
                            var go = entity.EntityGetComponent<FireViewComponent>(mainWorld).view.gameObject;
                            Object.Destroy(go);
                        }

                        mainWorld.DelEntity(entity);
                    });


                    var delay = packet.WorldUpdate.delay;

                    //применяем diff к прошлому миру полученному от сервера
                    WorldUtils.ApplyDiff(leo.Pool, serverWorld, dif);

                    //Debug.Log($"world\n{LeoDebug.e2s(serverWorld)}");

                    //удаляем гарантированно устаревший ввод от игрока
                    leo.FilterInputs(leo.GetCurrentTick(serverWorld) - 10);

                    lastClientTick = leo.GetCurrentTick(mainWorld).Value;
                    lastReceivedServerTick = leo.GetCurrentTick(serverWorld).Value;
                    //проматываем в будущее серверный мир


                    //if (leo.GetCurrentTick(serverWorld) < leo.GetCurrentTick(currentWorld))
                    if (delay != -999)
                    {
                        var delayDir = delay - prevDelay;

                        if (delay >= 2) stepOffset = 0.001f * delay;

                        if (delay < 0)
                        {
                            stepOffset = 0.001f * delay;
                            lags++;
                        }

                        prevDelay = delay;
                    }

                    //stepOffset = 0;
                    stepMult = 1;

                    var iterations = 0;

                    leo.SyncLog.WriteLine("sync begin");
                    var copyServerWorld = WorldUtils.CopyWorld(leo.Pool, serverWorld);
                    copyServerWorld.AddUnique<TickDeltaComponent>() = mainWorld.GetUnique<TickDeltaComponent>();

                    var serverTick = leo.GetCurrentTick(copyServerWorld);
                    var clientTick = leo.GetCurrentTick(mainWorld);
                    while (leo.GetCurrentTick(copyServerWorld) < leo.GetCurrentTick(mainWorld))
                    {
                        leo.Tick(serverSystems, inputWorld, copyServerWorld, leo.Inputs.ToArray(), false);
                        if (iterations > 50)
                        {
                            Debug.LogWarning(
                                $"too much iterations {serverTick} -> {clientTick}, {clientTick - serverTick}");
                            break;
                        }

                        iterations++;
                    }

                    leo.SyncLog.WriteLine("sync end\n");

                    var dif2 = WorldUtils.BuildDiff(leo.Pool, mainWorld,
                        copyServerWorld);

                    DeleteEntities(mainWorld, dif2.RemovedEntities);
                    WorldUtils.ApplyDiff(leo.Pool, mainWorld, dif2);
                    //перепривязываем юнитов
                    LinkUnits(mainWorld);

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
            return mainWorld;
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
            
            var dt = Leo.GetConfig(mainWorld).clientTickrate / Leo.GetConfig(mainWorld).serverTickrate;
            var a = Leo.GetCurrentTick(mainWorld).Value / dt + 1;
            var tick = new Tick(a * dt);
            return tick;
        }

        public LeoContexts GetContexts()
        {
            return leo;
        }

        public void Update()
        {
            if (!Connected)
                return;

            //выполняем один тик и применяем инпуты

            var deltaTime = mainWorld.GetDeltaSeconds();
            deltaTime *= stepMult;
            deltaTime += stepOffset;

            Tick(deltaTime, () =>
            {
                //leo.ApplyUserInput(world);
                leo.Tick(clientSystems, inputWorld, mainWorld, leo.Inputs.ToArray(), Config.SyncDataLogging);


                if (leo.GetCurrentTick(mainWorld).Value % 5 == 0)
                {
                    //ping
                    var packet = new Packet();
                    packet.playerID = playerID;
                    packet.input = new UserInput();
                    packet.isPing = true;
                    packet.input.time = leo.GetCurrentTick(mainWorld);

                    var body = JsonUtility.ToJson(packet);
                    socket.Send(P2P.ADDR_SERVER, body);
                }
            });

            //stepMult = 1;
            //stepOffset = 0;
        }

        public void AddUserInput(UserInput input)
        {
            var packet = new Packet
            {
                input = input
            };
            packet.playerID = playerID;
            
            leo.Inputs.Add(input);
            
            var body = JsonUtility.ToJson(packet);
            socket.Send(P2P.ADDR_SERVER, body);
            if (packet.input != null)
                leo.SyncLog.WriteLine($"send input {packet.input.time}");
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

                if (iteration > 100)
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
            GUILayout.Label($"entities {mainWorld.GetAllEntitiesCount()}");
            GUILayout.Label($"playerID {playerID}");
            //GUILayout.Label($"future {futureTicks}");
            GUILayout.Label($"lags {lags}");
            GUILayout.Label($"stepOffset {stepOffset}");
            GUILayout.Label($"stepMult {stepMult}");
            GUILayout.Label($"currentWorldTick {leo.GetCurrentTick(mainWorld)}");
            GUILayout.Label($"serverWorldTick {leo.GetCurrentTick(serverWorld)}");

            GUILayout.Label($"lastReceivedServerTick {lastReceivedServerTick}");
            GUILayout.Label($"lastClientTick {lastClientTick}");
            GUILayout.Label($"delta {lastClientTick - lastReceivedServerTick}");
            GUILayout.Label($"prevDelay {prevDelay}");

            GUILayout.Label($"diffSize {diffSize}");

            //WorldMono.OnGui(currentWorld);
            GUILayout.EndVertical();
        }
    }
}