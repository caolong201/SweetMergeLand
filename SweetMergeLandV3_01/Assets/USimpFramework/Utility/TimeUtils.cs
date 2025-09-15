using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;


namespace USimpFramework.Utility
{
    public static class TimeUtils
    {
        private readonly static DateTime dateTime1970 = new DateTime(1970, 1, 1);

        public const int ONE_SECOND_MILLITIME = 1000;
        public const int ONE_MINUTE_MILLITIME = 60 * ONE_SECOND_MILLITIME;
        public const int ONE_HOUR_MILLITIME = 60 * ONE_MINUTE_MILLITIME;
        public const int ONE_DAY_MILLITIME = 24 * ONE_HOUR_MILLITIME;


        static Stopwatch clock = new();

        static DateTime serverStartUtcTime;

        /// <summary>
        /// Call when first loading the game to set the server start time
        /// </summary>
        /// <param name="serverUtcNowMs"></param>
        public static void InitServerStartTime(long serverUtcNowMs)
        {
            clock.Restart();

            serverStartUtcTime = dateTime1970.AddSeconds(serverUtcNowMs / ONE_SECOND_MILLITIME);
        }

        public static long GetServerUtcNowMs()
        {
            return CurrentMillitime(serverStartUtcTime + clock.Elapsed);
        }

        public static long CurrentMillitime()
        {
            return CurrentMillitime(DateTime.UtcNow);
        }

        public static long CurrentMillitime(DateTime dateTime)
        {
            return (long)(dateTime - dateTime1970).TotalMilliseconds;
        }

        public static string CustomDateTimeToString()
        {
            return DateTime.UtcNow.ToString();
        }

        public static DateTime GetDateTime(long milliseconds)
        {
            return dateTime1970 + TimeSpan.FromMilliseconds(milliseconds);
        }

        public static long Today0hUtcMs()
        {
            return Today0hUtcMs(GetServerUtcNowMs());
        }

        public static long Today0hUtcMs(long currentUtcMs)
        {
            var nowDateTime = DateTimeOffset.FromUnixTimeMilliseconds(currentUtcMs);
            var todayAtMidnightUtc = new DateTimeOffset(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, 0, 0, 0, TimeSpan.Zero);

            return todayAtMidnightUtc.ToUnixTimeMilliseconds();
        }

        public static long NextMonthly0hMillitime()
        {
            var now = DateTime.UtcNow;

            var at0hToday = new DateTime(now.Year, now.Month, now.Day);

            var nextYear = now.Year;
            var nextMonth = now.Month;

            if (nextMonth < 12) nextMonth += 1;
            else
            {
                nextYear += 1;
                nextMonth = 1;
            }

            var at0hNextMonthly = new DateTime(nextYear, nextMonth, 1);

            return CurrentMillitime(at0hNextMonthly);
        }

        public static long ThisWeek0hMillitime()
        {
            var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

            var at0hMonday = new DateTime(monday.Year, monday.Month, monday.Day);

            return CurrentMillitime(at0hMonday);
        }

        public static long Tomorrow0hMillitime()
        {
            return Today0hUtcMs() + ONE_DAY_MILLITIME;
        }

        public static long Tomorrow0hMillitime(long ticks)
        {
            return Today0hUtcMs(ticks) + ONE_DAY_MILLITIME;
        }

    }
}
