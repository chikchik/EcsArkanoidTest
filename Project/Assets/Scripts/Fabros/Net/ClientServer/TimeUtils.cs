using System;

namespace Game.Fabros.Net.ClientServer
{
    public static class TimeUtils
    {
        public static long GetUnixTimeMS()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        
        public static long GetUnixTimeMS(this DateTime tm)
        {
            return new DateTimeOffset(tm).ToUnixTimeMilliseconds();
        }
    }
}