using AcoustID.Web;
using ATL;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using NickvisionTagger.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of a music file
/// NOTE: Custom properties are stored in a tag under the "tagger-custom" field and is a string in the following format: [[prop]]value\n[[prop]]value
/// </summary>
public class MusicFile : IComparable<MusicFile>, IEquatable<MusicFile>
{
    private string _dotExtension;
    private string _filename;
    private DateTime _modificationTimestamp;
    private string _fingerprint;
    private Dictionary<string, string> _customProperties;
    
    /// <summary>
    /// What to sort files in a music folder by
    /// </summary>
    public static SortBy SortFilesBy { get; set; }

    /// <summary>
    /// The path of the music file
    /// </summary>
    public string Path { get; private set; }
    /// <summary>
    /// The title of the music file
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The artist of the music file
    /// </summary>
    public string Artist { get; set; }
    /// <summary>
    /// The album of the music file
    /// </summary>
    public string Album { get; set; }
    /// <summary>
    /// The year of the music file
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// The track of the music file
    /// </summary>
    public int Track { get; set; }
    /// <summary>
    /// The album of the music file
    /// </summary>
    public string AlbumArtist { get; set; }
    /// <summary>
    /// The genre of the music file
    /// </summary>
    public string Genre { get; set; }
    /// <summary>
    /// The comment of the music file
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// The composer of the music file
    /// </summary>
    public string Composer { get; set; }
    /// <summary>
    /// The description of the music file
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The publisher of the music file
    /// </summary>
    public string Publisher { get; set; }
    /// <summary>
    /// The front album art of the music file
    /// </summary>
    public byte[] FrontAlbumArt { get; set; }
    /// <summary>
    /// The back album art of the music file
    /// </summary>
    public byte[] BackAlbumArt { get; set; }
    /// <summary>
    /// The duration of the music file
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// The size of the music file
    /// </summary>
    public long FileSize => new FileInfo(Path).Length;
    /// <summary>
    /// Whether or not the tag is empty
    /// </summary>
    public bool IsTagEmpty => Title == "" && Artist == "" && Album == "" && Year == 0 && Track == 0 && AlbumArtist == "" && Genre == "" && Comment == "" && Composer == "" && Description == "" && Publisher == "" && FrontAlbumArt.Length == 0 && BackAlbumArt.Length == 0 && _customProperties.Count == 0;

    /// <summary>
    /// Constructs a static MusicFile
    /// </summary>
    static MusicFile()
    {
        SortFilesBy = SortBy.Filename;
    }

    /// <summary>
    /// Constructs a MusicFile
    /// </summary>
    /// <param name="path">The path of the music file</param>
    public MusicFile(string path)
    {
        Path = path;
        _dotExtension = System.IO.Path.GetExtension(Path).ToLower();
        _filename = System.IO.Path.GetFileName(Path);
        _modificationTimestamp = File.GetLastWriteTime(Path);
        _fingerprint = "";
        _customProperties = new Dictionary<string, string>();
        Title = "";
        Artist = "";
        Album = "";
        Year = 0;
        Track = 0;
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        Composer = "";
        Description = "";
        Publisher = "";
        FrontAlbumArt = Array.Empty<byte>();
        BackAlbumArt = Array.Empty<byte>();
        Duration = 0;
        LoadTagFromDisk();
    }
    
    /// <summary>
    /// The filename of the music file
    /// </summary>
    public string Filename
    {
        get => _filename;
        
        set
        {
            var newFilename = value;
            if(System.IO.Path.GetExtension(newFilename).ToLower() != _dotExtension)
            {
                newFilename += _dotExtension;
            }
            if(File.Exists(newFilename))
            {
                throw new ArgumentException($"A file already exists with this filename: {newFilename}");
            }
            _filename = newFilename;
        }
    }
    
    /// <summary>
    /// The fingerprint of the music file
    /// </summary>
    public string Fingerprint
    {
        get
        {
            if(string.IsNullOrEmpty(_fingerprint) || _fingerprint == _("ERROR"))
            {
                using var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = DependencyManager.FpcalcPath,
                        Arguments = $"\"{Path}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                _fingerprint = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    _fingerprint = _fingerprint.Substring(_fingerprint.IndexOf("FINGERPRINT=") + 12);
                    _fingerprint = _fingerprint.Remove(_fingerprint.Length - 1);
                }
                else
                {
                    _fingerprint = _("ERROR");
                }
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
            var names = _customProperties.Keys.ToList();
            names.Sort();
            return names;
        }
    }

    /// <summary>
    /// Loads the tag metadata from the file on disk (discarding any unapplied metadata)
    /// </summary>
    /// <exception cref="FileLoadException">Thrown if unable to load MusicFile because of corrupted tag</exception>
    /// <exception cref="ArgumentException">Thrown if the file path is invalid or if the file is not supported</exception>
    /// <returns>True if successful, else false</returns>
    public bool LoadTagFromDisk()
    {
        _filename = System.IO.Path.GetFileName(Path);
        _customProperties.Clear();
        try
        {
            var track = new Track(Path);
            Title = track.Title ?? "";
            Artist = track.Artist ?? "";
            Album = track.Album ?? "";
            Year = track.Year ?? 0;
            Track = track.TrackNumber ?? 0;
            AlbumArtist = track.AlbumArtist ?? "";
            Genre = track.Genre ?? "";
            Comment = track.Comment ?? "";
            Composer = track.Composer ?? "";
            Description = track.Description ?? "";
            Publisher = track.Publisher ?? "";
            Duration = track.Duration;
            foreach(var picture in track.EmbeddedPictures)
            {
                if(picture.PicType == PictureInfo.PIC_TYPE.Front)
                {
                    FrontAlbumArt = picture.PictureData;
                }
                if(picture.PicType == PictureInfo.PIC_TYPE.Back)
                {
                    BackAlbumArt = picture.PictureData;
                }
            }
            foreach(var pair in track.AdditionalFields)
            {
                _customProperties.Add(pair.Key, pair.Value);
            }
            return true;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message + "\n" + e.StackTrace);
            throw new FileLoadException($"Unable to load music file: \"{Path}\". Tag could be corrupted.");
        }
    }
    
    /// <summary>
    /// Loads tag metadata from MusicBrainz (discarding any unapplied metadata)
    /// </summary>
    /// <param name="acoustIdClientKey">The app's AcoustId Key</param>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="overwriteTagWithMusicBrainz">Whether or not to overwrite a tag's existing data with data from MusicBrainz</param>
    /// <param name="overwriteAlbumArtWithMusicBrainz">Whether or not to overwrite a tag's existing album art with album art from MusicBrainz</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> LoadTagFromMusicBrainzAsync(string acoustIdClientKey, AppInfo appInfo, bool overwriteTagWithMusicBrainz, bool overwriteAlbumArtWithMusicBrainz)
    {
        //Use AcoustID to get MBID
        AcoustID.Configuration.ClientKey = acoustIdClientKey;
        var service = new LookupService();
        var response = await service.GetAsync(Fingerprint, Duration);
        if(response.Results.Count > 0)
        {
            //AcoustID Results
            var bestResult = response.Results[0];
            foreach (var r in response.Results)
            {
                if(r.Score > bestResult.Score && r.Recordings.Count > 0)
                {
                    bestResult = r;
                }
            }
            //AcoustID Recordings
            if(bestResult.Recordings.Count < 1)
            {
                return false;
            }
            var bestRecordingId = bestResult.Recordings[0].Id;
            foreach (var r in bestResult.Recordings)
            {
                if(r.Title != "")
                {
                    bestRecordingId = r.Id;
                    break;
                }
            }
            //MusicBrainz
            using var query = new Query(appInfo.Version, appInfo.Version, "mailto:nlogozzo225@gmail.com");
            IRecording? recording = null;
            IRelease? album = null;
            try
            {
                recording = await query.LookupRecordingAsync(Guid.Parse(bestRecordingId));
            }
            catch
            {
                return false;
            }
            if(recording.Releases != null && recording.Releases.Count > 0)
            {
                album = recording.Releases[0];
            }
            if(overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Title))
            {
                Title = recording.Title ?? "";
            }
            if(overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Artist))
            {
                if(recording.ArtistCredit != null && recording.ArtistCredit.Count > 0)
                {
                    Artist = recording.ArtistCredit[0].Name ?? "";
                }
            }
            if(overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Album))
            {
                if(album != null)
                {
                    Album = album.Title ?? "";
                }
            }
            if(overwriteTagWithMusicBrainz || Year == 0)
            {
                if(recording.FirstReleaseDate != null)
                {
                    Year = recording.FirstReleaseDate.Year ?? 0;
                }
            }
            if(overwriteTagWithMusicBrainz || string.IsNullOrEmpty(AlbumArtist))
            {
                if(album != null && album.ArtistCredit != null && album.ArtistCredit.Count > 0)
                {
                    AlbumArtist = album.ArtistCredit[0].Name ?? "";
                }
            }
            if(overwriteTagWithMusicBrainz || string.IsNullOrEmpty(Genre))
            {
                if(recording.Genres != null && recording.Genres.Count > 0)
                {
                    Genre = recording.Genres[0].Name ?? "";
                }
            }
            if(overwriteAlbumArtWithMusicBrainz || FrontAlbumArt.Length == 0)
            {
                if(album != null && album.CoverArtArchive != null && album.CoverArtArchive.Count > 0)
                {
                    using var caQuery = new CoverArt(appInfo.Version, appInfo.Version, "mailto:nlogozzo225@gmail.com");
                    CoverArtImage? img = null;
                    try
                    {
                        img = await caQuery.FetchFrontAsync(Guid.Parse(bestRecordingId));
                    }
                    catch { }
                    if(img != null)
                    {
                        var reader = new BinaryReader(img.Data);
                        FrontAlbumArt = reader.ReadBytes((int)img.Data.Length);
                        reader.Dispose();
                        img.Dispose();
                    }
                }
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Saves the tag of the music file to disk
    /// </summary>
    /// <param name="preserveModificationTimestamp">Whether or not to preserve (not change) a file's modification timestamp</param>
    /// <returns>True if successful, else false</returns>
    public bool SaveTagToDisk(bool preserveModificationTimestamp)
    {
        if(System.IO.Path.GetFileName(Path) != Filename)
        {
            var newPath = $"{System.IO.Path.GetDirectoryName(Path)}{System.IO.Path.DirectorySeparatorChar}{Filename}";
            File.Move(Path, newPath);
            Path = newPath;
        }
        var track = new Track(Path)
        {
            Title = Title,
            Artist = Artist,
            Album = Album,
            Year = Year,
            TrackNumber = Track,
            AlbumArtist = AlbumArtist,
            Genre = Genre,
            Comment = Comment,
            Composer = Composer,
            Description = Description,
            Publisher = Publisher
        };
        foreach(var picture in track.EmbeddedPictures.ToArray())
        {
            track.EmbeddedPictures.Remove(picture);
        }
        if(FrontAlbumArt.Length > 0)
        {
            track.EmbeddedPictures.Add(PictureInfo.fromBinaryData(FrontAlbumArt, PictureInfo.PIC_TYPE.Front));
        }
        if(BackAlbumArt.Length > 0)
        {
            track.EmbeddedPictures.Add(PictureInfo.fromBinaryData(BackAlbumArt, PictureInfo.PIC_TYPE.Front));
        }
        foreach(var key in track.AdditionalFields.Keys.ToArray())
        {
            track.AdditionalFields.Remove(key);
        }
        foreach(var pair in _customProperties)
        {
            track.AdditionalFields.Add(pair.Key, pair.Value);
        }
        var res = track.Save();
        if(res)
        {
            if(preserveModificationTimestamp)
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
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        FrontAlbumArt = Array.Empty<byte>();
        BackAlbumArt = Array.Empty<byte>();
        _customProperties.Clear();
    }

    /// <summary>
    /// Gets a custom property value
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <returns>The value of the custom property. Null if no custom property exists with the provided name</returns>
    public string? GetCustomProperty(string name)
    {
        if(!_customProperties.ContainsKey(name))
        {
            return null;
        }
        return _customProperties[name];
    }

    /// <summary>
    /// Sets a custom property value
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <param name="value">The value of the custom property</param>
    public void SetCustomProperty(string name, string value) => _customProperties[name] = value;

    /// <summary>
    /// Removes a custom property
    /// </summary>
    /// <param name="name">The name of the custom property</param>
    /// <returns>Whether or not the property was removed. If no property exists with the provided name, false will be returned</returns>
    public bool RemoveCustomProperty(string name)
    {
        if(!_customProperties.ContainsKey(name))
        {
            return false;
        }
        _customProperties.Remove(name);
        return true;
    }
    
    /// <summary>
    /// Uses the music file's filename to fill in tag information based on the format string
    /// </summary>
    /// <param name="formatString">Th format string</param>
    /// <returns>True if successful, else false</returns>
    public bool FilenameToTag(string formatString)
    {
        if (formatString == "%artist%- %title%")
        {
            var dashIndex = Filename.IndexOf("- ");
            if(dashIndex == -1)
            {
                return false;
            }
            Artist = Filename.Substring(0, dashIndex);
            Title = Filename.Substring(dashIndex + 2, Filename.IndexOf(System.IO.Path.GetExtension(Path)) - (Artist.Length + 2));
        }
        else if (formatString == "%title%- %artist%")
        {
            var dashIndex = Filename.IndexOf("- ");
            if(dashIndex == -1)
            {
                return false;
            }
            Title = Filename.Substring(0, dashIndex);
            Artist = Filename.Substring(dashIndex + 2, Filename.IndexOf(System.IO.Path.GetExtension(Path)) - (Title.Length + 2));
        }
        else if (formatString == "%track%- %title%")
        {
            var dashIndex = Filename.IndexOf("- ");
            if(dashIndex == -1)
            {
                return false;
            }
            try
            {
                var trackString = Filename.Substring(0, dashIndex);
                Track = int.Parse(trackString);
                Title = Filename.Substring(dashIndex + 2, Filename.IndexOf(System.IO.Path.GetExtension(Path)) - (trackString.Length + 2));
            }
            catch
            {
                Track = 0;
                Title = "";
            }
        }
        else if (formatString == "%title%")
        {
            Title = Filename.Substring(0, Filename.IndexOf(System.IO.Path.GetExtension(Path)));
        }
        else
        {
            return false;
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
        if (formatString == "%artist%- %title%")
        {
            if(string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Title))
            {
                return false;
            }
            try
            {
                Filename = $"{Artist}- {Title}{_dotExtension}";
            }
            catch
            {
                return false;
            }
        }
        else if (formatString == "%title%- %artist%")
        {
            if(string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Artist))
            {
                return false;
            }
            try
            {
                Filename = $"{Title}- {Artist}{_dotExtension}";
            }
            catch
            {
                return false;
            }
        }
        else if (formatString == "%track%- %title%")
        {
            if(string.IsNullOrEmpty(Title))
            {
                return false;
            }
            try
            {
                Filename = $"{Track:D2}- {Title}{_dotExtension}";
            }
            catch
            {
                return false;
            }
        }
        else if (formatString == "%title%")
        {
            if(string.IsNullOrEmpty(Title))
            {
                return false;
            }
            try
            {
                Filename = $"{Title}{_dotExtension}";
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
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
        AcoustID.Configuration.ClientKey = acoustIdClientKey;
        var service = new SubmitService(acoustIdUserKey);
        if(!string.IsNullOrEmpty(musicBrainzRecordingId))
        {
            var response = await service.SubmitAsync(new SubmitRequest(Fingerprint, Duration)
            {
                MBID = musicBrainzRecordingId
            });
            return string.IsNullOrEmpty(response.ErrorMessage);
        }
        else
        {
            var response = await service.SubmitAsync(new SubmitRequest(Fingerprint, Duration)
            {
                Title = Title,
                Artist = Artist,
                Album = Album,
                AlbumArtist = AlbumArtist,
                Year = (int)Year,
                TrackNumber = (int)Track
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
        else if (this > other)
        {
            return 1;
        }
        else
        {
            return 0;
        }
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
            return Path == toCompare.Path;
        }
        return false;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this MusicFile
    /// </summary>
    /// <param name="obj">The MusicFile? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(MusicFile? obj) => Equals((object?)obj);

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
    public static bool operator !=(MusicFile? a, MusicFile? b) =>  a?.Path != b?.Path;

    /// <summary>
    /// Compares two MusicFile objects by <
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a < b, else false</returns>
    public static bool operator <(MusicFile? a, MusicFile? b)
    {
        if(SortFilesBy == SortBy.Title)
        {
            return a?.Title.CompareTo(b?.Title) == -1;
        }
        else if(SortFilesBy == SortBy.Track)
        {
            return a?.Track.CompareTo(b?.Track) == -1 || a?.Track.CompareTo(b?.Track) == 0 && a?.Title.CompareTo(b?.Title) == -1;
        }
        else if(SortFilesBy == SortBy.Path)
        {
            return a?.Path.CompareTo(b?.Path) == -1;
        }
        else if(SortFilesBy == SortBy.Album)
        {
            return a?.Album.CompareTo(b?.Album) == -1 || a?.Album.CompareTo(b?.Album) == 0 && a?.Track.CompareTo(b?.Track) == -1;
        }
        else if(SortFilesBy == SortBy.Genre)
        {
            return a?.Genre.CompareTo(b?.Genre) == -1 || a?.Genre.CompareTo(b?.Genre) == 0 && a?.Path.CompareTo(b?.Path) == -1;
        }
        return a?.Filename.CompareTo(b?.Filename) == -1;
    }

    /// <summary>
    /// Compares two MusicFile objects by >
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a > b, else false</returns>
    public static bool operator >(MusicFile? a, MusicFile? b)
    {
        if(SortFilesBy == SortBy.Title)
        {
            return a?.Title.CompareTo(b?.Title) == 1;
        }
        else if(SortFilesBy == SortBy.Track)
        {
            return a?.Track.CompareTo(b?.Track) == 1 || a?.Track.CompareTo(b?.Track) == 0 && a?.Title.CompareTo(b?.Title) == 1;
        }
        else if(SortFilesBy == SortBy.Path)
        {
            return a?.Path.CompareTo(b?.Path) == 1;
        }
        else if(SortFilesBy == SortBy.Album)
        {
            return a?.Album.CompareTo(b?.Album) == 1 || a?.Album.CompareTo(b?.Album) == 0 && a?.Track.CompareTo(b?.Track) == 1;
        }
        else if(SortFilesBy == SortBy.Genre)
        {
            return a?.Genre.CompareTo(b?.Genre) == 1 || a?.Genre.CompareTo(b?.Genre) == 0 && a?.Path.CompareTo(b?.Path) == 1;
        }
        return a?.Filename.CompareTo(b?.Filename) == 1;
    }
}
