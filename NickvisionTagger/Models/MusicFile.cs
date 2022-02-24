using ATL;
using NickvisionTagger.Extensions;
using System;

namespace NickvisionTagger.Models;

public class MusicFile : IComparable<MusicFile>
{
    private Track? _file;

    public string Path { get; private set; }

    public MusicFile(string path)
    {
        Path = path;
        _file = new Track(path);
    }

    public string Filename
    {
        get => System.IO.Path.GetFileName(Path);

        set
        {
            var newPath = $"{System.IO.Path.GetDirectoryName(Path)}{System.IO.Path.DirectorySeparatorChar}{value}";
            if (!value.Contains(DotExtension))
            {
                newPath += DotExtension;
            }
            _file = null;
            System.IO.File.Move(Path, newPath, true);
            Path = newPath;
            _file = new Track(Path);
        }
    }

    public string DotExtension => System.IO.Path.GetExtension(Path);

    public string Title
    {
        get => _file!.Title;

        set => _file!.Title = value;
    }

    public string Artist
    {
        get => _file!.Artist;

        set => _file!.Artist = value;
    }

    public string Album
    {
        get => _file!.Album;

        set => _file!.Album = value;
    }

    public int? Year
    {
        get => _file!.Year;

        set => _file!.Year = value;
    }

    public int? Track
    {
        get => _file!.TrackNumber;

        set => _file!.TrackNumber = value;
    }

    public string AlbumArtist
    {
        get => _file!.AlbumArtist;

        set => _file!.AlbumArtist = value;
    }

    public string Composer
    {
        get => _file!.Composer;

        set => _file!.Composer = value;
    }

    public string Genre
    {
        get => _file!.Genre;

        set => _file!.Genre = value;
    }

    public string Comment
    {
        get => _file!.Comment;

        set => _file!.Comment = value;
    }

    public int Duration => _file!.Duration;

    public string DurationAsString => Duration.DurationToString();

    public long FileSize => new System.IO.FileInfo(Path).Length;

    public string FileSizeAsString => FileSize.FileSizeToString();

    public void SaveTag() => _file!.Save();

    public void RemoveTag() => _file!.Remove();

    public void FilenameToTag(string formatString)
    {
        if (formatString == "%artist%- %title%")
        {
            var dashIndex = Filename.IndexOf("- ");
            if (dashIndex == -1)
            {
                throw new ArgumentException("Invalid Filename. No dash was found.");
            }
            Artist = Filename.Substring(0, dashIndex);
            Title = Filename.Substring(dashIndex + 2, Filename.IndexOf(DotExtension) - (Artist.Length + 2));
            SaveTag();
        }
        else if (formatString == "%title%- %artist%")
        {
            var dashIndex = Filename.IndexOf("- ");
            if (dashIndex == -1)
            {
                throw new ArgumentException("Invalid Filename. No dash was found.");
            }
            Title = Filename.Substring(0, dashIndex);
            Artist = Filename.Substring(dashIndex + 2, Filename.IndexOf(DotExtension) - (Title.Length + 2));
            SaveTag();
        }
        else if (formatString == "%title%")
        {
            Title = Filename.Substring(0, Filename.IndexOf(DotExtension));
            SaveTag();
        }
        else
        {
            throw new ArgumentException("Invalid format string.");
        }
    }

    public void TagToFilename(string formatString)
    {
        if (formatString == "%artist%- %title%")
        {
            if (string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Title))
            {
                throw new ArgumentException("Invalid Tag. Artist and/or title are empty.");
            }
            Filename = $"{Artist}- {Title}{DotExtension}";
        }
        else if (formatString == "%title%- %artist%")
        {
            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Artist))
            {
                throw new ArgumentException("Invalid Tag. Title and/or artist are empty.");
            }
            Filename = $"{Title}- {Artist}{DotExtension}";
        }
        else if (formatString == "%title%")
        {
            if (string.IsNullOrEmpty(Title))
            {
                throw new ArgumentException("Invalid Tag. Title is empty.");
            }
            Filename = $"{Title}{DotExtension}";
        }
        else
        {
            throw new ArgumentException("Invalid Format String.");
        }
    }

    public override int GetHashCode() => Path.GetHashCode();

    public override bool Equals(object? obj)
    {
        if(obj is MusicFile other)
        {
            return Path == other.Path;
        }
        return false;
    }

    public int CompareTo(MusicFile? toCompare) => Filename.CompareTo(toCompare?.Filename);

    public static bool operator ==(MusicFile a, MusicFile b) => a.Path == b.Path;

    public static bool operator !=(MusicFile a, MusicFile b) => a.Path != b.Path;

    public static bool operator <(MusicFile a, MusicFile b) => a.CompareTo(b) == -1;

    public static bool operator >(MusicFile a, MusicFile b) => a.CompareTo(b) == 1;
}