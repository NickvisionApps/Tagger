using System;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Helpers;

/// <summary>
/// Helper functions for media functions
/// </summary>
public static class MediaHelpers
{
    /// <summary>
    /// Converts a duration in seconds to a duration string (HH:MM:SS)
    /// </summary>
    /// <param name="duration">The duration in seconds</param>
    /// <returns>The duration string (HH:MM:SS)</returns>
    public static string ToDurationString(this int duration) => TimeSpan.FromSeconds(duration).ToString("g");

    /// <summary>
    /// Converts a file size in bytes to a string (~ MB)
    /// </summary>
    /// <param name="fileSize">The file size in bytes</param>
    /// <returns>The file size string (~ MB)</returns>
    public static string ToFileSizeString(this long fileSize)
    {
        var sizes = new string[] { _("B"), _("KiB"), _("MiB"), _("GiB"), _("TiB") };
        var size = (double)fileSize;
        var index = 0;
        while (size >= 1024 && index < 4)
        {
            index++;
            size /= 1024;
        }
        return $"{Math.Ceiling(size * 100.0) / 100.0} {sizes[index]}";
    }

    /// <summary>
    /// Format the given duration using the following format
    ///     DDdHH:MM:SS.UUUU
    ///     
    ///  Where
    ///     DD is the number of days, if applicable (i.e. durations of less than 1 day won't display the "DDd" part)
    ///     HH is the number of hours, if applicable (i.e. durations of less than 1 hour won't display the "HH:" part)
    ///     MM is the number of minutes
    ///     SS is the number of seconds
    ///     UUUU is the number of milliseconds
    /// </summary>
    /// <param name="milliseconds">Duration to format (in milliseconds)</param>
    /// <returns>Formatted duration according to the abovementioned convention</returns>
    /// <remarks>TAKEN FROM: https://github.com/Zeugma440/atldotnet/blob/main/ATL/Utils/Utils.cs#L63</remarks>
    public static string MillisecondsToTimecode(this int milliseconds)
    {
        long seconds = Convert.ToInt64(Math.Floor((long)milliseconds / 1000.00));
        return SecondsToTimecode(seconds) + "." + (milliseconds - seconds * 1000);
    }

    /// <summary>
    /// Convert the duration of the given timecode to milliseconds
    /// Supported formats : hh:mm, hh:mm:ss.ddd, mm:ss, hh:mm:ss and mm:ss.ddd
    /// </summary>
    /// <param name="timeCode">Timecode to convert</param>
    /// <returns>Duration of the given timecode expressed in milliseconds if succeeded; -1 if failed</returns>
    /// <remarks>TAKEN FROM: https://github.com/Zeugma440/atldotnet/blob/main/ATL/Utils/Utils.cs#L114</remarks>
    public static int TimecodeToMs(this string timeCode)
    {
        int result = -1;
        DateTime dateTime;
        bool valid = false;
        if (DateTime.TryParse(timeCode, out dateTime)) // Handle classic cases hh:mm, hh:mm:ss.ddd (the latter being the spec)
        {
            valid = true;
            result = dateTime.Millisecond;
            result += dateTime.Second * 1000;
            result += dateTime.Minute * 60 * 1000;
            result += dateTime.Hour * 60 * 60 * 1000;
        }
        else // Handle mm:ss, hh:mm:ss and mm:ss.ddd
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int milliseconds = 0;
            if (timeCode.Contains(':'))
            {
                valid = true;
                try
                {
                    string[] parts = timeCode.Split(':');
                    if (parts[^1].Contains('.'))
                    {
                        string[] subPart = parts[^1].Split('.');
                        parts[^1] = subPart[0];
                        milliseconds = int.Parse(subPart[1]);
                    }

                    seconds = int.Parse(parts[^1]);
                    minutes = int.Parse(parts[^2]);
                    if (parts.Length >= 3)
                    {
                        string[] subPart = parts[^3].Split('d');
                        if (subPart.Length > 1)
                        {
                            days = int.Parse(subPart[0].Trim());
                            hours = int.Parse(subPart[1].Trim());
                        }
                        else
                        {
                            hours = int.Parse(subPart[0]);
                        }
                    }

                    result = milliseconds;
                    result += seconds * 1000;
                    result += minutes * 60 * 1000;
                    result += hours * 60 * 60 * 1000;
                    result += days * 24 * 60 * 60 * 1000;
                }
                catch
                {
                    valid = false;
                }
            }
        }
        if (!valid) result = -1;
        return result;
    }

    /// <summary>
    /// Format the given duration using the following format
    ///     DDdHH:MM:SS
    ///     
    ///  Where
    ///     DD is the number of days, if applicable (i.e. durations of less than 1 day won't display the "DDd" part)
    ///     HH is the number of hours, if applicable (i.e. durations of less than 1 hour won't display the "HH:" part)
    ///     MM is the number of minutes
    ///     SS is the number of seconds
    /// </summary>
    /// <param name="seconds">Duration to format (in seconds)</param>
    /// <returns>Formatted duration according to the abovementioned convention</returns>
    /// <remarks>TAKEN FROM: https://github.com/Zeugma440/atldotnet/blob/main/ATL/Utils/Utils.cs#L82</remarks>
    private static string SecondsToTimecode(long seconds)
    {
        int h;
        long m;
        string hStr, mStr, sStr;
        long s;
        int d;
        h = Convert.ToInt32(Math.Floor(seconds / 3600.00));
        m = Convert.ToInt64(Math.Floor((seconds - 3600.00 * h) / 60));
        s = seconds - 60 * m - 3600 * h;
        d = Convert.ToInt32(Math.Floor(h / 24.00));
        if (d > 0) h = h - 24 * d;
        hStr = h.ToString();
        if (1 == hStr.Length) hStr = "0" + hStr;
        mStr = m.ToString();
        if (1 == mStr.Length) mStr = "0" + mStr;
        sStr = s.ToString();
        if (1 == sStr.Length) sStr = "0" + sStr;
        if (d > 0) return d + "d " + hStr + ":" + mStr + ":" + sStr;
        if (h > 0) return hStr + ":" + mStr + ":" + sStr;
        return mStr + ":" + sStr;
    }
}