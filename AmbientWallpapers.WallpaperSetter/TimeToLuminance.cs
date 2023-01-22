using System;

namespace AmbientWallpapers.WallpaperSetter
{
    public static class TimeToLuminance
    {
        public static double MagicNumber = -7.665; //This number helps move Math.sin() to desired dates :P

        public static double LuminanceNow() {
            return LuminanceInTime(DateTime.Now);
        }

        public static double LuminanceInTime(DateTime currTime)
        {
            var dayLuminance = DayLuminanceNormalized(currTime);

            //Shortest 8:00 to 16:50 = 8:50 day - 15:10 night --- a = 0.7 b = 2pi/24 c = -1.9 d = 0.3
            //Longest 4:40 to 20:20 = 15:40 day - 8:20 night --- a = 0.5 b = 2pi/24 c = -1.7 d = 0.5
            const int shortestStartingMinute = 480;
            const int longestStartingMinute = 280;
            const int shortestEndingMinute = 1010;
            const int longestEndingMinute = 1220;

            var currentMinute = currTime.Hour * 60 + currTime.Minute;

            var todaySunrise = shortestStartingMinute - dayLuminance * (shortestStartingMinute - longestStartingMinute);
            var todaySunset = shortestEndingMinute - dayLuminance * (shortestEndingMinute - longestEndingMinute);

            var todayA = 0.7 - dayLuminance * (0.7 - 0.5);
            var todayB = 2 * Math.PI / 24;
            var todayC = -1.9 - dayLuminance * (-1.9 - -1.7);
            var todayD = 0.3 - dayLuminance * (0.3 - 0.5);

            var currentPosition = todayA * Math.Sin(todayB * (currentMinute / 60.0) + todayC) + todayD;

            return Math.Round(currentPosition.Clamp(0.0, 1.0), 4, MidpointRounding.ToEven);
        }

        public static double DayLuminanceNormalized(DateTime day)
        {
            var currentYearLength = DateTime.IsLeapYear(day.Year) ? 366 : 365;
            var winterSolisticeDayIndex = 334 + 21 + (DateTime.IsLeapYear(day.Year) ? 0 : -1);

            var currentDayLightness = 0.5 * Math.Sin(((2 * Math.PI) / currentYearLength) * day.DayOfYear + MagicNumber) + 0.5;

            return Math.Round(currentDayLightness, 4, MidpointRounding.ToEven);
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

    }
}
