using System;

namespace PerfectWardAPI.Data
{
    public static class DataExtensions
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this long timestamp)
        {
            return Epoch.AddSeconds(timestamp).ToLocalTime();
        }

        public static long ToTimeStamp(this DateTime dateTime)
        {
            if (dateTime == null) return 0;
            if (dateTime < Epoch) return 0;
            return (long)(dateTime - Epoch).TotalSeconds;
        }
    }
}
