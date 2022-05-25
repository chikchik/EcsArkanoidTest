using Fabros.P2P;
using UnityEngine;

namespace Game.ClientServer
{
    public static class Config
    {
#if CLIENT
        public static string SYNC_LOG_PATH = "../../../server.log";
        public static string TMP_HASHES_PATH = "../../../tmp";
#else
        public static string SYNC_LOG_PATH = "../../../client.log";
        public static string TMP_HASHES_PATH = "../tmp";
#endif

#if DEBUG
        public static readonly bool SyncDataLogging = true;
#else
        public static readonly bool SyncDataLogging = false;
#endif

        public static string ROOM = $"sandbox_{P2P.GetDevRoom()}";

        
        //Physics
        public static Vector2 GRAVITY = new Vector2(0, 0);
        public static int VELOCITY_ITERATIONS = 6;
        public static int POSITION_ITERATIONS = 2;

        //public static string url = $"wss://dev1.ecs.fbpub.net/XsZubnMOTHC0JRDTS95S/{ROOM}";
        //public static string url = $"ws://localhost:9096/XsZubnMOTHC0JRDTS95S/{ROOM}";
        public static string url = $"ws://dev1.ecs.fbpub.net:9096/XsZubnMOTHC0JRDTS95S/{ROOM}";
    }
}