using ATL;
using FuzzySharp;
using Nickvision.Aura;
using Nickvision.Aura.Network;
using Nickvision.Aura.Taskbar;
using Nickvision.Aura.Update;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// Types of album arts
/// </summary>
public enum AlbumArtType
{
    Front = 0,
    Back
}

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController : IDisposable
{
    private bool _disposed;
    private TaskbarItem? _taskbarItem;
    private Updater? _updater;
    private string? _libraryToLaunch;
    private MusicLibrary? _musicLibrary;
    private bool _forceAllowClose;
    private bool _hadUserFilenameChange;
    private readonly string[] _genreSuggestions;
    private readonly List<bool> _musicFileChangedFromUpdate;
    private readonly Dictionary<int, PropertyMap> _filesBeingEditedOriginals;

    /// <summary>
    /// The list of predefined format strings
    /// </summary>
    public string[] FormatStrings { get; init; }
    /// <summary>
    /// The list of music file save states
    /// </summary>
    /// <remarks>A true value means that the file is saved whereas a false value means the file has unsaved changed</remarks>
    public List<bool> MusicFileSaveStates { get; init; }
    /// <summary>
    /// The list of selected music files
    /// </summary>
    public Dictionary<int, MusicFile> SelectedMusicFiles { get; init; }
    /// <summary>
    /// The property map for the selected music files
    /// </summary>
    public PropertyMap SelectedPropertyMap { get; init; }
    /// <summary>
    /// The NetworkMonitor
    /// </summary>
    public NetworkMonitor? NetworkMonitor { get; private set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// The type of the music library
    /// </summary>
    public MusicLibraryType MusicLibraryType => _musicLibrary?.Type ?? MusicLibraryType.Folder;
    /// <summary>
    /// The name of the music library
    /// </summary>
    public string MusicLibraryName => _musicLibrary?.Name ?? "";
    /// <summary>
    /// The list of all music files in the music library
    /// </summary>
    public List<MusicFile> MusicFiles => _musicLibrary?.MusicFiles ?? new List<MusicFile>();
    /// <summary>
    /// The list of paths to corrupted music files in the music library
    /// </summary>
    public List<string> CorruptedFiles => _musicLibrary?.CorruptedFiles ?? new List<string>();

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when a shell notification is sent
    /// </summary>
    public event EventHandler<ShellNotificationSentEventArgs>? ShellNotificationSent;
    /// <summary>
    /// Occurs when the loading state is updated
    /// </summary>
    public event EventHandler<string>? LoadingStateUpdated;
    /// <summary>
    /// Occurs when the loading progress is updated
    /// </summary>
    public event EventHandler<(int Value, int MaxValue, string Message)>? LoadingProgressUpdated;
    /// <summary>
    /// Occurs when the music library is updated
    /// </summary>
    public event EventHandler<EventArgs>? MusicLibraryUpdated;
    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    /// <remarks>The boolean arg is whether or not there are unsaved changes</remarks>
    public event EventHandler<bool>? MusicFileSaveStatesChanged;
    /// <summary>
    /// Occurs when the selected music files' properties are changed
    /// </summary>
    public event EventHandler<EventArgs>? SelectedMusicFilesPropertiesChanged;
    /// <summary>
    /// Occurs when fingerprint calculating is done
    /// </summary>
    public event EventHandler<EventArgs>? FingerprintCalculated;
    /// <summary>
    /// Occurs when there are corrupted music files found in a music library
    /// </summary>
    public event EventHandler<EventArgs>? CorruptedFilesFound;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    public MainWindowController(string[] args)
    {
        _disposed = false;
        if (args.Length > 0)
        {
            var path = args[0];
            if (path.StartsWith("file://"))
            {
                path = path.Remove(0, "file://".Length);
            }
            if (Directory.Exists(path) || (File.Exists(path) && Enum.GetValues<PlaylistFormat>().Select(x => x.GetDotExtension()).Contains(Path.GetExtension(path))))
            {
                _libraryToLaunch = path;
            }
        }
        Aura.Init("org.nickvision.tagger", "Nickvision Tagger");
        if (Directory.Exists($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}"))
        {
            // Move config files from older versions and delete old directory
            try
            {
                foreach (var file in Directory.GetFiles($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}"))
                {
                    File.Move(file, $"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}{Path.GetFileName(file)}");
                }
            }
            catch (IOException) { }
            Directory.Delete($"{UserDirectories.Config}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Name}", true);
        }
        Aura.Active.SetConfig<Configuration>("config");
        Configuration.Current.Saved += ConfigurationSaved;
        AppInfo.Version = "2023.10.0-next";
        AppInfo.ShortName = _("Tagger");
        AppInfo.Description = _("Tag your music");
        AppInfo.SourceRepo = new Uri("https://github.com/NickvisionApps/Tagger");
        AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Tagger/issues/new");
        AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Tagger/discussions");
        AppInfo.ExtraLinks[_("Matrix Chat")] = new Uri("https://matrix.to/#/#nickvision:matrix.org");
        AppInfo.Developers[_("Nicholas Logozzo")] = new Uri("https://github.com/nlogozzo");
        AppInfo.Developers[_("Contributors on GitHub ❤️")] = new Uri("https://github.com/NickvisionApps/Tagger/graphs/contributors");
        AppInfo.Designers[_("Nicholas Logozzo")] = new Uri("https://github.com/nlogozzo");
        AppInfo.Designers[_("Fyodor Sobolev")] = new Uri("https://github.com/fsobolev");
        AppInfo.Designers[_("DaPigGuy")] = new Uri("https://github.com/DaPigGuy");
        AppInfo.Artists[_("David Lapshin")] = new Uri("https://github.com/daudix-UFO");
        AppInfo.TranslatorCredits = _("translator-credits");
        _musicLibrary = null;
        _forceAllowClose = false;
        _hadUserFilenameChange = false;
        _genreSuggestions = new string[]
        {
            "Blues", "Classic rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-hop", "Jazz", "Metal",
            "New age", "Oldies", "Other", "Pop", "Rhythm and blues", "Rap", "Reggae", "Rock", "Techno", "Industrial",
            "Alternative", "Ska", "Death metal", "Pranks", "Soundtrack", "Euro-techno", "Ambient", "Trip-hop", "Vocal",
            "Jazz & funk", "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound clip",
            "Gospel", "Noise", "Alternative rock", "Bass", "Soul", "Punk", "Space", "Meditative", "Instrumental pop",
            "Instrumental rock", "Ethnic", "Gothic", "Techno-industrial", "Electronic", "Pop-folk", "Eurodance",
            "Dream", "Southern rock", "Comedy", "Gangsta", "Top 40", "Christian rap", "Pop/funk", "New wave", "Rave",
            "Trailer", "Low-fi", "Tribal", "Polka", "Retro", "Musical", "Rock 'n' roll", "Hard rock", "Folk", "Swing",
            "Latin", "Chorus", "Acoustic", "Opera", "Club", "Tango", "Samba", "Freestyle", "A cappella", "Dance hall",
            "Indie", "Merengue", "Salsa", "Bachata", "Christmas", "EDM"
        };
        _musicFileChangedFromUpdate = new List<bool>();
        _filesBeingEditedOriginals = new Dictionary<int, PropertyMap>();
        FormatStrings = new string[] { _("%artist%- %title%"), _("%title%- %artist%"), _("%track%- %title%"), _("%title%") };
        MusicFileSaveStates = new List<bool>();
        SelectedMusicFiles = new Dictionary<int, MusicFile>();
        SelectedPropertyMap = new PropertyMap();
    }

    /// <summary>
    /// Finalizes the MainWindowController
    /// </summary>
    ~MainWindowController() => Dispose(false);

    /// <summary>
    /// Main window width
    /// </summary>
    public int WindowWidth
    {
        get => Configuration.Current.WindowWidth;

        set => Configuration.Current.WindowWidth = value;
    }

    /// <summary>
    /// Main window height
    /// </summary>
    public int WindowHeight
    {
        get => Configuration.Current.WindowHeight;

        set => Configuration.Current.WindowHeight = value;
    }

    /// <summary>
    /// Whether or not the main window is maximized
    /// </summary>
    public bool WindowMaximized
    {
        get => Configuration.Current.WindowMaximized;

        set => Configuration.Current.WindowMaximized = value;
    }

    /// <summary>
    /// Whether or not to show the Extras Pane
    /// </summary>
    public bool ExtrasPane
    {
        get => Configuration.Current.ExtrasPane;

        set => Configuration.Current.ExtrasPane = value;
    }

    /// <summary>
    /// What to sort files in a music library by
    /// </summary>
    public SortBy SortFilesBy
    {
        get => Configuration.Current.SortFilesBy;

        set => Configuration.Current.SortFilesBy = value;
    }

    /// <summary>
    /// The TaskbarItem
    /// </summary>
    public TaskbarItem? TaskbarItem
    {
        set
        {
            if (value == null)
            {
                return;
            }
            _taskbarItem = value;
        }
    }

    /// <summary>
    /// The string for greeting on the home page
    /// </summary>
    public string Greeting
    {
        get
        {
            return DateTime.Now.Hour switch
            {
                >= 0 and < 6 => _p("Night", "Good Morning!"),
                < 12 => _p("Morning", "Good Morning!"),
                < 18 => _("Good Afternoon!"),
                < 24 => _("Good Evening!"),
                _ => _("Good Day!")
            };
        }
    }

    /// <summary>
    /// Whether or not the window can close freely
    /// </summary>
    public bool CanClose
    {
        get
        {
            if (_forceAllowClose)
            {
                return true;
            }
            foreach (var saved in MusicFileSaveStates)
            {
                if (!saved)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Whether or not at least one file in the group of select files has unsaved changes
    /// </summary>
    public bool SelectedHasUnsavedChanges
    {
        get
        {
            foreach (var pair in SelectedMusicFiles)
            {
                if (!MusicFileSaveStates[pair.Key])
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        _taskbarItem?.Dispose();
        _musicLibrary?.Dispose();
        NetworkMonitor?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Creates a new LyricsDialogController
    /// </summary>
    /// <returns>The LyricsDialogController</returns>
    public LyricsDialogController CreateLyricsDialogController()
    {
        if (SelectedMusicFiles.Count == 1)
        {
            var first = SelectedMusicFiles.First().Value;
            return new LyricsDialogController(first.Lyrics);
        }
        return new LyricsDialogController(null);
    }

    /// <summary>
    /// Starts the application
    /// </summary>
    public async Task StartupAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Configuration.Current.AutomaticallyCheckForUpdates)
        {
            await CheckForUpdatesAsync();
        }
        NetworkMonitor = await NetworkMonitor.NewAsync();
        if (_libraryToLaunch != null)
        {
            await OpenLibraryAsync(_libraryToLaunch);
            _libraryToLaunch = null;
        }
        else if (Configuration.Current.RememberLastOpenedFolder && Path.Exists(Configuration.Current.LastOpenedFolder))
        {
            await OpenLibraryAsync(Configuration.Current.LastOpenedFolder);
        }
        else
        {
            MusicLibraryUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Forces CanClose to be true
    /// </summary>
    public void ForceAllowClose() => _forceAllowClose = true;

    /// <summary>
    /// Saves the app's configuration file to disk
    /// </summary>
    public void SaveConfig() => Aura.Active.SaveConfig("config");

    /// <summary>
    /// Checks for an application update and notifies the user if one is available
    /// </summary>
    public async Task CheckForUpdatesAsync()
    {
        if (!AppInfo.IsDevVersion)
        {
            if (_updater == null)
            {
                _updater = await Updater.NewAsync();
            }
            var version = await _updater!.GetCurrentStableVersionAsync();
            if (version != null && version > new System.Version(AppInfo.Version))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("New update available."), NotificationSeverity.Success, "update"));
            }
        }
    }

    /// <summary>
    /// Downloads and installs the latest application update for Windows systems
    /// </summary>
    /// <returns>True if successful, else false</returns>
    /// <remarks>CheckForUpdatesAsync must be called before this method</remarks>
    public async Task<bool> WindowsUpdateAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _updater != null)
        {
            var res = await _updater.WindowsUpdateAsync(VersionType.Stable);
            if (!res)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to download and install update."), NotificationSeverity.Error));
            }
            return res;
        }
        return false;
    }

    /// <summary>
    /// Opens a music library
    /// </summary>
    /// <param name="path">The path to the music library</param>
    public async Task OpenLibraryAsync(string path)
    {
        if (!MusicLibrary.GetIsValidLibraryPath(path))
        {
            return;
        }
        _musicLibrary = new MusicLibrary(path)
        {
            IncludeSubfolders = Configuration.Current.IncludeSubfolders,
            SortFilesBy = Configuration.Current.SortFilesBy
        };
        _musicLibrary.LibraryChangedOnDisk += (sender, e) =>
        {
            if (!_hadUserFilenameChange)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Library was changed on disk."), NotificationSeverity.Warning, "reload"));
            }
            _hadUserFilenameChange = false;
        };
        _musicLibrary.LoadingProgressUpdated += (sender, e) => UpdateLoadingProgress(e);
        if (Configuration.Current.RememberLastOpenedFolder)
        {
            Configuration.Current.LastOpenedFolder = _musicLibrary.Path;
            Aura.Active.SaveConfig("config");
        }
        await ReloadLibraryAsync();
    }

    /// <summary>
    /// Closes a music library
    /// </summary>
    public void CloseLibrary()
    {
        _musicLibrary?.Dispose();
        _musicLibrary = null;
        _musicFileChangedFromUpdate.Clear();
        _filesBeingEditedOriginals.Clear();
        MusicFileSaveStates.Clear();
        SelectedMusicFiles.Clear();
        if (Configuration.Current.RememberLastOpenedFolder)
        {
            Configuration.Current.LastOpenedFolder = "";
            Aura.Active.SaveConfig("config");
        }
        MusicLibraryUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Reloads the music library
    /// </summary>
    public async Task ReloadLibraryAsync()
    {
        if (_musicLibrary != null)
        {
            LoadingStateUpdated?.Invoke(this, _("Loading music files from library..."));
            _musicFileChangedFromUpdate.Clear();
            _filesBeingEditedOriginals.Clear();
            MusicFileSaveStates.Clear();
            var corruptedFound = await _musicLibrary.ReloadMusicFilesAsync();
            var count = _musicLibrary.MusicFiles.Count;
            for (var i = 0; i < count; i++)
            {
                _musicFileChangedFromUpdate.Add(false);
                MusicFileSaveStates.Add(true);
            }
            MusicLibraryUpdated?.Invoke(this, EventArgs.Empty);
            if (Path.Exists(_musicLibrary.Path))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Loaded {0} music file.", "Loaded {0} music files.", count, count), NotificationSeverity.Success));
            }
            if (corruptedFound)
            {
                CorruptedFilesFound?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Creates a playlist for the current music library
    /// </summary>
    /// <param name="options">PlaylistOptions</param>
    public void CreatePlaylist(PlaylistOptions options)
    {
        if (_musicLibrary != null && _musicLibrary.Type == MusicLibraryType.Folder)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Playlist path can not be empty."), NotificationSeverity.Error));
            }
            if (options.IncludeOnlySelectedFiles && SelectedMusicFiles.Count == 0)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("No music files are selected."), NotificationSeverity.Error));
            }
            else if (_musicLibrary.MusicFiles.Count == 0)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("No music files in library."), NotificationSeverity.Error));
            }
            var path = _musicLibrary.CreatePlaylist(options, options.IncludeOnlySelectedFiles ? SelectedMusicFiles.Keys.ToList() : null);
            if (!string.IsNullOrEmpty(path))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Playlist file created successfully."), NotificationSeverity.Success, "open-playlist", path));
            }
            else
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to create playlist."), NotificationSeverity.Error));
            }
        }
    }

    /// <summary>
    /// Adds a music file to the playlist
    /// </summary>
    /// <param name="path">The full path to the file</param>
    /// <param name="useRelativePath">Whether or not to use the file's relative path instead of full</param>
    public async Task AddFileToPlaylistAsync(string path, bool useRelativePath)
    {
        if (_musicLibrary != null && _musicLibrary.Type == MusicLibraryType.Playlist && File.Exists(path) && MusicLibrary.SupportedExtensions.Contains(Path.GetExtension(path).ToLower()))
        {
            if (_musicLibrary.AddFileToPlaylist(path, useRelativePath))
            {
                await ReloadLibraryAsync();
            }
            else
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to add file to playlist. File may already exist in playlist."), NotificationSeverity.Error));
                MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
            }
        }
    }

    /// <summary>
    /// Removes the selected files from the playlist
    /// </summary>
    public async Task RemoveSelectedFilesFromPlaylistAsync()
    {
        if (_musicLibrary != null && _musicLibrary.Type == MusicLibraryType.Playlist && SelectedMusicFiles.Count > 0)
        {
            if (!_musicLibrary.RemoveFilesFromPlaylist(SelectedMusicFiles.Select(x => x.Key).ToList()))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to remove some files from playlist."), NotificationSeverity.Warning));
            }
            await ReloadLibraryAsync();
        }
    }

    /// <summary>
    /// Updates the tags with values from the property map
    /// </summary>
    /// <param name="map">The PropertyMap with values</param>
    /// <param name="triggerSelectedMusicFilesPropertiesChanged">Whether or not to trigger the SelectedMusicFilesPropertiesChanged event</param>
    public void UpdateTags(PropertyMap map, bool triggerSelectedMusicFilesPropertiesChanged)
    {
        foreach (var pair in SelectedMusicFiles)
        {
            if (!_filesBeingEditedOriginals.ContainsKey(pair.Key))
            {
                _filesBeingEditedOriginals.Add(pair.Key, pair.Value.PropertyMap);
            }
            var updated = false;
            if (map.Filename != pair.Value.Filename && map.Filename != _("<keep>"))
            {
                try
                {
                    pair.Value.Filename = map.Filename;
                    map.Filename = pair.Value.Filename;
                    updated = map.Filename != _filesBeingEditedOriginals[pair.Key].Filename;
                }
                catch { }
            }
            _hadUserFilenameChange = updated;
            if (map.Title != pair.Value.Title && map.Title != _("<keep>"))
            {
                pair.Value.Title = map.Title;
                updated = map.Title != _filesBeingEditedOriginals[pair.Key].Title;
            }
            if (map.Artist != pair.Value.Artist && map.Artist != _("<keep>"))
            {
                pair.Value.Artist = map.Artist;
                updated = map.Artist != _filesBeingEditedOriginals[pair.Key].Artist;
            }
            if (map.Album != pair.Value.Album && map.Album != _("<keep>"))
            {
                pair.Value.Album = map.Album;
                updated = map.Album != _filesBeingEditedOriginals[pair.Key].Album;
            }
            if (map.Year != (pair.Value.Year == 0 ? "" : pair.Value.Year.ToString()) && map.Year != _("<keep>"))
            {
                try
                {
                    pair.Value.Year = int.Parse(map.Year);
                }
                catch
                {
                    pair.Value.Year = 0;
                }
                updated = map.Year != _filesBeingEditedOriginals[pair.Key].Year.ToString();
            }
            if (map.Track != (pair.Value.Track == 0 ? "" : pair.Value.Track.ToString()) && map.Track != _("<keep>"))
            {
                try
                {
                    pair.Value.Track = int.Parse(map.Track);
                }
                catch
                {
                    pair.Value.Track = 0;
                }
                updated = map.Track != _filesBeingEditedOriginals[pair.Key].Track.ToString();
            }
            if (map.TrackTotal != (pair.Value.TrackTotal == 0 ? "" : pair.Value.TrackTotal.ToString()) && map.TrackTotal != _("<keep>"))
            {
                try
                {
                    pair.Value.TrackTotal = int.Parse(map.TrackTotal);
                }
                catch
                {
                    pair.Value.TrackTotal = 0;
                }
                updated = map.TrackTotal != _filesBeingEditedOriginals[pair.Key].TrackTotal.ToString();
            }
            if (map.AlbumArtist != pair.Value.AlbumArtist && map.AlbumArtist != _("<keep>"))
            {
                pair.Value.AlbumArtist = map.AlbumArtist;
                updated = map.AlbumArtist != _filesBeingEditedOriginals[pair.Key].AlbumArtist;
            }
            if (map.Genre != pair.Value.Genre && map.Genre != _("<keep>"))
            {
                pair.Value.Genre = map.Genre;
                updated = map.Genre != _filesBeingEditedOriginals[pair.Key].Genre;
            }
            if (map.Comment != pair.Value.Comment && map.Comment != _("<keep>"))
            {
                pair.Value.Comment = map.Comment;
                updated = map.Comment != _filesBeingEditedOriginals[pair.Key].Comment;
            }
            if (map.Composer != pair.Value.Composer && map.Composer != _("<keep>"))
            {
                pair.Value.Composer = map.Composer;
                updated = map.Composer != _filesBeingEditedOriginals[pair.Key].Composer;
            }
            if (map.BeatsPerMinute != (pair.Value.BeatsPerMinute == 0 ? "" : pair.Value.BeatsPerMinute.ToString()) && map.BeatsPerMinute != _("<keep>"))
            {
                try
                {
                    pair.Value.BeatsPerMinute = int.Parse(map.BeatsPerMinute);
                }
                catch
                {
                    pair.Value.BeatsPerMinute = 0;
                }
                updated = map.BeatsPerMinute != _filesBeingEditedOriginals[pair.Key].BeatsPerMinute.ToString();
            }
            if (map.Description != pair.Value.Description && map.Description != _("<keep>"))
            {
                pair.Value.Description = map.Description;
                updated = map.Description != _filesBeingEditedOriginals[pair.Key].Description;
            }
            if (map.Publisher != pair.Value.Publisher && map.Publisher != _("<keep>"))
            {
                pair.Value.Publisher = map.Publisher;
                updated = map.Publisher != _filesBeingEditedOriginals[pair.Key].Publisher;
            }
            if (SelectedMusicFiles.Count == 1)
            {
                foreach (var p in map.CustomProperties)
                {
                    if (p.Value != pair.Value.GetCustomProperty(p.Key) && p.Value != _("<keep>"))
                    {
                        pair.Value.SetCustomProperty(p.Key, p.Value);
                        updated = !_filesBeingEditedOriginals[pair.Key].CustomProperties.ContainsKey(p.Key) || p.Value != _filesBeingEditedOriginals[pair.Key].CustomProperties[p.Key];
                    }
                }
            }
            if (updated)
            {
                _musicFileChangedFromUpdate[pair.Key] = true;
            }
            MusicFileSaveStates[pair.Key] = !updated && (MusicFileSaveStates[pair.Key] || _musicFileChangedFromUpdate[pair.Key]);
        }
        if (triggerSelectedMusicFilesPropertiesChanged)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
    }

    /// <summary>
    /// Updates the lyrics of the first selected muisc file
    /// </summary>
    /// <param name="lyrics">LyricsInfo</param>
    public void UpdateLyrics(LyricsInfo lyrics)
    {
        if (SelectedMusicFiles.Count == 1)
        {
            var first = SelectedMusicFiles.First();
            var updated = lyrics.LanguageCode != first.Value.Lyrics.LanguageCode ||
                          lyrics.Description != first.Value.Lyrics.Description ||
                          lyrics.UnsynchronizedLyrics != first.Value.Lyrics.UnsynchronizedLyrics ||
                          !lyrics.SynchronizedLyrics.SequenceEqual(first.Value.Lyrics.SynchronizedLyrics) ||
                          (lyrics.Metadata.TryGetValue("offset", out var offset) && offset != first.Value.Lyrics.Metadata["offset"]);
            first.Value.Lyrics = lyrics;
            if (updated)
            {
                _musicFileChangedFromUpdate[first.Key] = false;
            }
            MusicFileSaveStates[first.Key] = !updated && (MusicFileSaveStates[first.Key] || _musicFileChangedFromUpdate[first.Key]);
            MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
        }
    }

    /// <summary>
    /// Saves all files' tags to disk
    /// </summary>
    /// <param name="triggerMusicFileSaveStatesChanged">Whether or not to trigger the MusicFileSaveStatesChanged event</param>
    public async Task SaveAllTagsAsync(bool triggerMusicFileSaveStatesChanged)
    {
        if (_musicLibrary != null)
        {
            if (triggerMusicFileSaveStatesChanged)
            {
                LoadingStateUpdated?.Invoke(this, _("Saving tags..."));
            }
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var file in _musicLibrary.MusicFiles)
                {
                    if (!MusicFileSaveStates[i])
                    {
                        if (file.SaveTagToDisk(Configuration.Current.PreserveModificationTimestamp))
                        {
                            _musicFileChangedFromUpdate[i] = false;
                            MusicFileSaveStates[i] = true;
                        }
                        else
                        {
                            var path = file.Path.Remove(0, _musicLibrary.Path.Length);
                            if (path[0] == '/')
                            {
                                path = path.Remove(0, 1);
                            }
                            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to save {0}.", path), NotificationSeverity.Warning, "unsupported"));
                        }
                    }
                    i++;
                    UpdateLoadingProgress((i, _musicLibrary.MusicFiles.Count, $"{i}/{_musicLibrary.MusicFiles.Count}"));
                }
            });
            _filesBeingEditedOriginals.Clear();
            if (triggerMusicFileSaveStatesChanged)
            {
                MusicFileSaveStatesChanged?.Invoke(this, false);
            }
        }
    }

    /// <summary>
    /// Saves the selected files' tags
    /// </summary>
    public async Task SaveSelectedTagsAsync()
    {
        if (_musicLibrary != null)
        {
            LoadingStateUpdated?.Invoke(this, _("Saving tags..."));
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var pair in SelectedMusicFiles)
                {
                    if (!MusicFileSaveStates[pair.Key])
                    {
                        if (pair.Value.SaveTagToDisk(Configuration.Current.PreserveModificationTimestamp))
                        {
                            _musicFileChangedFromUpdate[pair.Key] = false;
                            MusicFileSaveStates[pair.Key] = true;
                        }
                        else
                        {
                            var path = pair.Value.Path.Remove(0, _musicLibrary.Path.Length);
                            if (path[0] == '/')
                            {
                                path = path.Remove(0, 1);
                            }
                            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to save {0}", path), NotificationSeverity.Warning, "unsupported"));
                        }
                    }
                    i++;
                    UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
                }
            });
            _filesBeingEditedOriginals.Clear();
            MusicFileSaveStatesChanged?.Invoke(this, false);
        }
    }

    /// <summary>
    /// Discards the selected files' unsaved tag changes
    /// </summary>
    public async Task DiscardSelectedUnappliedChangesAsync()
    {
        if (SelectedHasUnsavedChanges)
        {
            var discarded = false;
            LoadingStateUpdated?.Invoke(this, _("Discarding tags..."));
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var pair in SelectedMusicFiles)
                {
                    if (!MusicFileSaveStates[pair.Key])
                    {
                        pair.Value.ResetTag();
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = true;
                        discarded = true;
                    }
                    i++;
                    UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
                }
            });
            _filesBeingEditedOriginals.Clear();
            if (discarded)
            {
                UpdateSelectedMusicFilesProperties();
            }
            MusicFileSaveStatesChanged?.Invoke(this, false);
        }
    }

    /// <summary>
    /// Deletes the selected files' tags
    /// </summary>
    public async Task DeleteSelectedTagsAsync()
    {
        var deleted = false;
        LoadingStateUpdated?.Invoke(this, _("Deleting tags..."));
        await Task.Run(() =>
        {
            var i = 0;
            foreach (var pair in SelectedMusicFiles)
            {
                if (!pair.Value.IsTagEmpty)
                {
                    pair.Value.ClearTag();
                    _musicFileChangedFromUpdate[pair.Key] = false;
                    MusicFileSaveStates[pair.Key] = false;
                    deleted = true;
                }
                i++;
                UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
            }
        });
        if (deleted)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
    }

    /// <summary>
    /// Converts the selected files' file names to tags
    /// </summary>
    /// <param name="formatString">The format string</param>
    public async Task FilenameToTagAsync(string formatString)
    {
        if (!string.IsNullOrEmpty(formatString))
        {
            var success = 0;
            LoadingStateUpdated?.Invoke(this, _("Converting file names to tags..."));
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var pair in SelectedMusicFiles)
                {
                    if (pair.Value.FilenameToTag(formatString))
                    {
                        success++;
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                    }
                    i++;
                    UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
                }
            });
            if (success > 0)
            {
                UpdateSelectedMusicFilesProperties();
            }
            MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Converted {0} file name to tag successfully", "Converted {0} file names to tags successfully", success, success), NotificationSeverity.Success, "format"));
        }
    }

    /// <summary>
    /// Converts the selected files' tags to file names
    /// </summary>
    /// <param name="formatString">The format string</param>
    public async Task TagToFilenameAsync(string formatString)
    {
        if (!string.IsNullOrEmpty(formatString))
        {
            var success = 0;
            LoadingStateUpdated?.Invoke(this, _("Converting tags to file names..."));
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var pair in SelectedMusicFiles)
                {
                    if (pair.Value.TagToFilename(formatString))
                    {
                        success++;
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                    }
                    i++;
                    UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
                }
            });
            if (success > 0)
            {
                UpdateSelectedMusicFilesProperties();
            }
            MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Converted {0} tag to file name successfully", "Converted {0} tags to file names successfully", success, success), NotificationSeverity.Success, "format"));
        }
    }

    /// <summary>
    /// Inserts the image to the selected files' album art
    /// </summary>
    /// <param name="path">The path to the image</param>
    /// <param name="type">AlbumArtType</param>
    public async Task InsertSelectedAlbumArtAsync(string path, AlbumArtType type)
    {
        byte[] pic = Array.Empty<byte>();
        try
        {
            pic = await File.ReadAllBytesAsync(path);
        }
        catch
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to load image file"), NotificationSeverity.Error));
            return;
        }
        var inserted = false;
        LoadingStateUpdated?.Invoke(this, _("Inserting album art..."));
        await Task.Run(() =>
        {
            var i = 0;
            foreach (var pair in SelectedMusicFiles)
            {
                if (type == AlbumArtType.Front)
                {
                    if (pair.Value.FrontAlbumArt != pic)
                    {
                        pair.Value.FrontAlbumArt = pic;
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                        inserted = true;
                    }
                }
                else if (type == AlbumArtType.Back)
                {
                    if (pair.Value.BackAlbumArt != pic)
                    {
                        pair.Value.BackAlbumArt = pic;
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                        inserted = true;
                    }
                }
                i++;
                UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
            }
        });
        if (inserted)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
    }

    /// <summary>
    /// Removes the selected files' album art
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    public async Task RemoveSelectedAlbumArtAsync(AlbumArtType type)
    {
        var removed = false;
        LoadingStateUpdated?.Invoke(this, _("Removing album art..."));
        await Task.Run(() =>
        {
            var i = 0;
            foreach (var pair in SelectedMusicFiles)
            {
                if (type == AlbumArtType.Front)
                {
                    if (pair.Value.FrontAlbumArt.Length > 0)
                    {
                        pair.Value.FrontAlbumArt = Array.Empty<byte>();
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                        removed = true;
                    }
                }
                else if (type == AlbumArtType.Back)
                {
                    if (pair.Value.BackAlbumArt.Length > 0)
                    {
                        pair.Value.BackAlbumArt = Array.Empty<byte>();
                        _musicFileChangedFromUpdate[pair.Key] = false;
                        MusicFileSaveStates[pair.Key] = false;
                        removed = true;
                    }
                }
                i++;
                UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
            }
        });
        if (removed)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
    }

    /// <summary>
    /// Exports the selected album art of first selected music file
    /// </summary>
    /// <param name="path">The path for new image file</param>
    /// <param name="type">AlbumArtType</param>
    public void ExportSelectedAlbumArt(string path, AlbumArtType type)
    {
        var musicFile = SelectedMusicFiles.First().Value;
        if (type == AlbumArtType.Front)
        {
            if (musicFile.FrontAlbumArt.Length > 0)
            {
                try
                {
                    File.WriteAllBytes(path, musicFile.FrontAlbumArt);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Exported front album art to file successfully"), NotificationSeverity.Success));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Failed to export front album art to a file"), NotificationSeverity.Error));
                }
            }
        }
        else if (type == AlbumArtType.Back)
        {
            if (musicFile.BackAlbumArt.Length > 0)
            {
                try
                {
                    File.WriteAllBytes(path, musicFile.BackAlbumArt);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Exported back album art to file successfully"), NotificationSeverity.Success));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Failed to export back album art to a file"), NotificationSeverity.Error));
                }
            }
        }
    }

    /// <summary>
    /// Adds a custom property to the first selected music files
    /// </summary>
    /// <param name="name">The name of the property to add</param>
    public void AddCustomProperty(string name)
    {
        var musicFile = SelectedMusicFiles.First();
        if (musicFile.Value.SetCustomProperty(name, ""))
        {
            _musicFileChangedFromUpdate[musicFile.Key] = false;
            MusicFileSaveStates[musicFile.Key] = false;
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, true);
        }
    }

    /// <summary>
    /// Removes the custom property from the first selected music files
    /// </summary>
    /// <param name="The name of the property to remove"></param>
    public void RemoveCustomProperty(string name)
    {
        var musicFile = SelectedMusicFiles.First();
        if (musicFile.Value.RemoveCustomProperty(name))
        {
            _musicFileChangedFromUpdate[musicFile.Key] = false;
            MusicFileSaveStates[musicFile.Key] = false;
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, true);
        }
    }

    /// <summary>
    /// Gets MimeType of album art for the first selected file
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    public string GetFirstAlbumArtMimeType(AlbumArtType type)
    {
        if (type == AlbumArtType.Front)
        {
            var art = SelectedMusicFiles.First().Value.FrontAlbumArt;
            return art.Length > 0 ? PictureInfo.fromBinaryData(art, PictureInfo.PIC_TYPE.Front).MimeType : "";
        }
        else if (type == AlbumArtType.Back)
        {
            var art = SelectedMusicFiles.First().Value.BackAlbumArt;
            return art.Length > 0 ? PictureInfo.fromBinaryData(art, PictureInfo.PIC_TYPE.Back).MimeType : "";
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// Downloads MusicBrainz metadata for the selected files
    /// </summary>
    public async Task DownloadMusicBrainzMetadataAsync()
    {
        var i = 0;
        var successful = 0;
        var errors = new Dictionary<string, MusicBrainzLoadStatus>();
        LoadingStateUpdated?.Invoke(this, _("Downloading MusicBrainz metadata..."));
        foreach (var pair in SelectedMusicFiles)
        {
            var res = await pair.Value.DownloadFromMusicBrainzAsync("b'ISSq9E4n", AppInfo.Version, Configuration.Current.OverwriteTagWithMusicBrainz, Configuration.Current.OverwriteAlbumArtWithMusicBrainz);
            if (res == MusicBrainzLoadStatus.Success)
            {
                successful++;
                _musicFileChangedFromUpdate[pair.Key] = false;
                MusicFileSaveStates[pair.Key] = false;
            }
            else
            {
                var p = pair.Value.Path.Remove(0, _musicLibrary!.Path.Length);
                if (p[0] == '/')
                {
                    p = p.Remove(0, 1);
                }
                errors.Add(p, res);
            }
            i++;
            UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
        }
        UpdateSelectedMusicFilesProperties();
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
        var errorString = "";
        foreach (var pair in errors)
        {
            errorString += $"\"{pair.Key}\" - {pair.Value switch
            {
                MusicBrainzLoadStatus.NoAcoustIdResult => _("No AcoustId entry found for the file's fingerprint"),
                MusicBrainzLoadStatus.NoAcoustIdRecordingId => _("No MusicBrainz RecordingId was provided for the AcoustId entry"),
                MusicBrainzLoadStatus.InvalidMusicBrainzRecordingId => _("An invalid RecordingId was provided to MusicBrainz"),
                MusicBrainzLoadStatus.InvalidFingerprint => _("This file does not have a valid fingerprint"),
                _ => _("Error")
            }}\n\n";
        }
        if (!string.IsNullOrEmpty(errorString))
        {
            errorString = errorString.Remove(errorString.Length - 2);
        }
        NotificationSent?.Invoke(this, new NotificationSentEventArgs(successful > 0 ? _("Downloaded metadata for {0} files successfully", successful) : _("No metadata was downloaded"), successful > 0 ? NotificationSeverity.Success : NotificationSeverity.Error, "musicbrainz", errorString));
    }

    /// <summary>
    /// Downloads lyrics for the music file
    /// </summary>
    public async Task DownloadLyricsAsync()
    {
        var i = 0;
        var successful = 0;
        LoadingStateUpdated?.Invoke(this, _("Downloading lyrics..."));
        foreach (var pair in SelectedMusicFiles)
        {
            var res = await pair.Value.DownloadLyricsAsync(Configuration.Current.OverwriteLyricsWithWebService);
            if (res)
            {
                successful++;
                _musicFileChangedFromUpdate[pair.Key] = false;
                MusicFileSaveStates[pair.Key] = false;
            }
            i++;
            UpdateLoadingProgress((i, SelectedMusicFiles.Count, $"{i}/{SelectedMusicFiles.Count}"));
        }
        MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
        NotificationSent?.Invoke(this, new NotificationSentEventArgs(successful > 0 ? _("Downloaded lyrics for {0} files successfully", successful) : _("No lyrics were downloaded"), successful > 0 ? NotificationSeverity.Success : NotificationSeverity.Error, "web"));
    }

    /// <summary>
    /// Submits tag information to AcoustId for the selected file
    /// </summary>
    /// <param name="recordingID">The MusicBrainz Recording Id to associate, if available</param>
    public async Task SubmitToAcoustIdAsync(string? recordingID)
    {
        if (SelectedMusicFiles.Count == 1)
        {
            if (string.IsNullOrEmpty(Configuration.Current.AcoustIdUserAPIKey))
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("No user api key configured."), NotificationSeverity.Error));
                return;
            }
            LoadingStateUpdated?.Invoke(this, _("Submitting data to AcoustId..."));
            var result = await SelectedMusicFiles.First().Value.SubmitToAcoustIdAsync("b'Ch3cuJ0d", Configuration.Current.AcoustIdUserAPIKey, recordingID);
            MusicFileSaveStatesChanged?.Invoke(this, MusicFileSaveStates.Any(x => !x));
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(result ? _("Submitted metadata to AcoustId successfully") : _("Unable to submit to AcoustId. Check API key"), result ? NotificationSeverity.Success : NotificationSeverity.Error, "web"));
        }
    }

    /// <summary>
    /// Performs an advanced search
    /// </summary>
    /// <param name="s">The search string in the format: !prop1="value1";prop2="value2"</param>
    /// <returns>A bool based on whether or not the search was successful and a list of lowercase filenames matching the search</returns>
    public (bool Success, List<string>? LowerFilenames) AdvancedSearch(string s)
    {
        if (_musicLibrary != null)
        {
            //Parse Search String
            if (string.IsNullOrEmpty(s) || s[0] != '!')
            {
                return (false, null);
            }
            var search = s.Substring(1).ToLower();
            if (string.IsNullOrEmpty(search))
            {
                return (false, null);
            }
            var propValPairs = search.Split(';');
            var validProperties = new string[] { "filename", _("filename"), "title", _("title"), "artist", _("artist"), "album", _("album"), "year", _("year"), "track", _("track"), "tracktotal", _("tracktotal"), "albumartist", _("albumartist"), "genre", _("genre"), "comment", _("comment"), "beatsperminute", _("beatsperminute"), "bpm", _("bpm"), "composer", _("composer"), "description", _("description"), "publisher", _("publisher"), "custom", _("custom") };
            var propertyMap = new PropertyMap();
            var customPropName = "";
            foreach (var propVal in propValPairs)
            {
                var fields = propVal.Split('=');
                if (fields.Length != 2)
                {
                    return (false, null);
                }
                var prop = fields[0].ToLower();
                var val = fields[1];
                if (!validProperties.Contains(prop))
                {
                    return (false, null);
                }
                if (val.Length <= 1 || val.Substring(0, 1) != "\"" || val.Substring(val.Length - 1) != "\"")
                {
                    return (false, null);
                }
                val = val.Remove(0, 1);
                val = val.Remove(val.Length - 1, 1);
                if (string.IsNullOrEmpty(val))
                {
                    val = "NULL";
                }
                if (prop == "filename" || prop == _("filename"))
                {
                    propertyMap.Filename = val;
                }
                else if (prop == "title" || prop == _("title"))
                {
                    propertyMap.Title = val;
                }
                else if (prop == "artist" || prop == _("artist"))
                {
                    propertyMap.Artist = val;
                }
                else if (prop == "album" || prop == _("album"))
                {
                    propertyMap.Album = val;
                }
                else if (prop == "year" || prop == _("year"))
                {
                    if (val != "NULL")
                    {
                        try
                        {
                            int.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.Year = val;
                }
                else if (prop == "track" || prop == _("track"))
                {
                    if (val != "NULL")
                    {
                        try
                        {
                            int.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.Track = val;
                }
                else if (prop == "tracktotal" || prop == _("tracktotal"))
                {
                    if (val != "NULL")
                    {
                        try
                        {
                            int.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.TrackTotal = val;
                }
                else if (prop == "albumartist" || prop == _("albumartist"))
                {
                    propertyMap.AlbumArtist = val;
                }
                else if (prop == "genre" || prop == _("genre"))
                {
                    propertyMap.Genre = val;
                }
                else if (prop == "comment" || prop == _("comment"))
                {
                    propertyMap.Comment = val;
                }
                else if (prop == "beatsperminute" || prop == _("beatsperminute") || prop == "bpm" || prop == _("bpm"))
                {
                    if (val != "NULL")
                    {
                        try
                        {
                            int.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.BeatsPerMinute = val;
                }
                else if (prop == "composer" || prop == _("composer"))
                {
                    propertyMap.Composer = val;
                }
                else if (prop == "description" || prop == _("description"))
                {
                    propertyMap.Description = val;
                }
                else if (prop == "publisher" || prop == _("publisher"))
                {
                    propertyMap.Publisher = val;
                }
                else if (prop == "custom" || prop == _("custom"))
                {
                    customPropName = val;
                }
            }
            var matches = new List<string>();
            var ratios = new Dictionary<string, int>();
            //Test Files
            foreach (var musicFile in _musicLibrary.MusicFiles)
            {
                var ratio = 0;
                if (TestAdvancedSearchShouldSkip(musicFile.Filename, propertyMap.Filename, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Title, propertyMap.Title, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Artist, propertyMap.Artist, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Album, propertyMap.Album, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Year, propertyMap.Year, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Track, propertyMap.Track, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.TrackTotal, propertyMap.TrackTotal, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.AlbumArtist, propertyMap.AlbumArtist, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Genre, propertyMap.Genre, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Comment, propertyMap.Comment, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.BeatsPerMinute, propertyMap.BeatsPerMinute, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Composer, propertyMap.Composer, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Description, propertyMap.Description, ref ratio))
                {
                    continue;
                }
                if (TestAdvancedSearchShouldSkip(musicFile.Publisher, propertyMap.Publisher, ref ratio))
                {
                    continue;
                }
                //Check for custom property
                if (!string.IsNullOrEmpty(customPropName))
                {
                    if (customPropName == "NULL")
                    {
                        continue;
                    }
                    ratio = 100;
                    if (!musicFile.CustomPropertyNames.Select(x => x.ToLower()).Contains(customPropName))
                    {
                        continue;
                    }
                }
                matches.Add(musicFile.Filename.ToLower());
                ratios.Add(musicFile.Filename.ToLower(), ratio);
            }
            return (true, matches.Where(x => ratios.ContainsKey(x)).OrderByDescending(x => ratios[x]).ToList());
        }
        return (false, null);
    }

    /// <summary>
    /// Updates the list of selected music files from a list of selected indexes
    /// </summary>
    /// <param name="indexes">The list of selected indexes</param>
    public void UpdateSelectedMusicFiles(List<int> indexes)
    {
        SelectedMusicFiles.Clear();
        if (_musicLibrary != null)
        {
            foreach (var index in indexes)
            {
                SelectedMusicFiles.Add(index, _musicLibrary.MusicFiles[index]);
            }
        }
        UpdateSelectedMusicFilesProperties();
    }

    /// <summary>
    /// Gets a list of suggestions for the provided genre
    /// </summary>
    /// <param name="genre">The genre text to get suggestions for</param>
    /// <returns>The list of suggestions</returns>
    public List<string> GetGenreSuggestions(string genre)
    {
        if (_musicLibrary != null)
        {
            return _genreSuggestions.Union(_musicLibrary.Genres)
                .Where(x => Fuzz.PartialRatio(x.ToLower(), genre.ToLower()) > 75)
                .OrderByDescending(x => Fuzz.PartialRatio(x.ToLower(), genre.ToLower()))
                .Take(5).ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Gets a header for a MusicFile
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    /// <returns>The header for the MusicFile or null if not supported by sorting type</returns>
    public string? GetHeaderForMusicFile(MusicFile musicFile)
    {
        var header = SortFilesBy switch
        {
            SortBy.Album => musicFile.Album,
            SortBy.Artist => musicFile.Artist,
            SortBy.Genre => musicFile.Genre,
            SortBy.Path => Path.GetDirectoryName(musicFile.Path)!.Replace(MusicLibraryType == MusicLibraryType.Folder ? MusicLibraryName : UserDirectories.Home, MusicLibraryType == MusicLibraryType.Folder ? "" : "~"),
            SortBy.Year => musicFile.Year.ToString(),
            _ => null
        };
        if (!string.IsNullOrEmpty(header) && header[0] == Path.DirectorySeparatorChar && !Directory.Exists(header))
        {
            header = header.Remove(0, 1);
        }
        if (header == string.Empty)
        {
            header = SortFilesBy != SortBy.Path ? _("Unknown") : "";
        }
        if (header != null && header.Contains($"{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}"))
        {
            header = header.Substring(header.LastIndexOf($"{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}") + 1);
        }
        return header;
    }

    /// <summary>
    /// Occurs when the configuration is saved
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">EventArgs</param>
    private async void ConfigurationSaved(object? sender, EventArgs e)
    {
        if (_musicLibrary != null)
        {
            var includeSubfoldersChanged = _musicLibrary.IncludeSubfolders != Configuration.Current.IncludeSubfolders;
            var sortingChanged = _musicLibrary.SortFilesBy != Configuration.Current.SortFilesBy;
            if (includeSubfoldersChanged || sortingChanged)
            {
                LoadingStateUpdated?.Invoke(this, _("Loading music files from library..."));
                _musicLibrary.IncludeSubfolders = Configuration.Current.IncludeSubfolders;
                _musicLibrary.SortFilesBy = Configuration.Current.SortFilesBy;
                await ReloadLibraryAsync();
            }
        }
    }

    /// <summary>
    /// Triggers the loading progress updated event while updating taskbar icon
    /// </summary>
    /// <param name="e">(int Value, int MaxValue, string Message)</param>
    private void UpdateLoadingProgress((int Value, int MaxValue, string Message) e)
    {
        if (_taskbarItem != null)
        {
            _taskbarItem.Progress = (double)e.Value / (double)e.MaxValue;
            _taskbarItem.ProgressState = e.Value == e.MaxValue ? ProgressFlags.NoProgress : ProgressFlags.Normal;
        }
        LoadingProgressUpdated?.Invoke(this, e);
    }

    /// <summary>
    /// Updates the property map of the selected music files
    /// </summary>
    private void UpdateSelectedMusicFilesProperties()
    {
        SelectedPropertyMap.Clear();
        if (SelectedMusicFiles.Count == 1)
        {
            var first = SelectedMusicFiles.First().Value;
            SelectedPropertyMap.Filename = first.Filename;
            SelectedPropertyMap.Title = first.Title;
            SelectedPropertyMap.Artist = first.Artist;
            SelectedPropertyMap.Album = first.Album;
            SelectedPropertyMap.Year = first.Year == 0 ? "" : first.Year.ToString();
            SelectedPropertyMap.Track = first.Track == 0 ? "" : first.Track.ToString();
            SelectedPropertyMap.TrackTotal = first.TrackTotal == 0 ? "" : first.TrackTotal.ToString();
            SelectedPropertyMap.AlbumArtist = first.AlbumArtist;
            SelectedPropertyMap.Genre = first.Genre;
            SelectedPropertyMap.Comment = first.Comment;
            SelectedPropertyMap.BeatsPerMinute = first.BeatsPerMinute == 0 ? "" : first.BeatsPerMinute.ToString();
            SelectedPropertyMap.Composer = first.Composer;
            SelectedPropertyMap.Description = first.Description;
            SelectedPropertyMap.Publisher = first.Publisher;
            SelectedPropertyMap.Duration = first.Duration.ToDurationString();
            SelectedPropertyMap.Fingerprint = _("Calculating...");
            Task.Run(() =>
            {
                var fingerprint = first.Fingerprint;
                if (first == SelectedMusicFiles.First().Value && SelectedMusicFiles.Count == 1) //make sure this file is still selected
                {
                    SelectedPropertyMap.Fingerprint = string.IsNullOrEmpty(fingerprint) ? _("Calculating...") : fingerprint;
                    FingerprintCalculated?.Invoke(this, EventArgs.Empty);
                }
            });
            SelectedPropertyMap.FileSize = first.FileSize.ToFileSizeString();
            SelectedPropertyMap.FrontAlbumArt = first.FrontAlbumArt.Length == 0 ? "noArt" : "hasArt";
            SelectedPropertyMap.BackAlbumArt = first.BackAlbumArt.Length == 0 ? "noArt" : "hasArt";
            foreach (var custom in first.CustomPropertyNames)
            {
                SelectedPropertyMap.CustomProperties.Add(custom, first.GetCustomProperty(custom)!);
            }
        }
        else if (SelectedMusicFiles.Count > 1)
        {
            var first = SelectedMusicFiles.First().Value;
            var haveSameTitle = true;
            var haveSameArtist = true;
            var haveSameAlbum = true;
            var haveSameYear = true;
            var haveSameTrack = true;
            var haveSameTrackTotal = true;
            var haveSameAlbumArtist = true;
            var haveSameGenre = true;
            var haveSameComment = true;
            var haveSameBPM = true;
            var haveSameComposer = true;
            var haveSameDescription = true;
            var haveSamePublisher = true;
            var haveSameFrontAlbumArt = true;
            var haveSameBackAlbumArt = true;
            var totalDuration = 0;
            var totalFileSize = 0L;
            foreach (var pair in SelectedMusicFiles)
            {
                if (first.Title != pair.Value.Title)
                {
                    haveSameTitle = false;
                }
                if (first.Artist != pair.Value.Artist)
                {
                    haveSameArtist = false;
                }
                if (first.Album != pair.Value.Album)
                {
                    haveSameAlbum = false;
                }
                if (first.Year != pair.Value.Year)
                {
                    haveSameYear = false;
                }
                if (first.Track != pair.Value.Track)
                {
                    haveSameTrack = false;
                }
                if (first.TrackTotal != pair.Value.TrackTotal)
                {
                    haveSameTrackTotal = false;
                }
                if (first.AlbumArtist != pair.Value.AlbumArtist)
                {
                    haveSameAlbumArtist = false;
                }
                if (first.Genre != pair.Value.Genre)
                {
                    haveSameGenre = false;
                }
                if (first.Comment != pair.Value.Comment)
                {
                    haveSameComment = false;
                }
                if (first.BeatsPerMinute != pair.Value.BeatsPerMinute)
                {
                    haveSameBPM = false;
                }
                if (first.Composer != pair.Value.Composer)
                {
                    haveSameComposer = false;
                }
                if (first.Description != pair.Value.Description)
                {
                    haveSameDescription = false;
                }
                if (first.Publisher != pair.Value.Publisher)
                {
                    haveSamePublisher = false;
                }
                if (!first.FrontAlbumArt.SequenceEqual(pair.Value.FrontAlbumArt))
                {
                    haveSameFrontAlbumArt = false;
                }
                if (!first.BackAlbumArt.SequenceEqual(pair.Value.BackAlbumArt))
                {
                    haveSameBackAlbumArt = false;
                }
                totalDuration += pair.Value.Duration;
                totalFileSize += pair.Value.FileSize;
            }
            SelectedPropertyMap.Filename = _("<keep>");
            SelectedPropertyMap.Title = haveSameTitle ? first.Title : _("<keep>");
            SelectedPropertyMap.Artist = haveSameArtist ? first.Artist : _("<keep>");
            SelectedPropertyMap.Album = haveSameAlbum ? first.Album : _("<keep>");
            SelectedPropertyMap.Year = haveSameYear ? (first.Year == 0 ? "" : first.Year.ToString()) : _("<keep>");
            SelectedPropertyMap.Track = haveSameTrack ? (first.Track == 0 ? "" : first.Track.ToString()) : _("<keep>");
            SelectedPropertyMap.TrackTotal = haveSameTrackTotal ? (first.TrackTotal == 0 ? "" : first.TrackTotal.ToString()) : _("<keep>");
            SelectedPropertyMap.AlbumArtist = haveSameAlbumArtist ? first.AlbumArtist : _("<keep>");
            SelectedPropertyMap.Genre = haveSameGenre ? first.Genre : _("<keep>");
            SelectedPropertyMap.Comment = haveSameComment ? first.Comment : _("<keep>");
            SelectedPropertyMap.BeatsPerMinute = haveSameBPM ? (first.BeatsPerMinute == 0 ? "" : first.BeatsPerMinute.ToString()) : _("<keep>");
            SelectedPropertyMap.Composer = haveSameComposer ? first.Composer : _("<keep>");
            SelectedPropertyMap.Description = haveSameDescription ? first.Description : _("<keep>");
            SelectedPropertyMap.Publisher = haveSamePublisher ? first.Publisher : _("<keep>");
            SelectedPropertyMap.FrontAlbumArt = haveSameFrontAlbumArt ? (first.FrontAlbumArt.Length == 0 ? "noArt" : "hasArt") : "keepArt";
            SelectedPropertyMap.BackAlbumArt = haveSameBackAlbumArt ? (first.BackAlbumArt.Length == 0 ? "noArt" : "hasArt") : "keepArt";
            SelectedPropertyMap.Duration = totalDuration.ToDurationString();
            SelectedPropertyMap.Fingerprint = _("<keep>");
            SelectedPropertyMap.FileSize = totalFileSize.ToFileSizeString();
        }
        SelectedMusicFilesPropertiesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Tests the value of a music file with the value from a PropertyMap for advanced search
    /// </summary>
    /// <param name="fileValue">The string value from the file</param>
    /// <param name="propValue">The value from the PropertyMap</param>
    /// <param name="ratio">A variable to store the similarity ratio</param>
    /// <returns>True to skip the file as a match, else false</returns>
    private bool TestAdvancedSearchShouldSkip(string fileValue, string propValue, ref int ratio)
    {
        if (!string.IsNullOrEmpty(propValue))
        {
            fileValue = fileValue.ToLower();
            if (propValue == "NULL")
            {
                if (!string.IsNullOrEmpty(fileValue))
                {
                    return true;
                }
            }
            else
            {
                ratio = Fuzz.PartialRatio(fileValue.Normalize(NormalizationForm.FormKD), propValue.Normalize(NormalizationForm.FormKD));
                if (ratio < 65)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Tests the value of a music file with the value from a PropertyMap for advanced search
    /// </summary>
    /// <param name="fileValue">The int value from the file</param>
    /// <param name="propValue">The value from the PropertyMap</param>
    /// <param name="ratio">A variable to store the similarity ratio</param>
    /// <returns>True to skip the file as a match, else false</returns>
    private bool TestAdvancedSearchShouldSkip(int fileValue, string propValue, ref int ratio)
    {
        if (!string.IsNullOrEmpty(propValue))
        {
            if (propValue == "NULL")
            {
                if (fileValue != 0)
                {
                    return true;
                }
            }
            else
            {
                ratio = 100;
                if (fileValue != int.Parse(propValue))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
