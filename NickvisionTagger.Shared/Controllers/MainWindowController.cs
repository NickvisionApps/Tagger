using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    private MusicFolder? _musicFolder;
    private bool _forceAllowClose;

    /// <summary>
    /// The list of music file save states
    /// </summary>
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
    public event EventHandler<EventArgs> MusicFileSaveStatesChanged;
    /// <summary>
    /// Occurs when the selected music files' properties are changed
    /// </summary>
    public event EventHandler<EventArgs> SelectedMusicFilesPropertiesChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        _musicFolder = null;
        _forceAllowClose = false;
        MusicFileSaveStates = new List<bool>();
        SelectedMusicFiles = new Dictionary<int, MusicFile>();
        SelectedPropertyMap = new PropertyMap();
    }

    /// <summary>
    /// Whether or not the window can close freely
    /// </summary>
    public bool CanClose
    {
        get
        {
            if(_forceAllowClose)
            {
                return true;
            }
            foreach(var saved in MusicFileSaveStates)
            {
                if(!saved)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Starts the application
    /// </summary>
    public async Task StartupAsync()
    {
        Configuration.Current.Saved += ConfigurationSaved;
        if(Configuration.Current.RememberLastOpenedFolder && Directory.Exists(Configuration.Current.LastOpenedFolder))
        {
            await OpenFolderAsync(Configuration.Current.LastOpenedFolder);
        }
        else
        {
            MusicFolderUpdated?.Invoke(this, true);
        }
    }

    /// <summary>
    /// Forces CanClose to be true
    /// </summary>
    public void ForceAllowClose() => _forceAllowClose = true;

    /// <summary>
    /// Opens a music folder
    /// </summary>
    /// <param name="path">The path to the music folder</param>
    public async Task OpenFolderAsync(string path)
    {
        _musicFolder = new MusicFolder(path);
        _musicFolder.IncludeSubfolders = Configuration.Current.IncludeSubfolders;
        if(Configuration.Current.RememberLastOpenedFolder)
        {
            Configuration.Current.LastOpenedFolder = _musicFolder.ParentPath;
            Configuration.Current.Save();
        }
        await ReloadFolderAsync();
    }

    /// <summary>
    /// Closes a music folder
    /// </summary>
    public void CloseFolder()
    {
        _musicFolder = null;
        MusicFileSaveStates.Clear();
        SelectedMusicFiles.Clear();
        if(Configuration.Current.RememberLastOpenedFolder)
        {
            Configuration.Current.LastOpenedFolder = "";
            Configuration.Current.Save();
        }
        MusicFolderUpdated?.Invoke(this, true);
    }

    /// <summary>
    /// Reloads the music folder
    /// </summary>
    public async Task ReloadFolderAsync()
    {
        if(_musicFolder != null)
        {
            MusicFileSaveStates.Clear();
            await _musicFolder.ReloadMusicFilesAsync();
            for(var i = 0; i < _musicFolder.MusicFiles.Count; i++)
            {
                MusicFileSaveStates.Add(true);
            }
            MusicFolderUpdated?.Invoke(this, true);
        }
    }

    /// <summary>
    /// Updates the tags with values from the property map
    /// </summary>
    /// <param name="map">The PropertyMap with values</param>
    /// <param name="triggerSelectedMusicFilesPropertiesChanged">Whether or not to trigger the SelectedMusicFilesPropertiesChanged event</param>
    public void UpdateTags(PropertyMap map, bool triggerSelectedMusicFilesPropertiesChanged)
    {
        foreach(var pair in SelectedMusicFiles)
        {
            var updated = false;
            if(map.Filename != pair.Value.Filename && map.Filename != "<keep>")
            {
                try
                {
                    pair.Value.Filename = map.Filename;
                    updated = true;
                }
                catch { }
            }
            if(map.Title != pair.Value.Title && map.Title != "<keep>")
            {
                pair.Value.Title = map.Title;
                updated = true;
            }
            if(map.Artist != pair.Value.Artist && map.Artist != "<keep>")
            {
                pair.Value.Artist = map.Artist;
                updated = true;
            }
            if(map.Album != pair.Value.Album && map.Album != "<keep>")
            {
                pair.Value.Album = map.Album;
                updated = true;
            }
            if(map.Year != pair.Value.Year.ToString() && map.Year != "<keep>")
            {
                try
                {
                    pair.Value.Year = uint.Parse(map.Year);
                    updated = true;
                }
                catch { }
            }
            if(map.Track != pair.Value.Track.ToString() && map.Track != "<keep>")
            {
                try
                {
                    pair.Value.Track = uint.Parse(map.Track);
                    updated = true;
                }
                catch { }
            }
            if(map.AlbumArtist != pair.Value.AlbumArtist && map.AlbumArtist != "<keep>")
            {
                pair.Value.AlbumArtist = map.AlbumArtist;
                updated = true;
            }
            if(map.Genre != pair.Value.Genre && map.Genre != "<keep>")
            {
                pair.Value.Genre = map.Genre;
                updated = true;
            }
            if(map.Comment != pair.Value.Comment && map.Comment != "<keep>")
            {
                pair.Value.Comment = map.Comment;
                updated = true;
            }
            MusicFileSaveStates[pair.Key] = !updated;
        }
        if(triggerSelectedMusicFilesPropertiesChanged)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Saves all files' tags to disk
    /// </summary>
    /// <param name="triggerMusicFileSaveStatesChanged">Whether or not to trigger the MusicFileSaveStatesChanged event</param>
    public async Task SaveAllTagsAsync(bool triggerMusicFileSaveStatesChanged)
    {
        if(_musicFolder != null)
        {
            await Task.Run(() =>
            {
                var i = 0;
                foreach(var file in _musicFolder.MusicFiles)
                {
                    if(!MusicFileSaveStates[i])
                    {
                        file.SaveTagToDisk(Configuration.Current.PreserveModificationTimestamp);
                        MusicFileSaveStates[i] = true;
                    }
                    i++;
                }
            });
            if(triggerMusicFileSaveStatesChanged)
            {
                MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Saves the selected files' tags
    /// </summary>
    public async Task SaveSelectedTagsAsync()
    {
        if(_musicFolder != null)
        {
            await Task.Run(() =>
            {
                foreach(var pair in SelectedMusicFiles)
                {
                    if(!MusicFileSaveStates[pair.Key])
                    {
                        pair.Value.SaveTagToDisk(Configuration.Current.PreserveModificationTimestamp);
                        MusicFileSaveStates[pair.Key] = true;
                    }
                }
            });
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Discards the selected files' unsaved tag changes
    /// </summary>
    public async Task DiscardSelectedUnappliedChangesAsync()
    {
        var discarded = false;
        await Task.Run(() =>
        {
            foreach(var pair in SelectedMusicFiles)
            {
                if(!MusicFileSaveStates[pair.Key])
                {
                    pair.Value.LoadTagFromDisk();
                    MusicFileSaveStates[pair.Key] = true;
                    discarded = true;
                }
            }
        });
        if(discarded)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Deletes the selected files' tags
    /// </summary>
    public void DeleteSelectedTags()
    {
        var deleted = false;
        foreach(var pair in SelectedMusicFiles)
        {
            if(!pair.Value.IsTagEmpty)
            {
                pair.Value.ClearTag();
                MusicFileSaveStates[pair.Key] = false;
                deleted = true;
            }
        }
        if(deleted)
        {
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Converts the selected files' file names to tags
    /// </summary>
    /// <param name="formatString">The format string</param>
    public void FilenameToTag(string formatString)
    {
        if(!string.IsNullOrEmpty(formatString))
        {
            var success = 0;
            foreach(var pair in SelectedMusicFiles)
            {
                if(pair.Value.FilenameToTag(formatString))
                {
                    success++;
                    MusicFileSaveStates[pair.Key] = false;
                }
            }
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(string.Format(_("Converted {0} file names to tags successfully"), success), NotificationSeverity.Success));
        }
    }

    /// <summary>
    /// Converts the selected files' tags to file names
    /// </summary>
    /// <param name="formatString">The format string</param>
    public void TagToFilename(string formatString)
    {
        if(_musicFolder != null && !string.IsNullOrEmpty(formatString))
        {
            var success = 0;
            foreach(var pair in SelectedMusicFiles)
            {
                if(pair.Value.TagToFilename(formatString))
                {
                    success++;
                    MusicFileSaveStates[pair.Key] = false;
                }
            }
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(string.Format(_("Converted {0} tags to file names successfully"), success), NotificationSeverity.Success));

        }
    }

    /// <summary>
    /// Inserts the image to the selected files' album art
    /// </summary>
    /// <param="path">The path to the image</param>
    public async Task InsertSelectedAlbumArtAsync(string path)
    {
        TagLib.ByteVector? byteVector = null;
        try
        {
            byteVector = TagLib.ByteVector.FromPath(path);
        }
        catch { }
        if(byteVector == null)
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to load image file"), NotificationSeverity.Error));
        }
        else
        {
            var inserted = false;
            foreach(var pair in SelectedMusicFiles)
            {
                if(pair.Value.AlbumArt != byteVector)
                {
                    pair.Value.AlbumArt = byteVector;
                    MusicFileSaveStates[pair.Key] = false;
                    inserted = true;
                }
            }
            if(inserted)
            {
                UpdateSelectedMusicFilesProperties();
            }
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Removes the selected files' album art
    /// </summary>
    public void RemoveSelectedAlbumArt()
    {
        var removed = false;
        foreach(var pair in SelectedMusicFiles)
        {
            if(!pair.Value.AlbumArt.IsEmpty)
            {
                pair.Value.AlbumArt = new TagLib.ByteVector();
                MusicFileSaveStates[pair.Key] = false;
                removed = true;
            }
        }
        if(removed)
        {
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Downloads MusicBrainz metadata for the selected files
    /// </summary>
    public async Task DownloadMusicBrainzMetadataAsync()
    {
        var successful = 0;
        foreach(var pair in SelectedMusicFiles)
        {
            if(await pair.Value.LoadTagFromMusicBrainzAsync("b'Ch3cuJ0d", AppInfo.Current, Configuration.Current.OverwriteTagWithMusicBrainz, Configuration.Current.OverwriteAlbumArtWithMusicBrainz))
            {
                successful++;
                MusicFileSaveStates[pair.Key] = false;
            }
        }
        UpdateSelectedMusicFilesProperties();
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        NotificationSent?.Invoke(this, new NotificationSentEventArgs(string.Format(_("Downloaded metadata for {0} files successfully"), successful), NotificationSeverity.Success));
    }

    /// <summary>
    /// Updates the list of selected music files from a list of selected indexes
    /// </summary>
    /// <param name="indexes">The list of selected indexes</param>
    public void UpdateSelectedMusicFiles(List<int> indexes)
    {
        SelectedMusicFiles.Clear();
        SelectedPropertyMap.Clear();
        foreach(var index in indexes)
        {
            SelectedMusicFiles.Add(index, _musicFolder.MusicFiles[index]);
        }
        UpdateSelectedMusicFilesProperties();
    }

    /// <summary>
    /// Updates the property map of the selected music files
    /// </summary>
    private void UpdateSelectedMusicFilesProperties()
    {
        if(SelectedMusicFiles.Count == 1)
        {
            var first = SelectedMusicFiles.First().Value;
            SelectedPropertyMap.Filename = first.Filename;
            SelectedPropertyMap.Title = first.Title;
            SelectedPropertyMap.Artist = first.Artist;
            SelectedPropertyMap.Album = first.Album;
            SelectedPropertyMap.Year = first.Year.ToString();
            SelectedPropertyMap.Track = first.Track.ToString();
            SelectedPropertyMap.AlbumArtist = first.AlbumArtist;
            SelectedPropertyMap.Genre = first.Genre;
            SelectedPropertyMap.Comment = first.Comment;
            SelectedPropertyMap.Duration = first.Duration.ToDurationString();
            SelectedPropertyMap.Fingerprint = first.Fingerprint;
            SelectedPropertyMap.FileSize = first.FileSize.ToFileSizeString();
            SelectedPropertyMap.AlbumArt = first.AlbumArt.IsEmpty ? "noArt" : "hasArt";
        }
        else if(SelectedMusicFiles.Count > 1)
        {
            var first = SelectedMusicFiles.First().Value;
            var haveSameTitle = true;
            var haveSameArtist = true;
            var haveSameAlbum = true;
            var haveSameYear = true;
            var haveSameTrack = true;
            var haveSameAlbumArtist = true;
            var haveSameGenre = true;
            var haveSameComment = true;
            var haveSameAlbumArt = true;
            var totalDuration = 0;
            var totalFileSize = 0l;
            foreach(var pair in SelectedMusicFiles)
            {
                if(first.Title != pair.Value.Title)
                {
                    haveSameTitle = false;
                }
                if(first.Artist != pair.Value.Artist)
                {
                    haveSameArtist = false;
                }
                if(first.Album != pair.Value.Album)
                {
                    haveSameAlbum = false;
                }
                if(first.Year != pair.Value.Year)
                {
                    haveSameYear = false;
                }
                if(first.Track != pair.Value.Track)
                {
                    haveSameTrack = false;
                }
                if(first.AlbumArtist != pair.Value.AlbumArtist)
                {
                    haveSameAlbumArtist = false;
                }
                if(first.Genre != pair.Value.Genre)
                {
                    haveSameGenre = false;
                }
                if(first.Comment != pair.Value.Comment)
                {
                    haveSameComment = false;
                }
                if(first.AlbumArt != pair.Value.AlbumArt)
                {
                    haveSameAlbumArt = false;
                }
                totalDuration += pair.Value.Duration;
                totalFileSize += pair.Value.FileSize;
            }
            SelectedPropertyMap.Filename = "<keep>";
            SelectedPropertyMap.Title = haveSameTitle ? first.Title : "<keep>";
            SelectedPropertyMap.Artist = haveSameArtist ? first.Artist : "<keep>";
            SelectedPropertyMap.Album = haveSameAlbum ? first.Album : "<keep>";
            SelectedPropertyMap.Year = haveSameYear ? first.Year.ToString() : "<keep>";
            SelectedPropertyMap.Track = haveSameTrack ? first.Track.ToString() : "<keep>";
            SelectedPropertyMap.AlbumArtist = haveSameAlbumArtist ? first.AlbumArtist : "<keep>";
            SelectedPropertyMap.Genre = haveSameGenre ? first.Genre : "<keep>";
            SelectedPropertyMap.Comment = haveSameComment ? first.Comment : "<keep>";
            SelectedPropertyMap.AlbumArt = haveSameAlbumArt ? (first.AlbumArt.IsEmpty ? "noArt" : "hasArt") : "keepArt";
            SelectedPropertyMap.Duration = totalDuration.ToDurationString();
            SelectedPropertyMap.Fingerprint = "<keep>";
            SelectedPropertyMap.FileSize = totalFileSize.ToFileSizeString();
        }
        SelectedMusicFilesPropertiesChanged?.Invoke(this, EventArgs.Empty);
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
