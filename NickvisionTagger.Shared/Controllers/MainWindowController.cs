using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    private MusicFolder? _musicFolder;
    private List<bool> _musicFilesSaved;

    /// <summary>
    /// The list of selected music files
    /// </summary>
    public Dictionary<int, MusicFile> SelectedMusicFiles { get; init; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// The path of the music folder
    /// </summary>
    public string MusicFolderPath => _musicFolder?.ParentPath ?? "";
    /// <summary>
    /// The list of all music files in the music folder
    /// </summary>
    public List<MusicFile> MusicFiles => _musicFolder?.MusicFiles ?? new List<MusicFile>();

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when a shell notification is sent
    /// </summary>
    public event EventHandler<ShellNotificationSentEventArgs>? ShellNotificationSent;
    /// <summary>
    /// Occurs when the music folder is updated. The boolean arg represents whether or not to send a toast
    /// </summary>
    public event EventHandler<bool> MusicFolderUpdated;
    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    public event EventHandler<EventArgs> MusicFilesSaveStateChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        _musicFolder = null;
        _musicFilesSaved = new List<bool>();
        SelectedMusicFiles = new Dictionary<int, MusicFile>();
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Starts the application
    /// </summary>
    public void Startup()
    {
        Configuration.Current.Saved += ConfigurationSaved;
    }

    /// <summary>
    /// Opens a music folder
    /// </summary>
    /// <param name="path">The path to the music folder</param>
    public async Task OpenFolderAsync(string path)
    {
        _musicFolder = new MusicFolder(path);
        _musicFolder.IncludeSubfolders = Configuration.Current.IncludeSubfolders;
        await _musicFolder.ReloadMusicFilesAsync();
        MusicFolderUpdated?.Invoke(this, true);
    }

    /// <summary>
    /// Occurs when the configuration is saved
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private async void ConfigurationSaved(object? sender, EventArgs e)
    {
        if(_musicFolder != null && _musicFolder.IncludeSubfolders != Configuration.Current.IncludeSubfolders)
        {
            _musicFolder.IncludeSubfolders = Configuration.Current.IncludeSubfolders;
            await _musicFolder.ReloadMusicFilesAsync();
            MusicFolderUpdated?.Invoke(this, true);
        }
    }
}
