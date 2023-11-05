using ATL;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Nickvision.Aura;
using NickvisionTagger.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Statuses for MusicBrainz lookup
/// </summary>
public enum MusicBrainzLoadStatus
{
    Success = 0,
    NoAcoustIdResult,
    NoAcoustIdRecordingId,
    InvalidMusicBrainzRecordingId,
    InvalidFingerprint
}

/// <summary>
/// A model of a music file
/// </summary>
public class MusicFile : IComparable<MusicFile>, IDisposable, IEquatable<MusicFile>
{
    private static string[] _validProperties;
    private static List<char> _invalidWindowsFilenameCharacters;
    private static List<char> _invalidSystemFilenameCharacters;

    private bool _disposed;
    private Track _track;
    private string _dotExtension;
    private string _filename;
    private DateTime _modificationTimestamp;
    private Process? _fpcalc;
    private string _fingerprint;
    private AlbumArt _frontAlbumArt;
    private AlbumArt _backAlbumArt;

    /// <summary>
    /// What to sort files in a music folder by
    /// </summary>
    public static SortBy SortFilesBy { get; set; }
    /// <summary>
    /// Whether or not filename characters should be limited to those only supported by Windows
    /// </summary>
    public static bool LimitFilenameCharacters { get; set; }

    /// <summary>
    /// The path of the music file
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// The duration of the music file
    /// </summary>
    public int Duration => _track.Duration;
    /// <summary>
    /// The size of the music file
    /// </summary>
    public long FileSize => new FileInfo(Path).Length;
    /// <summary>
    /// Whether or not the tag is empty
    /// </summary>
    public bool IsTagEmpty => Title == "" && Artist == "" && Album == "" && Year == 0 && Track == 0 && TrackTotal == 0 && AlbumArtist == "" && Genre == "" && Comment == "" && BeatsPerMinute == 0 && Composer == "" && Description == "" && DiscNumber == 0 && DiscTotal == 0 && Publisher == "" && PublishingDate == DateTime.MinValue && FrontAlbumArt.IsEmpty && BackAlbumArt.IsEmpty && _track.AdditionalFields.Count == 0 && string.IsNullOrEmpty(Lyrics.Description) && string.IsNullOrEmpty(Lyrics.UnsynchronizedLyrics) && Lyrics.SynchronizedLyrics.Count == 0;

    /// <summary>
    /// Constructs a static MusicFile
    /// </summary>
    static MusicFile()
    {
        _validProperties = new string[] { "title", _("title"), "artist", _("artist"), "album", _("album"), "year", _("year"), "track", _("track"), "tracktotal", _("tracktotal"), "albumartist", _("albumartist"), "genre", _("genre"), "comment", _("comment"), "beatsperminute", _("beatsperminute"), "bpm", _("bpm"), "composer", _("composer"), "description", _("description"), "discnumber", _("discnumber"), "disctotal", _("disctotal"), "publisher", _("publisher"), "publishingdate", _("publishingdate"), "lyrics", _("lyrics") };
        _invalidWindowsFilenameCharacters = new List<char>() { '"', '<', '>', ':', '\\', '/', '|', '?', '*' };
        _invalidSystemFilenameCharacters = System.IO.Path.GetInvalidPathChars().Union(System.IO.Path.GetInvalidFileNameChars()).ToList();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _invalidSystemFilenameCharacters.Remove('\\');
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _invalidSystemFilenameCharacters.Remove('/');
        }
        SortFilesBy = SortBy.Path;
        LimitFilenameCharacters = false;
        ATL.Settings.UseFileNameWhenNoTitle = false;
        ATL.Settings.FileBufferSize = 1024;
        ATL.Settings.ID3v2_writePictureDataLengthIndicator = false;
    }

    /// <summary>
    /// Constructs a MusicFile
    /// </summary>
    /// <param name="path">The path of the music file</param>
    public MusicFile(string path)
    {
        _disposed = false;
        Path = path;
        try
        {
            _track = new Track(Path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message + "\n" + e.StackTrace);
            throw new CorruptedMusicFileException(CorruptionType.InvalidTagData);
        }
        _dotExtension = System.IO.Path.GetExtension(Path).ToLower();
        _filename = System.IO.Path.GetFileName(Path);
        _modificationTimestamp = File.GetLastWriteTime(Path);
        _fpcalc = null;
        _fingerprint = "";
        //Load Front Album Art
        PictureInfo? front = null;
        foreach (var picture in _track.EmbeddedPictures)
        {
            if (picture.PicType == PictureInfo.PIC_TYPE.Front)
            {
                front = picture;
                break;
            }
            if (picture.PicType == PictureInfo.PIC_TYPE.Generic)
            {
                front = picture;
            }
        }
        _frontAlbumArt = front != null ? new AlbumArt(front) : new AlbumArt(Array.Empty<byte>(), AlbumArtType.Front);
        //Load Back Album Art
        var back = _track.EmbeddedPictures.FirstOrDefault(x => x.PicType == PictureInfo.PIC_TYPE.Back);
        _backAlbumArt = back != null ? new AlbumArt(back) : new AlbumArt(Array.Empty<byte>(), AlbumArtType.Back);
    }

    /// <summary>
    /// Finalizes the MusicFile
    /// </summary>
    ~MusicFile() => Dispose(false);

    /// <summary>
    /// The filename of the music file
    /// </summary>
    public string Filename
    {
        get => _filename;

        set
        {
            var newFilename = value;
            if (System.IO.Path.GetExtension(newFilename).ToLower() != _dotExtension)
            {
                newFilename += _dotExtension;
            }
            IEnumerable<char> invalidCharacters = _invalidSystemFilenameCharacters;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && LimitFilenameCharacters)
            {
                invalidCharacters = _invalidSystemFilenameCharacters.Union(_invalidWindowsFilenameCharacters);
            }
            foreach (var invalidChar in invalidCharacters)
            {
                if (newFilename.Contains(invalidChar))
                {
                    newFilename = newFilename.Replace(invalidChar, '_');
                }
            }
            if (File.Exists(newFilename))
            {
                return;
            }
            _filename = newFilename;
        }
    }

    /// <summary>
    /// The title of the music file
    /// </summary>
    public string Title
    {
        get => _track.Title ?? "";

        set => _track.Title = value;
    }

    /// <summary>
    /// The artist of the music file
    /// </summary>
    public string Artist
    {
        get => _track.Artist ?? "";

        set => _track.Artist = value;
    }

    /// <summary>
    /// The album of the music file
    /// </summary>
    public string Album
    {
        get => _track.Album ?? "";

        set => _track.Album = value;
    }

    /// <summary>
    /// The year of the music file
    /// </summary>
    public int Year
    {
        get => _track.Year ?? 0;

        set => _track.Year = value;
    }

    /// <summary>
    /// The track of the music file
    /// </summary>
    public int Track
    {
        get => _track.TrackNumber ?? 0;

        set => _track.TrackNumber = value;
    }

    /// <summary>
    /// The number of total tracks of the music file
    /// </summary>
    public int TrackTotal
    {
        get => _track.TrackTotal ?? 0;

        set => _track.TrackTotal = value;
    }

    /// <summary>
    /// The album artist of the music file
    /// </summary>
    public string AlbumArtist
    {
        get => _track.AlbumArtist ?? "";

        set => _track.AlbumArtist = value;
    }

    /// <summary>
    /// The genre of the music file
    /// </summary>
    public string Genre
    {
        get => _track.Genre ?? "";

        set => _track.Genre = value;
    }

    /// <summary>
    /// The comment of the music file
    /// </summary>
    public string Comment
    {
        get => _track.Comment ?? "";

        set => _track.Comment = value;
    }

    /// <summary>
    /// The BPM of the music file
    /// </summary>
    public int BeatsPerMinute
    {
        get => _track.BPM ?? 0;

        set => _track.BPM = value;
    }

    /// <summary>
    /// The composer of the music file
    /// </summary>
    public string Composer
    {
        get => _track.Composer ?? "";

        set => _track.Composer = value;
    }

    /// <summary>
    /// The description of the music file
    /// </summary>
    public string Description
    {
        get => _track.Description ?? "";

        set => _track.Description = value;
    }

    /// <summary>
    /// The disc number of the music file
    /// </summary>
    public int DiscNumber
    {
        get => _track.DiscNumber ?? 0;

        set => _track.DiscNumber = value;
    }

    /// <summary>
    /// The disc total of the music file
    /// </summary>
    public int DiscTotal
    {
        get => _track.DiscTotal ?? 0;

        set => _track.DiscTotal = value;
    }

    /// <summary>
    /// The publisher of the music file
    /// </summary>
    public string Publisher
    {
        get => _track.Publisher ?? "";

        set => _track.Publisher = value;
    }

    /// <summary>
    /// The publishing date of the music file
    /// </summary>
    /// <remarks>DateTime.MinValue signifies an empty date</remarks>
    public DateTime PublishingDate
    {
        get => _track.PublishingDate ?? DateTime.MinValue;

        set => _track.PublishingDate = value;
    }

    /// <summary>
    /// The front album art of the music file
    /// </summary>
    public AlbumArt FrontAlbumArt
    {
        get => _frontAlbumArt;

        set
        {
            var backAlbumArt = BackAlbumArt;
            _track.EmbeddedPictures.Clear();
            if (!value.IsEmpty)
            {
                _track.EmbeddedPictures.Add(value.ATLPictureInfo);
            }
            if (!backAlbumArt.IsEmpty)
            {
                _track.EmbeddedPictures.Add(backAlbumArt.ATLPictureInfo);
            }
            _frontAlbumArt = value;
        }
    }

    /// <summary>
    /// The back album art of the music file
    /// </summary>
    public AlbumArt BackAlbumArt
    {
        get => _backAlbumArt;

        set
        {
            var frontAlbumArt = FrontAlbumArt;
            _track.EmbeddedPictures.Clear();
            if (!value.IsEmpty)
            {
                _track.EmbeddedPictures.Add(value.ATLPictureInfo);
            }
            if (frontAlbumArt.Image.Length > 0)
            {
                _track.EmbeddedPictures.Add(frontAlbumArt.ATLPictureInfo);
            }
            _backAlbumArt = value;
        }
    }

    /// <summary>
    /// The LyricsInfo object of the music file
    /// </summary>
    public LyricsInfo Lyrics
    {
        get
        {
            if (!_track.Lyrics.Metadata.ContainsKey("offset"))
            {
                _track.Lyrics.Metadata["offset"] = "0";
            }
            return _track.Lyrics;
        }

        set
        {
            _track.Lyrics = value;
            if (!_track.Lyrics.Metadata.ContainsKey("offset"))
            {
                _track.Lyrics.Metadata["offset"] = "0";
            }
        }
    }

    /// <summary>
    /// The fingerprint of the music file
    /// </summary>
    public string Fingerprint
    {
        get
        {
            if (_fpcalc == null && string.IsNullOrEmpty(_fingerprint))
            {
                _fpcalc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = DependencyLocator.Find("fpcalc"),
                        Arguments = $"\"{Path}\" -length 120",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                _fpcalc.Start();
                _fpcalc.WaitForExit(TimeSpan.FromSeconds(8));
                _fingerprint = _fpcalc.StandardOutput.ReadToEnd();
                try
                {
                    _fpcalc.Kill(true);
                }
                catch { }
                if (_fpcalc.ExitCode == 0)
                {
                    _fingerprint = _fingerprint.Substring(_fingerprint.IndexOf("FINGERPRINT=") + 12);
                    _fingerprint = _fingerprint.Remove(_fingerprint.Length - 1);
                }
                else
                {
                    _fingerprint = _("ERROR");
                }
                _fpcalc.Dispose();
                _fpcalc = null;
            }
            return _fingerprint;
        }
    }

    /// <summary>
    /// The sorted list of the names of the custom properties
    /// </summary>
    public List<string> CustomPropertyNames
    {
        get
        {
            var names = _track.AdditionalFields.Keys.ToList();
            names.Sort();
            return names;
        }
    }

    /// <summary>
    /// The PropertyMap of the MusicFile
    /// </summary>
    public PropertyMap PropertyMap
    {
        get
        {
            return new PropertyMap()
            {
                Filename = Filename,
                Title = Title,
                Artist = Artist,
                Album = Album,
                Year = Year == 0 ? "" : Year.ToString(),
                Track = Track == 0 ? "" : Track.ToString(),
                TrackTotal = TrackTotal == 0 ? "" : TrackTotal.ToString(),
                AlbumArtist = AlbumArtist,
                Genre = Genre,
                Comment = Comment,
                BeatsPerMinute = BeatsPerMinute == 0 ? "" : BeatsPerMinute.ToString(),
                Composer = Composer,
                Description = Description,
                DiscNumber = DiscNumber == 0 ? "" : DiscNumber.ToString(),
                DiscTotal = DiscTotal == 0 ? "" : DiscTotal.ToString(),
                Publisher = Publisher,
                PublishingDate = PublishingDate == DateTime.MinValue ? "" : PublishingDate.ToShortDateString(),
                FrontAlbumArt = FrontAlbumArt.ToString(),
                BackAlbumArt = BackAlbumArt.ToString(),
                CustomProperties = _track.AdditionalFields.ToDictionary(x => x.Key, x => x.Value),
                Duration = Duration.ToDurationString(),
                Fingerprint = _fingerprint,
                FileSize = FileSize.ToFileSizeString()
            };
        }
    }

    /// <summary>
    /// Frees resources used by the MusicFile object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MusicFile object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        try
        {
            _fpcalc?.Kill(true);
        }
        catch { }
        _fpcalc?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Saves the tag of the music file to disk
    /// </summary>
    /// <param name="preserveModificationTimestamp">Whether or not to preserve (not change) a file's modification timestamp</param>
    /// <returns>True if successful, else false</returns>
    public bool SaveTagToDisk(bool preserveModificationTimestamp)
    {
        var res = _track.Save();
        if (res)
        {
            if (System.IO.Path.GetFileName(Path) != Filename)
            {
                var newPath = $"{System.IO.Path.GetDirectoryName(Path)}{System.IO.Path.DirectorySeparatorChar}{Filename}";
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(newPath)!);
                var i = 1;
                while (File.Exists(newPath))
                {
                    var oldNumber = $" ({i - 1})";
                    newPath = newPath.Remove(newPath.IndexOf(oldNumber), oldNumber.Length) + $" ({i})";
                }
                File.Move(Path, newPath);
                Path = newPath;
                _track = new Track(Path);
            }
            if (preserveModificationTimestamp)
            {
                File.SetLastWriteTime(Path, _modificationTimestamp);
            }
            else
            {
                _modificationTimestamp = File.GetLastWriteTime(Path);
            }
        }
        return res;
    }

    /// <summary>
    /// Resets the file's tag back to what is stored on disk
    /// </summary>
    public void ResetTag()
    {
        _track = new Track(Path);
        _filename = System.IO.Path.GetFileName(Path);
    }

    /// <summary>
    /// Clears a file's tag (does not save to disk)
    /// </summary>
    public void ClearTag()
    {
        _filename = System.IO.Path.GetFileName(Path);
        Title = "";
        Artist = "";
        Album = "";
        Year = 0;
        Track = 0;
        TrackTotal = 0;
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        BeatsPerMinute = 0;
        Composer = "";
        Description = "";
        DiscNumber = 0;
        DiscTotal = 0;
        Publisher = "";
        PublishingDate = DateTime.MinValue;
        _track.EmbeddedPictures.Clear();
        _track.AdditionalFields.Clear();
        Lyrics = new LyricsInfo();
    }

    /// <summary>
    /// Gets a custom property value
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <returns>The value of the custom property. Null if no custom property exists with the provided name</returns>
    public string? GetCustomProperty(string name)
    {
        if (!_track.AdditionalFields.ContainsKey(name))
        {
            return null;
        }
        return _track.AdditionalFields[name];
    }

    /// <summary>
    /// Sets a custom property value
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <param name="value">The value of the custom property</param>
    /// <returns>True if set, else false</returns>
    public bool SetCustomProperty(string name, string value)
    {
        if (_validProperties.Contains(name.ToLower()) || _validProperties.Contains(name.Replace(" ", "").ToLower()))
        {
            return false;
        }
        _track.AdditionalFields[name] = value;
        return true;
    }

    /// <summary>
    /// Removes a custom property
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <returns>Whether or not the property was removed. If no property exists with the provided name, false will be returned</returns>
    public bool RemoveCustomProperty(string name)
    {
        if (!_track.AdditionalFields.ContainsKey(name))
        {
            return false;
        }
        _track.AdditionalFields.Remove(name);
        return true;
    }

    /// <summary>
    /// Uses the music file's filename to fill in tag information based on the format string
    /// </summary>
    /// <param name="formatString">Th format string</param>
    /// <returns>True if successful, else false</returns>
    public bool FilenameToTag(string formatString)
    {
        if (string.IsNullOrEmpty(formatString))
        {
            return false;
        }
        var matches = Regex.Matches(formatString, @"%(\w*)%", RegexOptions.IgnoreCase); //wrapped in %%
        if (matches.Count == 0)
        {
            return false;
        }
        var splits = Regex.Split(formatString, @"(\%\w*\%)", RegexOptions.IgnoreCase).Where(x =>
        {
            if (x.Length > 1)
            {
                return x[0] != '%' && x[x.Length - 1] != '%';
            }
            return x.Length != 0;
        }).ToList();
        foreach (var s in splits)
        {
            if (!Filename.Contains(s))
            {
                return false;
            }
        }
        var filename = System.IO.Path.GetFileNameWithoutExtension(Filename);
        var i = 0;
        foreach (Match match in matches)
        {
            string value = match.Value.Remove(0, 1); //remove first %
            value = value.Remove(value.Length - 1, 1).ToLower(); //remove last %;
            var len = 0;
            try
            {
                len = filename.IndexOf(splits[i]);
            }
            catch
            {
                len = filename.Length;
            }
            if (value == "title" || value == _("title"))
            {
                Title = filename.Substring(0, len);
            }
            else if (value == "artist" || value == _("artist"))
            {
                Artist = filename.Substring(0, len);
            }
            else if (value == "album" || value == _("album"))
            {
                Album = filename.Substring(0, len);
            }
            else if (value == "year" || value == _("year"))
            {
                try
                {
                    Year = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "track" || value == _("track"))
            {
                try
                {
                    Track = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "tracktotal" || value == _("tracktotal"))
            {
                try
                {
                    TrackTotal = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "albumartist" || value == _("albumartist"))
            {
                AlbumArtist = filename.Substring(0, len);
            }
            else if (value == "genre" || value == _("genre"))
            {
                Genre = filename.Substring(0, len);
            }
            else if (value == "comment" || value == _("comment"))
            {
                Comment = filename.Substring(0, len);
            }
            else if (value == "beatsperminute" || value == _("beatsperminute") || value == "bpm" || value == _("bpm"))
            {
                try
                {
                    BeatsPerMinute = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "composer" || value == _("composer"))
            {
                Composer = filename.Substring(0, len);
            }
            else if (value == "description" || value == _("description"))
            {
                Description = filename.Substring(0, len);
            }
            else if (value == "discnumber" || value == _("discnumber"))
            {
                try
                {
                    DiscNumber = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "disctotal" || value == _("disctotal"))
            {
                try
                {
                    DiscTotal = int.Parse(filename.Substring(0, len));
                }
                catch { }
            }
            else if (value == "publisher" || value == _("publisher"))
            {
                Publisher = filename.Substring(0, len);
            }
            else if (value == "publishingdate" || value == _("publishingdate"))
            {
                try
                {
                    PublishingDate = DateTime.Parse(filename.Substring(0, len)).Date;
                }
                catch { }
            }
            else
            {
                SetCustomProperty(value, filename.Substring(0, len));
            }
            filename = filename.Remove(0, len == filename.Length ? len : len + splits[i].Length);
            i++;
        }
        return true;
    }

    /// <summary>
    /// Uses the music file's tag to set the file's filename in the format of the format string
    /// </summary>
    /// <param name="formatString">Th format string</param>
    /// <returns>True if successful, else false</returns>
    public bool TagToFilename(string formatString)
    {
        if (string.IsNullOrEmpty(formatString))
        {
            return false;
        }
        var customProps = _track.AdditionalFields.Keys.ToList();
        var matches = Regex.Matches(formatString, @"%(\w+)%", RegexOptions.IgnoreCase); //wrapped in %%
        if (matches.Count == 0)
        {
            return false;
        }
        foreach (Match match in matches)
        {
            string value = match.Value.Remove(0, 1); //remove first %
            value = value.Remove(value.Length - 1, 1); //remove last %;
            if (_validProperties.Contains(value.ToLower()) && value.ToLower() != "lyrics" && value.ToLower() != _("lyrics"))
            {
                value = value.ToLower();
                var replace = "";
                if (value == "title" || value == _("title"))
                {
                    replace = Title;
                }
                else if (value == "artist" || value == _("artist"))
                {
                    replace = Artist;
                }
                else if (value == "album" || value == _("album"))
                {
                    replace = Album;
                }
                else if (value == "year" || value == _("year"))
                {
                    replace = Year.ToString();
                }
                else if (value == "track" || value == _("track"))
                {
                    replace = Track.ToString("D2");
                }
                else if (value == "tracktotal" || value == _("tracktotal"))
                {
                    replace = TrackTotal.ToString("D2");
                }
                else if (value == "albumartist" || value == _("albumartist"))
                {
                    replace = AlbumArtist;
                }
                else if (value == "genre" || value == _("genre"))
                {
                    replace = Genre;
                }
                else if (value == "comment" || value == _("comment"))
                {
                    replace = Comment;
                }
                else if (value == "beatsperminute" || value == _("beatsperminute") || value == "bpm" || value == _("bpm"))
                {
                    replace = BeatsPerMinute.ToString();
                }
                else if (value == "composer" || value == _("composer"))
                {
                    replace = Composer;
                }
                else if (value == "description" || value == _("description"))
                {
                    replace = Description;
                }
                else if (value == "discnumber" || value == _("discnumber"))
                {
                    replace = DiscNumber.ToString("D2");
                }
                else if (value == "disctotal" || value == _("disctotal"))
                {
                    replace = DiscTotal.ToString("D2");
                }
                else if (value == "publisher" || value == _("publisher"))
                {
                    replace = Publisher;
                }
                else if (value == "publishingdate" || value == _("publishingdate"))
                {
                    replace = PublishingDate == DateTime.MinValue ? "" : PublishingDate.ToShortDateString();
                }
                formatString = formatString.Replace(match.Value, replace);
            }
            else if (customProps.Contains(value))
            {
                formatString = formatString.Replace(match.Value, _track.AdditionalFields[value]);
            }
            else
            {
                formatString = formatString.Replace(match.Value, "");
            }
        }
        Filename = formatString;
        return true;
    }

    /// <summary>
    /// Downloads tag metadata from MusicBrainz (discarding any unapplied metadata)
    /// </summary>
    /// <param name="acoustIdClientKey">The app's AcoustId Key</param>
    /// <param name="version">The version of the app</param>
    /// <param name="overwriteTagWithMusicBrainz">Whether or not to overwrite a tag's existing data with data from MusicBrainz</param>
    /// <param name="overwriteAlbumArtWithMusicBrainz">Whether or not to overwrite a tag's existing album art with album art from MusicBrainz</param>
    /// <returns>MusicBrainzLoadStatus</returns>
    public async Task<MusicBrainzLoadStatus> DownloadFromMusicBrainzAsync(string acoustIdClientKey, string version, bool overwriteTagWithMusicBrainz, bool overwriteAlbumArtWithMusicBrainz)
    {
        if (Fingerprint == _("ERROR"))
        {
            return MusicBrainzLoadStatus.InvalidFingerprint;
        }
        //Use AcoustID to get MBID
        AcoustID.Configuration.ClientKey = acoustIdClientKey;
        var service = new AcoustID.Web.LookupService();
        AcoustID.Web.LookupResponse? response = null;
        try
        {
            response = await service.GetAsync(Fingerprint, Duration, new string[] { "recordingids" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return MusicBrainzLoadStatus.NoAcoustIdResult;
        }
        if (response.Results.Count > 0)
        {
            //AcoustID Results
            var bestResult = response.Results[0];
            foreach (var r in response.Results)
            {
                if (r.Recordings.Count > 0 && (r.Score > bestResult.Score || bestResult.Recordings.Count == 0))
                {
                    bestResult = r;
                }
            }
            //AcoustID Recordings
            if (bestResult.Recordings.Count < 1)
            {
                return MusicBrainzLoadStatus.NoAcoustIdRecordingId;
            }
            var bestRecordingId = bestResult.Recordings[0].Id;
            foreach (var r in bestResult.Recordings)
            {
                if (r.Title != "")
                {
                    bestRecordingId = r.Id;
                    break;
                }
            }
            //MusicBrainz
            using var query = new MetaBrainz.MusicBrainz.Query("Tagger", version, "mailto:nlogozzo225@gmail.com");
            IRecording? recording = null;
            IRelease? album = null;
            try
            {
                var include = MetaBrainz.MusicBrainz.Include.Releases | MetaBrainz.MusicBrainz.Include.ArtistCredits | MetaBrainz.MusicBrainz.Include.Genres;
                recording = await query.LookupRecordingAsync(Guid.Parse(bestRecordingId), include, MetaBrainz.MusicBrainz.ReleaseType.Album, MetaBrainz.MusicBrainz.ReleaseStatus.Official);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return MusicBrainzLoadStatus.InvalidMusicBrainzRecordingId;
            }
            if (recording.Releases != null && recording.Releases.Count > 0)
            {
                album = recording.Releases[0];
            }
            if (overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Title))
            {
                Title = recording.Title ?? "";
            }
            if (overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Artist))
            {
                if (recording.ArtistCredit != null && recording.ArtistCredit.Count > 0)
                {
                    Artist = recording.ArtistCredit[0].Artist!.Name ?? recording.ArtistCredit[0].Artist!.SortName ?? "";
                }
            }
            if (overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Album))
            {
                if (album != null)
                {
                    Album = album.Title ?? "";
                }
            }
            if (overwriteTagWithMusicBrainz || Year == 0)
            {
                if (recording.FirstReleaseDate != null)
                {
                    Year = recording.FirstReleaseDate.Year ?? 0;
                }
            }
            if (overwriteTagWithMusicBrainz || string.IsNullOrEmpty(AlbumArtist))
            {
                if (album != null && album.ArtistCredit != null && album.ArtistCredit.Count > 0)
                {
                    AlbumArtist = album.ArtistCredit[0].Artist!.Name ?? album.ArtistCredit[0].Artist!.SortName ?? "";
                }
            }
            if (overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Genre))
            {
                if (recording.Genres != null && recording.Genres.Count > 0)
                {
                    Genre = recording.Genres[0].Name ?? "";
                }
            }
            if (overwriteTagWithMusicBrainz || PublishingDate == DateTime.MinValue)
            {
                if (recording.FirstReleaseDate != null)
                {
                    PublishingDate = recording.FirstReleaseDate.NearestDate;
                }
            }
            if (overwriteAlbumArtWithMusicBrainz || FrontAlbumArt.IsEmpty)
            {
                if (album != null && album.CoverArtArchive != null && album.CoverArtArchive.Count > 0)
                {
                    using var caQuery = new CoverArt(version, version, "mailto:nlogozzo225@gmail.com");
                    CoverArtImage? img = null;
                    try
                    {
                        img = await caQuery.FetchFrontAsync(Guid.Parse(bestRecordingId));
                    }
                    catch { }
                    if (img != null)
                    {
                        var reader = new BinaryReader(img.Data);
                        FrontAlbumArt = new AlbumArt(reader.ReadBytes((int)img.Data.Length), AlbumArtType.Front);
                        reader.Dispose();
                        img.Dispose();
                    }
                }
            }
            return MusicBrainzLoadStatus.Success;
        }
        return MusicBrainzLoadStatus.NoAcoustIdResult;
    }

    /// <summary>
    /// Downloads lyrics for the music file
    /// </summary>
    /// <param name="overwrite">Whether or not to overwrite a tag's existing lyric data with data from LyricService</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> DownloadLyricsAsync(bool overwrite)
    {
        var lyrics = await LyricService.GetAsync(Title, Artist);
        if (lyrics == null)
        {
            return false;
        }
        if (overwrite || string.IsNullOrEmpty(Lyrics.UnsynchronizedLyrics))
        {
            Lyrics.UnsynchronizedLyrics = lyrics.UnsynchronizedLyrics;
        }
        if (overwrite || Lyrics.SynchronizedLyrics.Count == 0)
        {
            Lyrics.SynchronizedLyrics = lyrics.SynchronizedLyrics.Select(x => new LyricsInfo.LyricsPhrase(x)).ToList();
        }
        return true;
    }

    /// <summary>
    /// Loads tag metadata from MusicBrainz (discarding any unapplied metadata)
    /// </summary>
    /// <param name="acoustIdClientKey">The app's AcoustId Key</param>
    /// <param name="acoustIdUserKey">The users's AcoustId Key</param>
    /// <param name="musicBrainzRecordingId">The MusicBrainz recourding id of the file, if available</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> SubmitToAcoustIdAsync(string acoustIdClientKey, string acoustIdUserKey, string? musicBrainzRecordingId)
    {
        if (Fingerprint == _("ERROR"))
        {
            return false;
        }
        AcoustID.Configuration.ClientKey = acoustIdClientKey;
        var service = new AcoustID.Web.SubmitService(acoustIdUserKey);
        if (!string.IsNullOrEmpty(musicBrainzRecordingId))
        {
            var response = await service.SubmitAsync(new AcoustID.Web.SubmitRequest(Fingerprint, Duration)
            {
                MBID = musicBrainzRecordingId
            });
            return string.IsNullOrEmpty(response.ErrorMessage);
        }
        else
        {
            var response = await service.SubmitAsync(new AcoustID.Web.SubmitRequest(Fingerprint, Duration)
            {
                Title = Title,
                Artist = Artist,
                Album = Album,
                AlbumArtist = AlbumArtist,
                Year = Year,
                TrackNumber = Track
            });
            return string.IsNullOrEmpty(response.ErrorMessage);
        }
    }

    /// <summary>
    /// Compares this with other
    /// </summary>
    /// <param name="other">The MusicFile object to compare to</param>
    /// <returns>-1 if this is less than other. 0 if this is equal to other. 1 if this is greater than other</returns>
    /// <exception cref="NullReferenceException">Thrown if other is null</exception>
    public int CompareTo(MusicFile? other)
    {
        if (other == null)
        {
            throw new NullReferenceException();
        }
        if (this < other)
        {
            return -1;
        }
        if (this > other)
        {
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this MusicFile
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is MusicFile toCompare)
        {
            return Equals(toCompare);
        }
        return false;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this MusicFile
    /// </summary>
    /// <param name="obj">The MusicFile? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(MusicFile? obj) => Path == obj?.Path;

    /// <summary>
    /// Gets a hash code for the object
    /// </summary>
    /// <returns>The hash code for the object</returns>
    public override int GetHashCode() => Path.GetHashCode();

    /// <summary>
    /// Compares two MusicFile objects by ==
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(MusicFile? a, MusicFile? b) => a?.Path == b?.Path;

    /// <summary>
    /// Compares two MusicFile objects by !=
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(MusicFile? a, MusicFile? b) => a?.Path != b?.Path;

    /// <summary>
    /// Compares two MusicFile objects by less than
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a is less than b, else false</returns>
    public static bool operator <(MusicFile? a, MusicFile? b)
    {
        return SortFilesBy switch
        {
            SortBy.Filename => CompareFilename(a?.Filename, b?.Filename) == -1,
            SortBy.Path => ComparePath(a?.Path, b?.Path) == -1,
            SortBy.Title => a?.Title.CompareTo(b?.Title) == -1,
            SortBy.Artist => a?.Artist.CompareTo(b?.Artist) == -1
                || a?.Artist == b?.Artist && a?.Album.CompareTo(b?.Album) == -1
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber < b?.DiscNumber
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track < b?.Track
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == -1,
            SortBy.Album => a?.Album.CompareTo(b?.Album) == -1
                || a?.Album == b?.Album && a?.DiscNumber < b?.DiscNumber
                || a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track < b?.Track
                || a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == -1,
            SortBy.Year => a?.Year.CompareTo(b?.Year) == -1
                || a?.Year == b?.Year && a?.Album.CompareTo(b?.Album) == -1
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber < b?.DiscNumber
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track < b?.Track
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == -1,
            SortBy.Track => a?.Track.CompareTo(b?.Track) == -1
                || a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == -1,
            SortBy.Genre => a?.Genre.CompareTo(b?.Genre) == -1
                || a?.Genre == b?.Genre && a?.Album.CompareTo(b?.Album) == -1
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber < b?.DiscNumber
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track < b?.Track
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == -1,
            _ => false
        };
    }

    /// <summary>
    /// Compares two MusicFile objects by greater than
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a is greater than b, else false</returns>
    public static bool operator >(MusicFile? a, MusicFile? b)
    {
        return SortFilesBy switch
        {
            SortBy.Filename => CompareFilename(a?.Filename, b?.Filename) == 1,
            SortBy.Path => ComparePath(a?.Path, b?.Path) == 1,
            SortBy.Title => a?.Title.CompareTo(b?.Title) == 1,
            SortBy.Artist => a?.Artist.CompareTo(b?.Artist) == 1
                || a?.Artist == b?.Artist && a?.Album.CompareTo(b?.Album) == 1
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber > b?.DiscNumber
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track > b?.Track
                || a?.Artist == b?.Artist && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == 1,
            SortBy.Album => a?.Album.CompareTo(b?.Album) == 1
                || a?.Album == b?.Album && a?.DiscNumber > b?.DiscNumber
                || a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track > b?.Track
                || a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == 1,
            SortBy.Year => a?.Year.CompareTo(b?.Year) == 1
                || a?.Year == b?.Year && a?.Album.CompareTo(b?.Album) == 1
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber > b?.DiscNumber
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track > b?.Track
                || a?.Year == b?.Year && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == 1,
            SortBy.Track => a?.Track.CompareTo(b?.Track) == 1
                || a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == 1,
            SortBy.Genre => a?.Genre.CompareTo(b?.Genre) == 1
                || a?.Genre == b?.Genre && a?.Album.CompareTo(b?.Album) == 1
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber > b?.DiscNumber
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track > b?.Track
                || a?.Genre == b?.Genre && a?.Album == b?.Album && a?.DiscNumber == b?.DiscNumber && a?.Track == b?.Track && a?.Title.CompareTo(b?.Title) == 1,
            _ => false
        };
    }

    /// <summary>
    /// Compares two filenames
    /// </summary>
    /// <param name="a">First filename</param>
    /// <param name="b">Second filename</param>
    /// <returns>-1 if a &lt; b, 1 if a &gt; b, else 0</returns>
    private static int CompareFilename(string? a, string? b)
    {
        var padA = Regex.Replace(a ?? "", @"\d+", match => match.Value.PadLeft(4, '0'));
        var padB = Regex.Replace(b ?? "", @"\d+", match => match.Value.PadLeft(4, '0'));
        return padA.CompareTo(padB);
    }

    /// <summary>
    /// Compares two paths
    /// </summary>
    /// <param name="a">First path</param>
    /// <param name="b">Second path</param>
    /// <returns>-1 if a &lt; b, 1 if a &gt; b, else 0</returns>
    private static int ComparePath(string? a, string? b)
    {
        var parentA = System.IO.Path.GetDirectoryName(a) ?? "";
        var parentB = System.IO.Path.GetDirectoryName(b) ?? "";
        if (parentA != parentB)
        {
            return CompareFilename(parentA, parentB);
        }
        return CompareFilename(System.IO.Path.GetFileName(a), System.IO.Path.GetFileName(b));
    }
}
