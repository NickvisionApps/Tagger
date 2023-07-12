using System;
using System.IO;
using System.Text.Json;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Ways to sort transactions
/// </summary>
public enum SortBy
{
    Filename = 0,
    Title,
    Track,
    Path,
    Album,
    Genre
}

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration
{
    public static readonly string ConfigDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Current.Name}";
    private static readonly string ConfigPath = $"{ConfigDir}{Path.DirectorySeparatorChar}config.json";
    private static Configuration? _instance;

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
    /// The user's AcoustId API Key
    /// </summary>
    public string AcoustIdUserAPIKey { get; set; }

    /// <summary>
    /// Occurs when the configuration is saved to disk
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }
        Theme = Theme.System;
        RememberLastOpenedFolder = true;
        IncludeSubfolders = true;
        SortFilesBy = SortBy.Filename;
        LastOpenedFolder = "";
        PreserveModificationTimestamp = false;
        OverwriteTagWithMusicBrainz = true;
        OverwriteAlbumArtWithMusicBrainz = true;
        AcoustIdUserAPIKey = "";
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    _instance = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(ConfigPath)) ?? new Configuration();
                }
                catch
                {
                    _instance = new Configuration();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this));
        Saved?.Invoke(this, EventArgs.Empty);
    }
}
