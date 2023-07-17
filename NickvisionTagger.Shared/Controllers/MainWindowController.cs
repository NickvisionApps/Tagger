using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
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
    /// The list of paths to corrupted music files in the music folder
    /// </summary>
    public List<string> CorruptedFiles => _musicFolder?.CorruptedFiles ?? new List<string>();

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
    public event EventHandler<string> LoadingStateUpdated;
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
    /// Occurs when fingerprint calculating is done
    /// </summary>
    public event EventHandler<EventArgs> FingerprintCalculated;
    /// <summary>
    /// Occurs when there are corrupted music files found in a music folder
    /// </summary>
    public event EventHandler<EventArgs> CorruptedFilesFound;

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
        _musicFolder = new MusicFolder(path)
        {
            IncludeSubfolders = Configuration.Current.IncludeSubfolders,
            SortFilesBy = Configuration.Current.SortFilesBy
        };
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
            var corruptedFound = await _musicFolder.ReloadMusicFilesAsync();
            for(var i = 0; i < _musicFolder.MusicFiles.Count; i++)
            {
                MusicFileSaveStates.Add(true);
            }
            MusicFolderUpdated?.Invoke(this, true);
            if(corruptedFound)
            {
                CorruptedFilesFound?.Invoke(this, EventArgs.Empty);
            }
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
            if(map.Filename != pair.Value.Filename && map.Filename != _("<keep>"))
            {
                try
                {
                    pair.Value.Filename = map.Filename;
                    updated = true;
                }
                catch { }
            }
            if(map.Title != pair.Value.Title && map.Title != _("<keep>"))
            {
                pair.Value.Title = map.Title;
                updated = true;
            }
            if(map.Artist != pair.Value.Artist && map.Artist != _("<keep>"))
            {
                pair.Value.Artist = map.Artist;
                updated = true;
            }
            if(map.Album != pair.Value.Album && map.Album != _("<keep>"))
            {
                pair.Value.Album = map.Album;
                updated = true;
            }
            if(map.Year != pair.Value.Year.ToString() && map.Year != _("<keep>"))
            {
                try
                {
                    pair.Value.Year = uint.Parse(map.Year);
                    updated = true;
                }
                catch { }
            }
            if(map.Track != pair.Value.Track.ToString() && map.Track != _("<keep>"))
            {
                try
                {
                    pair.Value.Track = uint.Parse(map.Track);
                    updated = true;
                }
                catch { }
            }
            if(map.AlbumArtist != pair.Value.AlbumArtist && map.AlbumArtist != _("<keep>"))
            {
                pair.Value.AlbumArtist = map.AlbumArtist;
                updated = true;
            }
            if(map.Genre != pair.Value.Genre && map.Genre != _("<keep>"))
            {
                pair.Value.Genre = map.Genre;
                updated = true;
            }
            if(map.Comment != pair.Value.Comment && map.Comment != _("<keep>"))
            {
                pair.Value.Comment = map.Comment;
                updated = true;
            }
            if(map.BPM != pair.Value.BPM.ToString() && map.BPM != _("<keep>"))
            {
                try
                {
                    pair.Value.BPM = uint.Parse(map.BPM);
                    updated = true;
                }
                catch { }
            }
            if(map.Composer != pair.Value.Composer && map.Composer != _("<keep>"))
            {
                pair.Value.Composer = map.Composer;
                updated = true;
            }
            if(map.Description != pair.Value.Description && map.Description != _("<keep>"))
            {
                pair.Value.Description = map.Description;
                updated = true;
            }
            if(map.Publisher != pair.Value.Publisher && map.Publisher != _("<keep>"))
            {
                pair.Value.Publisher = map.Publisher;
                updated = true;
            }
            if(map.ISRC != pair.Value.ISRC && map.ISRC != _("<keep>"))
            {
                pair.Value.ISRC = map.ISRC;
                updated = true;
            }
            if(SelectedMusicFiles.Count == 1)
            {
                foreach(var p in map.CustomProperties)
                {
                    if(p.Value != pair.Value.GetCustomProperty(p.Key) && p.Value != _("<keep>"))
                    {
                        pair.Value.SetCustomProperty(p.Key, p.Value);
                        updated = true;
                    }
                }
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
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Converted {0} file name to tag successfully", "Converted {0} file names to tags successfully", success, success), NotificationSeverity.Success));
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
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_n("Converted {0} tag to file name successfully", "Converted {0} tags to file names successfully", success, success), NotificationSeverity.Success));

        }
    }

    /// <summary>
    /// Inserts the image to the selected files' album art
    /// </summary>
    /// <param name="path">The path to the image</param>
    /// <param name="type">AlbumArtType</param>
    public void InsertSelectedAlbumArt(string path, AlbumArtType type)
    {
        TagLib.ByteVector? byteVector = null;
        try
        {
            byteVector = TagLib.ByteVector.FromPath(path);
        }
        catch
        {
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Unable to load image file"), NotificationSeverity.Error));
            return;
        }
        var inserted = false;
        foreach(var pair in SelectedMusicFiles)
        {
            if(type == AlbumArtType.Front)
            {
                if(pair.Value.FrontAlbumArt != byteVector!)
                {
                    pair.Value.FrontAlbumArt = byteVector;
                    MusicFileSaveStates[pair.Key] = false;
                    inserted = true;
                }
            }
            else if(type == AlbumArtType.Back)
            {
                if(pair.Value.BackAlbumArt != byteVector)
                {
                    pair.Value.BackAlbumArt = byteVector;
                    MusicFileSaveStates[pair.Key] = false;
                    inserted = true;
                }
            }
        }
        if(inserted)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes the selected files' album art
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    public void RemoveSelectedAlbumArt(AlbumArtType type)
    {
        var removed = false;
        foreach(var pair in SelectedMusicFiles)
        {
            if(type == AlbumArtType.Front)
            {
                if(!pair.Value.FrontAlbumArt.IsEmpty)
                {
                    pair.Value.FrontAlbumArt = new TagLib.ByteVector();
                    MusicFileSaveStates[pair.Key] = false;
                    removed = true;
                }
            }
            else if(type == AlbumArtType.Back)
            {
                if(!pair.Value.BackAlbumArt.IsEmpty)
                {
                    pair.Value.BackAlbumArt = new TagLib.ByteVector();
                    MusicFileSaveStates[pair.Key] = false;
                    removed = true;
                }
            }
        }
        if(removed)
        {
            UpdateSelectedMusicFilesProperties();
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Exports the selected album art of first selected music file
    /// </summary>
    /// <param name="path">The path for new image file</param>
    /// <param name="type">AlbumArtType</param>
    public void ExportSelectedAlbumArt(string path, AlbumArtType type)
    {
        var musicFile = SelectedMusicFiles.First().Value;
        if(type == AlbumArtType.Front)
        {
            if(!musicFile.FrontAlbumArt.IsEmpty)
            {
                try
                {
                    System.IO.File.WriteAllBytes(path, musicFile.FrontAlbumArt.Data);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Exported front album art to file successfully"), NotificationSeverity.Success));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Failed to export front album art to a file"), NotificationSeverity.Error));
                }
            }
        }
        else if(type == AlbumArtType.Back)
        {
            if(!musicFile.BackAlbumArt.IsEmpty)
            {
                try
                {
                    System.IO.File.WriteAllBytes(path, musicFile.BackAlbumArt.Data);
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
    /// Adds a custom property to the selected music files
    /// </summary>
    /// <param name="name">The name of the property to add</param>
    public void AddCustomProperty(string name)
    {
        var added = false;
        foreach(var pair in SelectedMusicFiles)
        {
            pair.Value.SetCustomProperty(name, "");
            MusicFileSaveStates[pair.Key] = false;
            added = true;
        }
        if(added)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes the custom property from selected music files
    /// </summary>
    /// <param name="The name of the property to remove"></param>
    public void RemoveCustomProperty(string name)
    {
        var removed = false;
        foreach(var pair in SelectedMusicFiles)
        {
            if(pair.Value.RemoveCustomProperty(name))
            {
                MusicFileSaveStates[pair.Key] = false;
                removed = true;
            }
        }
        if(removed)
        {
            UpdateSelectedMusicFilesProperties();
        }
        MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets MimeType of album art for the first selected file
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    public string GetFirstAlbumArtMimeType(AlbumArtType type) => new Picture(type == AlbumArtType.Front ? SelectedMusicFiles.First().Value.FrontAlbumArt : SelectedMusicFiles.First().Value.BackAlbumArt).MimeType;

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
    /// Submits tag information to AcoustId for the selected file
    /// </summary>
    /// <param name="recordingID">The MusicBrainz Recording Id to associate, if available</param>
    public async Task SubmitToAcoustIdAsync(string? recordingID)
    {
        if(SelectedMusicFiles.Count == 1)
        {
            var result = await SelectedMusicFiles[0].SubmitToAcoustIdAsync("b'Ch3cuJ0d", Configuration.Current.AcoustIdUserAPIKey, recordingID);
            MusicFileSaveStatesChanged?.Invoke(this, EventArgs.Empty);
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(result ? _("Submitted metadata to AcoustId successfully") : _("Unable to submit to AcoustId. Check API key"), result ? NotificationSeverity.Success : NotificationSeverity.Error));
        }
    }

    /// <summary>
    /// Performs an advanced search
    /// </summary>
    /// <param name="s">The search string in the format: !prop1="value1";prop2="value2"</param>
    /// <returns>A bool based on whether or not the search was successful and a list of lowercase filenames matching the search</returns>
    public (bool Success, List<string>? LowerFilenames) AdvancedSearch(string s)
    {
        if(_musicFolder != null)
        {
            if(string.IsNullOrEmpty(s) || s[0] != '!')
            {
                return (false, null);
            }
            var search = s.Substring(1);
            if(string.IsNullOrEmpty(search))
            {
                return (false, null);
            }
            var propValPairs = search.Split(';');
            var validProperties = new string[] { "filename", _("filename"), "title", _("title"), "artist", _("artist"), "album", _("album"), "year", _("year"), "track", _("track"), "albumartist", _("albumartist"), "genre", _("genre"), "comment", _("comment"), "bpm", _("bpm"), "composer", _("composer"), "description", _("description"), "publisher", _("publisher"), "isrc", _("isrc"), "custom", _("custom") };
            var propertyMap = new PropertyMap();
            var customPropName = "";
            foreach(var propVal in propValPairs)
            {
                var fields = propVal.Split('=');
                if(fields.Length != 2)
                {
                    return (false, null);
                }
                var prop = fields[0].ToLower();
                var val = fields[1];
                if(!validProperties.Contains(prop))
                {
                    return (false, null);
                }
                if(val.Length <= 1 || val.Substring(0, 1) != "\"" || val.Substring(val.Length - 1) != "\"")
                {
                    return (false, null);
                }
                val = val.Remove(0, 1);
                val = val.Remove(val.Length - 1, 1);
                if(string.IsNullOrEmpty(val))
                {
                    val = "NULL";
                }
                if(prop == "filename" || prop == _("filename"))
                {
                    propertyMap.Filename = val;
                }
                else if(prop == "title" || prop == _("title"))
                {
                    propertyMap.Title = val;
                }
                else if(prop == "artist" || prop == _("artist"))
                {
                    propertyMap.Artist = val;
                }
                else if(prop == "album" || prop == _("album"))
                {
                    propertyMap.Album = val;
                }
                else if(prop == "year" || prop == _("year"))
                {
                    if(val != "NULL")
                    {
                        try
                        {
                            uint.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.Year = val;
                }
                else if(prop == "track" || prop == _("track"))
                {
                    if(val != "NULL")
                    {
                        try
                        {
                            uint.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.Track = val;
                }
                else if(prop == "albumartist" || prop == _("albumartist"))
                {
                    propertyMap.AlbumArtist = val;
                }
                else if(prop == "genre" || prop == _("genre"))
                {
                    propertyMap.Genre = val;
                }
                else if(prop == "comment" || prop == _("comment"))
                {
                    propertyMap.Comment = val;
                }
                else if(prop == "bpm" || prop == _("bpm"))
                {
                    if(val != "NULL")
                    {
                        try
                        {
                            uint.Parse(val);
                        }
                        catch
                        {
                            return (false, null);
                        }
                    }
                    propertyMap.BPM = val;
                }
                else if(prop == "composer" || prop == _("composer"))
                {
                    propertyMap.Composer = val;
                }
                else if(prop == "description" || prop == _("description"))
                {
                    propertyMap.Description = val;
                }
                else if(prop == "publisher" || prop == _("publisher"))
                {
                    propertyMap.Publisher = val;
                }
                else if(prop == "isrc" || prop == _("isrc"))
                {
                    propertyMap.ISRC = val;
                }
                else if(prop == "custom" || prop == _("custom"))
                {
                    customPropName = val;
                }
            }
            var matches = new List<string>();
            foreach(var musicFile in _musicFolder.MusicFiles)
            {
                if(!string.IsNullOrEmpty(propertyMap.Filename))
                {
                    var value = musicFile.Filename.ToLower();
                    if(propertyMap.Filename == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Filename)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Title))
                {
                    var value = musicFile.Title.ToLower();
                    if(propertyMap.Title == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Title)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Artist))
                {
                    var value = musicFile.Artist.ToLower();
                    if(propertyMap.Artist == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Artist)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Album))
                {
                    var value = musicFile.Album.ToLower();
                    if(propertyMap.Album == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Album)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Year))
                {
                    var value = musicFile.Year;
                    if(propertyMap.Year == "NULL")
                    {
                        if(value != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != uint.Parse(propertyMap.Year))
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Track))
                {
                    var value = musicFile.Track;
                    if(propertyMap.Track == "NULL")
                    {
                        if(value != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != uint.Parse(propertyMap.Track))
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.AlbumArtist))
                {
                    var value = musicFile.AlbumArtist.ToLower();
                    if(propertyMap.AlbumArtist == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.AlbumArtist)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Genre))
                {
                    var value = musicFile.Genre.ToLower();
                    if(propertyMap.Genre == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Genre)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Comment))
                {
                    var value = musicFile.Comment.ToLower();
                    if(propertyMap.Comment == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Comment)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.BPM))
                {
                    var value = musicFile.BPM;
                    if(propertyMap.BPM == "NULL")
                    {
                        if(value != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != uint.Parse(propertyMap.BPM))
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Composer))
                {
                    var value = musicFile.Composer.ToLower();
                    if(propertyMap.Composer == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Composer)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Description))
                {
                    var value = musicFile.Description.ToLower();
                    if(propertyMap.Description == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Description)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.Publisher))
                {
                    var value = musicFile.Publisher.ToLower();
                    if(propertyMap.Publisher == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.Publisher)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(propertyMap.ISRC))
                {
                    var value = musicFile.ISRC.ToLower();
                    if(propertyMap.ISRC == "NULL")
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(value != propertyMap.ISRC)
                        {
                            continue;
                        }
                    }
                }
                if(!string.IsNullOrEmpty(customPropName))
                {
                    if(customPropName == "NULL")
                    {
                        continue;
                    }
                    if(!musicFile.CustomPropertyNames.Select(x => x.ToLower()).Contains(customPropName))
                    {
                        continue;
                    }
                }
                matches.Add(musicFile.Filename.ToLower());
            }
            return (true, matches);
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
            SelectedPropertyMap.BPM = first.BPM.ToString();
            SelectedPropertyMap.Composer = first.Composer;
            SelectedPropertyMap.Description = first.Description;
            SelectedPropertyMap.Publisher = first.Publisher;
            SelectedPropertyMap.ISRC = first.ISRC;
            SelectedPropertyMap.Duration = first.Duration.ToDurationString();
            SelectedPropertyMap.Fingerprint = "";
            Task.Run(() =>
            {
                var fingerprint = first.Fingerprint;
                if(first == SelectedMusicFiles.First().Value)
                {
                    SelectedPropertyMap.Fingerprint = fingerprint;
                    FingerprintCalculated?.Invoke(this, EventArgs.Empty);
                }
            });
            SelectedPropertyMap.FileSize = first.FileSize.ToFileSizeString();
            SelectedPropertyMap.FrontAlbumArt = first.FrontAlbumArt.IsEmpty ? "noArt" : "hasArt";
            SelectedPropertyMap.BackAlbumArt = first.BackAlbumArt.IsEmpty ? "noArt" : "hasArt";
            foreach (var custom in first.CustomPropertyNames)
            {
                SelectedPropertyMap.CustomProperties.Add(custom, first.GetCustomProperty(custom)!);
            }
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
            var haveSameBPM = true;
            var haveSameComposer = true;
            var haveSameDescription = true;
            var haveSamePublisher = true;
            var haveSameISRC = true;
            var haveSameFrontAlbumArt = true;
            var haveSameBackAlbumArt = true;
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
                if(first.BPM != pair.Value.BPM)
                {
                    haveSameBPM = false;
                }
                if(first.Composer != pair.Value.Composer)
                {
                    haveSameComposer = false;
                }
                if(first.Description != pair.Value.Description)
                {
                    haveSameDescription = false;
                }
                if(first.Publisher != pair.Value.Publisher)
                {
                    haveSamePublisher = false;
                }
                if(first.ISRC != pair.Value.ISRC)
                {
                    haveSameISRC = false;
                }
                if(first.FrontAlbumArt != pair.Value.FrontAlbumArt)
                {
                    haveSameFrontAlbumArt = false;
                }
                if(first.BackAlbumArt != pair.Value.BackAlbumArt)
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
            SelectedPropertyMap.Year = haveSameYear ? first.Year.ToString() : _("<keep>");
            SelectedPropertyMap.Track = haveSameTrack ? first.Track.ToString() : _("<keep>");
            SelectedPropertyMap.AlbumArtist = haveSameAlbumArtist ? first.AlbumArtist : _("<keep>");
            SelectedPropertyMap.Genre = haveSameGenre ? first.Genre : _("<keep>");
            SelectedPropertyMap.Comment = haveSameComment ? first.Comment : _("<keep>");
            SelectedPropertyMap.BPM = haveSameBPM ? first.BPM.ToString() : _("<keep>");
            SelectedPropertyMap.Composer = haveSameComposer ? first.Composer : _("<keep>");
            SelectedPropertyMap.Description = haveSameDescription ? first.Description : _("<keep>");
            SelectedPropertyMap.Publisher = haveSamePublisher ? first.Publisher : _("<keep>");
            SelectedPropertyMap.ISRC = haveSameISRC ? first.ISRC : _("<keep>");
            SelectedPropertyMap.FrontAlbumArt = haveSameFrontAlbumArt ? (first.FrontAlbumArt.IsEmpty ? "noArt" : "hasArt") : "keepArt";
            SelectedPropertyMap.BackAlbumArt = haveSameBackAlbumArt ? (first.BackAlbumArt.IsEmpty ? "noArt" : "hasArt") : "keepArt";
            SelectedPropertyMap.Duration = totalDuration.ToDurationString();
            SelectedPropertyMap.Fingerprint = _("<keep>");
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
        if(_musicFolder != null)
        {
            var includeSubfoldersChanged = _musicFolder.IncludeSubfolders != Configuration.Current.IncludeSubfolders;
            var sortingChanged = _musicFolder.SortFilesBy != Configuration.Current.SortFilesBy;
            if(includeSubfoldersChanged || sortingChanged)
            {
                LoadingStateUpdated?.Invoke(this, _("Loading music files from folder..."));
                _musicFolder.IncludeSubfolders = Configuration.Current.IncludeSubfolders;
                _musicFolder.SortFilesBy = Configuration.Current.SortFilesBy;
                await ReloadFolderAsync();
            }
        }
    }
}
