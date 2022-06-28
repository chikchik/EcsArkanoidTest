using System.Collections.Generic;

namespace Game.Fabros.Net.Client
{
    public class Stats
    {
        public int lags;
        public int oppLags;
        public int diffSize;
            
        public int lastClientTick;
        public int lastReceivedServerTick;
        public float lastServerTime;
        public int simTicksTotal;

        public List<int> delaysHistory = new List<int>();
        public const int HISTORY_LEN = 10;

        public Stats()
        {
            for (int i = 0; i < HISTORY_LEN; ++i)
                delaysHistory.Add(0);
        }
    }
}