// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Extension methods for DateTime operations and formatting.
/// Centralizes timezone and formatting logic to ensure consistency.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the start of the day (00:00:00) for the given DateTime.
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59) for the given DateTime.
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday) for the given DateTime.
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        int diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return dateTime.AddDays(-diff).StartOfDay();
    }

    /// <summary>
    /// Gets the end of the week (Sunday) for the given DateTime.
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).EndOfDay();
    }

    /// <summary>
    /// Gets the start of the month for the given DateTime.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month for the given DateTime.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Calculates the age in years between the given date and today.
    /// Useful for age validation in forms.
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Converts UTC DateTime to ISO 8601 format string.
    /// Standard format for API responses and logging.
    /// </summary>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Returns human-readable relative time (e.g., "2 hours ago", "in 3 days").
    /// Used for displaying timestamps in UI without requiring timezone conversion.
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        return timeSpan.TotalSeconds < 60
            ? "just now"
            : timeSpan.TotalMinutes < 60
            ? $"{(int)timeSpan.TotalMinutes}m ago"
            : timeSpan.TotalHours < 24
            ? $"{(int)timeSpan.TotalHours}h ago"
            : timeSpan.TotalDays < 30
            ? $"{(int)timeSpan.TotalDays}d ago"
            : $"{dateTime:MMM d, yyyy}";
    }

    /// <summary>
    /// Checks if the given DateTime is within business hours (9 AM to 5 PM).
    /// </summary>
    public static bool IsBusinessHours(this DateTime dateTime)
    {
        return dateTime.Hour >= 9 && dateTime.Hour < 17 && dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday;
    }

    /// <summary>
    /// Checks if the given DateTime is in the past relative to now.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the given DateTime is in the future relative to now.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }
}
