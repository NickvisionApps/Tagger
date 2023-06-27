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
    public static string ToDurationString(double duration)
    {
        duration = Math.Round(duration);
        var seconds = duration % 60;
        duration /= 60;
        var minutes = duration % 60;
        duration /= 60;
        var hours = duration / 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    
    /// <summary>
    /// Converts a file size in bytes to a string (~ MB)
    /// </summary>
    /// <param name="fileSize">The file size in bytes</param>
    /// <returns>The file size string (~ MB)</returns>
    public static string ToFileSizeString(long fileSize)
    {
        var sizes = new string[] { _("B"), _("KB"), _("MB"), _("GB"), _("TB") };
        var size = (double)fileSize;
        var index = 0;
        while (size >= 1024 && index < 4)
        {
            index++;
            size /= 1024;
        }
        return $"{Math.Ceiling(size * 100.0) / 100.0} {sizes[index]}";
    }
}