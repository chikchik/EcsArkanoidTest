using Fabros.P2P;
using UnityEngine;

namespace Game.ClientServer
{
    public static class Config
    { 
        public static readonly string DEFAULT_ROOM_A = "sandbox_";
        public static readonly string DEFAULT_ROOM_B = P2P.GetDevRoom();
        
#if CLIENT
        public static string SYNC_LOG_PATH = "../client.log"; 
        public static string TMP_HASHES_PATH = "../tmp";
        //public static string OTHER_LOGS_PATH = "../tmp2";
#else
        public static string SYNC_LOG_PATH = "../server.log";
        public static string TMP_HASHES_PATH = "../tmp";
        //public static string OTHER_LOGS_PATH = "../tmp2";
#endif

#if DEBUG
        public static readonly bool SyncDataLogging = false;
#else
        public static readonly bool SyncDataLogging = false;
#endif

        public static string ROOM_A = DEFAULT_ROOM_A;
        public static string ROOM_B = DEFAULT_ROOM_B;


        //public static string url = $"wss://dev1.ecs.fbpub.net/XsZubnMOTHC0JRDTS95S/{ROOM}";
        //public static string url = $"ws://localhost:9096/XsZubnMOTHC0JRDTS95S/{ROOM}";
        public static string url => $"ws://dev1.ecs.fbpub.net:9096/XsZubnMOTHC0JRDTS95S/{ROOM_A}{ROOM_B}";
        
        
#if UNITY_EDITOR //unity editor domain reload feature
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void _reset()
        {
                ROOM_A = "sandbox_";
                ROOM_B = P2P.GetDevRoom();
        }
#endif
    }
}