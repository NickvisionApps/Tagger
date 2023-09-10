using Nickvision.Aura;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Ways to sort transactions
/// </summary>
public enum SortBy
{
    Filename = 0,
    Path,
    Title,
    Artist,
    Album,
    Year,
    Track,
    Genre
}

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration : ConfigurationBase
{
    /// <summary>
    /// The preferred theme for the application
    /// </summary>
    public Theme Theme { get; set; }
    /// <summary>
    /// Whether or not to remember the last opened folder
    /// </summary>
    public bool RememberLastOpenedFolder { get; set; }
    /// <summary>
    /// Whether or not to scan subfolders for music
    /// </summary>
    public bool IncludeSubfolders { get; set; }
    /// <summary>
    /// What to sort files in a music folder by
    /// </summary>
    public SortBy SortFilesBy { get; set; }
    /// <summary>
    /// The last opened folder (if available)
    /// </summary>
    public string LastOpenedFolder { get; set; }
    /// <summary>
    /// Whether or not to preserve (not change) a file's modification timestamp
    /// </summary>
    public bool PreserveModificationTimestamp { get; set; }
    /// <summary>
    /// Whether or not to overwrite a tag's existing data with data from MusicBrainz
    /// </summary>
    public bool OverwriteTagWithMusicBrainz { get; set; }
    /// <summary>
    /// Whether or not to overwrite a tag's existing album art with album art from MusicBrainz
    /// </summary>
    public bool OverwriteAlbumArtWithMusicBrainz { get; set; }
    /// <summary>
    /// Whether or not to overwrite a tag's existing lyric data with data from the web
    /// </summary>
    public bool OverwriteLyricsWithWebService { get; set; }
    /// <summary>
    /// The user's AcoustId API Key
    /// </summary>
    public string AcoustIdUserAPIKey { get; set; }

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        Theme = Theme.System;
        RememberLastOpenedFolder = true;
        IncludeSubfolders = true;
        SortFilesBy = SortBy.Path;
        LastOpenedFolder = "";
        PreserveModificationTimestamp = false;
        OverwriteTagWithMusicBrainz = true;
        OverwriteAlbumArtWithMusicBrainz = true;
        OverwriteLyricsWithWebService = true;
        AcoustIdUserAPIKey = "";
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current => (Configuration)Aura.Active.ConfigFiles["config"];
}
