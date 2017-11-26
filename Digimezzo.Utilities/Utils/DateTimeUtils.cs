using System;
using System.Globalization;

namespace Digimezzo.Utilities.Utils
{
    public static class DateTimeUtils
    {
        public static long ConvertToUnixTime(DateTime dateTime)
        {
            DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime.ToUniversalTime() - referenceTime).TotalSeconds;
        }

        public static bool IsValidDate(int year, int month, int day)
        {
            DateTime dateValue;

            if (DateTime.TryParseExact($"{year}-{month.ToString("D2")}-{day.ToString("D2")}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                return true;
            }

            return false;
        }
    }
}
