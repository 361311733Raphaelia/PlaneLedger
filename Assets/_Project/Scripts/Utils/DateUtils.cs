using System;

namespace PlaneLedger.Utils
{
    /// <summary>
    /// 日期相关工具方法。
    /// </summary>
    public static class DateUtils
    {
        /// <summary>获取今日日期字符串 yyyy-MM-dd</summary>
        public static string TodayKey => DateTime.Now.Date.ToString("yyyy-MM-dd");

        /// <summary>获取当前月份字符串 yyyy-MM</summary>
        public static string CurrentMonthKey => DateTime.Now.ToString("yyyy-MM");

        /// <summary>解析日期字符串，失败返回 DateTime.MinValue</summary>
        public static DateTime ParseDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var result))
                return result;
            return DateTime.MinValue;
        }

        /// <summary>计算两个日期间的天数差（绝对值）</summary>
        public static int DaysBetween(string dateA, string dateB)
        {
            var a = ParseDate(dateA);
            var b = ParseDate(dateB);
            return Math.Abs((a.Date - b.Date).Days);
        }

        /// <summary>检查是否是同一天</summary>
        public static bool IsSameDay(string dateA, string dateB)
        {
            return DaysBetween(dateA, dateB) == 0;
        }

        /// <summary>检查 dateA 是否是 dateB 的前一天</summary>
        public static bool IsConsecutiveDay(string earlier, string later)
        {
            var a = ParseDate(earlier);
            var b = ParseDate(later);
            return (b.Date - a.Date).Days == 1;
        }

        /// <summary>获取当前小时 (0-23)</summary>
        public static int CurrentHour => DateTime.Now.Hour;

        /// <summary>获取当前是星期几</summary>
        public static DayOfWeek CurrentDayOfWeek => DateTime.Now.DayOfWeek;

        /// <summary>检查当前时间是否在指定范围内（24小时制）</summary>
        public static bool IsInTimeRange(int startHour, int endHour)
        {
            int hour = DateTime.Now.Hour;
            if (startHour <= endHour)
                return hour >= startHour && hour < endHour;
            // 跨午夜
            return hour >= startHour || hour < endHour;
        }
    }
}
