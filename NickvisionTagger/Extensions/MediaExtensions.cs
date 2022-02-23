using System;

namespace NickvisionTagger.Extensions;

public static class MediaExtensions
{
    public static string DurationToString(this int duration)
    {
        var seconds = duration % 60;
        duration /= 60;
        var minutes = duration % 60;
        var hours = duration / 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public static string FileSizeToString(this long fileSize)
    {
        var sizes = new string[] { "B", "KB", "MB", "GB", "TB" };
        double length = fileSize;
        var index = 0;
        while (length >= 1024 && index < 4)
        {
            index++;
            length /= 1024;
        }
        return $"{Math.Round((decimal)length, 2)} {sizes[index]}";
    }
}
