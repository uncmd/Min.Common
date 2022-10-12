namespace System;

public static class MinDateTimeExtensions
{
    public static DateTime ClearTime(this DateTime dateTime)
    {
        return dateTime.Subtract(
            new TimeSpan(
                0,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond
            )
        );
    }

    public static long GetTimestamp(this DateTime dateTime, bool isSecond = false)
    {
        var dateTimeOffset = new DateTimeOffset(dateTime);

        return isSecond
            ? dateTimeOffset.ToUnixTimeSeconds()
            : dateTimeOffset.ToUnixTimeMilliseconds();
    }

    public static bool IsWeekend(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek.IsIn(DayOfWeek.Saturday, DayOfWeek.Sunday);
    }

    public static bool IsWeekday(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek.IsIn(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);
    }
}
