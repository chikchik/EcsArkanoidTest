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
        public static string url = $"wss://rts.oxygine.org/XsZubnMOTHC0JRDTS95S/{ROOM}";
        
        //Physics
        public static Vector2 GRAVITY = new Vector2(0, -9.8f);
        public static int VELOCITY_ITERATIONS = 6;
        public static int POSITION_ITERATIONS = 2;
    }
}