using Fabros.P2P;

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
        public static string url = $"ws://dev1.ecs.fbpub.net:9096/XsZubnMOTHC0JRDTS95S/{ROOM}";
    }
}