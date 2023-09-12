namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Formats for a playlist
/// </summary>
public enum PlaylistFormat
{
    M3U,
    PLS,
    FPL,
    XSPF,
    SMIL,
    ASX,
    B4S,
    DPL
}

/// <summary>
/// Extension methods for PlaylistFormat
/// </summary>
public static class PlaylistFormatExtensions
{
    public static string GetDotExtension(this PlaylistFormat format) => $".{format.ToString().ToLower()}";
}

/// <summary>
/// A model of options for a playlist
/// </summary>
public class PlaylistOptions
{
    /// <summary>
    /// The name of the playlist
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// The format of the playlist
    /// </summary>
    public PlaylistFormat Format { get; init; }
    /// <summary>
    /// Whether or not to include only selected files in the playlist
    /// </summary>
    public bool IncludeOnlySelectedFiles { get; init; }

    /// <summary>
    /// Constructs a PlaylistOptions
    /// </summary>
    /// <param name="name">The name of the playlist</param>
    /// <param name="format">The format of the playlist</param>
    /// <param name="includeOnlySelectedFiles">Whether or not to include only selected files in the playlist</param>
    public PlaylistOptions(string name, PlaylistFormat format, bool includeOnlySelectedFiles)
    {
        Name = name;
        Format = format;
        IncludeOnlySelectedFiles = includeOnlySelectedFiles;
    }
}