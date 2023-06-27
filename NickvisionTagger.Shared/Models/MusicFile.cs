using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagLib;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of a music file
/// </summary>
public class MusicFile : IComparable<MusicFile>, IEquatable<MusicFile>
{
    private string _dotExtension;
    private string _filename;
    private DateTime _modificationTimestamp;
    private string _fingerprint;
    
    /// <summary>
    /// The path of the music file
    /// </summary>
    public string Path { get; init; }
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
    public uint Year { get; set; }
    /// <summary>
    /// The track of the music file
    /// </summary>
    public uint Track { get; set; }
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
    /// The album art of the music file
    /// </summary>
    public TagLib.ByteVector AlbumArt { get; set; }
    /// <summary>
    /// The duration of the music file
    /// </summary>
    public double Duration { get; private set; }
    
    /// <summary>
    /// The size of the music file
    /// </summary>
    public long FileSize => new FileInfo(Path).Length;
    
    /// <summary>
    /// Constructs a MusicFile
    /// </summary>
    /// <param name="path">The path of the music file</param>
    public MusicFile(string path)
    {
        Path = path;
        _dotExtension = System.IO.Path.GetExtension(Path).ToLower();
        _filename = System.IO.Path.GetFileName(Path);
        _modificationTimestamp = System.IO.File.GetLastWriteTime(Path);
        _fingerprint = "";
        Title = "";
        Artist = "";
        Album = "";
        Year = 0;
        Track = 0;
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        AlbumArt = new ByteVector();
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
            if(System.IO.File.Exists(newFilename))
            {
                throw new ArgumentException($"A file already exists with this filename: {newFilename}");
            }
            _filename = value;
        }
    }
    
    /// <summary>
    /// The fingerprint of the music file
    /// </summary>
    public string Fingerprint
    {
        get
        {
            if(string.IsNullOrEmpty(_fingerprint) || _fingerprint == "ERROR")
            {
                using var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "fpcalc",
                        Arguments = $"\"{Path}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                _fingerprint = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if(process.ExitCode != 0)
                {
                    _fingerprint = "ERROR";
                }
            }
            return _fingerprint;
        }
    }
    
    /// <summary>
    /// Loads the tag metadata from the file on disk (discarding any unapplied metadata)
    /// </summary>
    /// <returns>True if successful, else false</returns>
    public bool LoadTagFromDisk()
    {
        TagLib.File? file = null;
        Tag? tag = null;
        if(_dotExtension == ".mp3")
        {
            file = new TagLib.Mpeg.File(Path);
            tag = file.GetTag(TagTypes.Id3v2, true);
        }
        else if(_dotExtension == ".m4a" || _dotExtension == ".m4b")
        {
            file = new TagLib.Mpeg4.File(Path);
            tag = file.GetTag(TagTypes.Apple, true);
        }
        else if(_dotExtension == ".ogg" || _dotExtension == ".opus" || _dotExtension == ".oga")
        {
            file = new TagLib.Ogg.File(Path);
            tag = file.GetTag(TagTypes.Xiph, true);
        }
        else if(_dotExtension == ".flac")
        {
            file = new TagLib.Flac.File(Path);
            tag = file.GetTag(TagTypes.Xiph, true);
        }
        else if(_dotExtension == ".wma")
        {
            file = new TagLib.Asf.File(Path);
            tag = file.GetTag(TagTypes.Asf, true);
        }
        else if(_dotExtension == ".wav")
        {
            file = new TagLib.Riff.File(Path);
            tag = file.GetTag(TagTypes.Id3v2, true);
        }
        if(file != null && tag != null)
        {
            Title = tag.Title;
            Artist = tag.FirstPerformer;
            Album = tag.Album;
            Year = tag.Year;
            Track = tag.Track;
            AlbumArtist = tag.FirstAlbumArtist;
            Genre = tag.FirstGenre;
            Comment = tag.Comment;
            Duration = file.Properties.Duration.TotalSeconds;
            if(tag.Pictures.Length > 0)
            {
                foreach(var picture in tag.Pictures)
                {
                    if(picture.Type == PictureType.FrontCover)
                    {
                        AlbumArt = picture.Data;
                        break;
                    }
                }
            }
            file.Dispose();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Loads tag metadata from MusicBrainz (discarding any unapplied metadata)
    /// </summary>
    /// <param name="acoustIdClientKey">The app's AcoustId Key</param>
    /// <param name="overwriteTagWithMusicBrainz">Whether or not to overwrite a tag's existing data with data from MusicBrainz</param>
    /// <param name="overwriteAlbumArtWithMusicBrainz">Whether or not to overwrite a tag's existing album art with album art from MusicBrainz</param>
    /// <returns>True if successful, else false</returns>
    public bool LoadTagFromMusicBrainz(string acoustIdClientKey, bool overwriteTagWithMusicBrainz, bool overwriteAlbumArtWithMusicBrainz)
    {
        // TODO
        return false;
    }
    
    /// <summary>
    /// Saves the tag of the music file to disk
    /// </summary>
    /// <param name="preserveModificationTimestamp">Whether or not to preserve (not change) a file's modification timestamp</param>
    /// <returns>True if successful, else false</returns>
    public bool SaveTagToDisk(bool preserveModificationTimestamp)
    {
        TagLib.File? file = null;
        Tag? tag = null;
        if(_dotExtension == ".mp3")
        {
            file = new TagLib.Mpeg.File(Path);
            tag = file.GetTag(TagTypes.Id3v2, true);
        }
        else if(_dotExtension == ".m4a" || _dotExtension == ".m4b")
        {
            file = new TagLib.Mpeg4.File(Path);
            tag = file.GetTag(TagTypes.Apple, true);
        }
        else if(_dotExtension == ".ogg" || _dotExtension == ".opus" || _dotExtension == ".oga")
        {
            file = new TagLib.Ogg.File(Path);
            tag = file.GetTag(TagTypes.Xiph, true);
        }
        else if(_dotExtension == ".flac")
        {
            file = new TagLib.Flac.File(Path);
            tag = file.GetTag(TagTypes.Xiph, true);
        }
        else if(_dotExtension == ".wma")
        {
            file = new TagLib.Asf.File(Path);
            tag = file.GetTag(TagTypes.Asf, true);
        }
        else if(_dotExtension == ".wav")
        {
            file = new TagLib.Riff.File(Path);
            tag = file.GetTag(TagTypes.Id3v2, true);
        }
        if(file != null && tag != null)
        {
            tag.Title = Title;
            tag.Performers = new string[] { Artist };
            tag.Album = Album;
            tag.Year = Year;
            tag.Track = Track;
            tag.AlbumArtists = new string[] { AlbumArtist };
            tag.Genres = new string[] { Genre };
            tag.Comment = Comment;
            tag.Pictures = new IPicture[]
            {
                new Picture(AlbumArt)
                {
                    MimeType = "image/jpeg",
                    Type = PictureType.FrontCover
                }
            };
            file.Save();
            file.Dispose();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Clears a file's tag (does not save to disk)
    /// </summary>
    public void ClearTag()
    {
        Title = "";
        Artist = "";
        Album = "";
        Year = 0;
        Track = 0;
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        AlbumArt = new ByteVector();
    }
    
    /// <summary>
    /// Uses the music file's filename to fill in tag information based on the format string
    /// </summary>
    /// <param name="formatString">Th format string</param>
    /// <returns>True if successful, else false</returns>
    public bool FilenameToTag(string formatString)
    {
        // TODO
        return false;
    }
    
    /// <summary>
    /// Uses the music file's tag to set the file's filename in the format of the format string
    /// </summary>
    /// <param name="formatString">Th format string</param>
    /// <returns>True if successful, else false</returns>
    public bool TagToFilename(string formatString)
    {
        // TODO
        return false;
    }
    
    /// <summary>
    /// Loads tag metadata from MusicBrainz (discarding any unapplied metadata)
    /// </summary>
    /// <param name="acoustIdClientKey">The app's AcoustId Key</param>
    /// <param name="acoustIdUserKey">The users's AcoustId Key</param>
    /// <param name="musicBrainzRecordingId">The MusicBrainz recourding id of the file</param>
    /// <returns>True if successful, else false</returns>
    public bool SubmitToAcoustId(string acoustIdClientKey, string acoustIdUserKey, string musicBrainzRecordingId)
    {
        // TODO
        return false;
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
        else if (this == other)
        {
            return 0;
        }
        else
        {
            return 1;
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
    public static bool operator !=(MusicFile? a, MusicFile? b) => a?.Path != b?.Path;

    /// <summary>
    /// Compares two MusicFile objects by <
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a < b, else false</returns>
    public static bool operator <(MusicFile? a, MusicFile? b) => a?.Filename.CompareTo(b?.Filename) == -1;

    /// <summary>
    /// Compares two MusicFile objects by >
    /// </summary>
    /// <param name="a">The first MusicFile object</param>
    /// <param name="b">The second MusicFile object</param>
    /// <returns>True if a > b, else false</returns>
    public static bool operator >(MusicFile? a, MusicFile? b) => a?.Filename.CompareTo(b?.Filename) == 1;
}