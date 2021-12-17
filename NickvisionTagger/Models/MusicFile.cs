using NickvisionTagger.Extensions;
using System;

namespace NickvisionTagger.Models
{
    public class MusicFile : IComparable<MusicFile>
    {
        private TagLib.File _file;

        public string Path { get; private set; }

        public MusicFile(string path)
        {
            Path = path;
            _file = TagLib.File.Create(path);
        }

        public string Filename
        {
            get => System.IO.Path.GetFileName(Path);

            set
            {
                var newPath = $"{System.IO.Path.GetDirectoryName(Path)}\\{value}";
                if (!value.Contains(DotExtension))
                {
                    newPath += DotExtension;
                }
                _file.Dispose();
                _file = null;
                System.IO.File.Move(Path, newPath, true);
                Path = newPath;
                _file = TagLib.File.Create(Path);
            }
        }

        public string DotExtension => System.IO.Path.GetExtension(Path);

        public string Title
        {
            get => _file.Tag.Title;

            set => _file.Tag.Title = value;
        }

        public string Artist
        {
            get => _file.Tag.FirstPerformer;

            set => _file.Tag.Performers = new string[] { value ?? "" };
        }

        public string Album
        {
            get => _file.Tag.Album;

            set => _file.Tag.Album = value;
        }

        public uint Year
        {
            get => _file.Tag.Year;

            set => _file.Tag.Year = value;
        }

        public uint Track
        {
            get => _file.Tag.Track;

            set => _file.Tag.Track = value;
        }

        public string AlbumArtist
        {
            get => _file.Tag.FirstAlbumArtist;

            set => _file.Tag.AlbumArtists = new string[] { value ?? "" };
        }

        public string Genre
        {
            get => _file.Tag.FirstGenre;

            set => _file.Tag.Genres = new string[] { value ?? "" };
        }

        public string Comment
        {
            get => _file.Tag.Comment;

            set => _file.Tag.Comment = value;
        }

        public TimeSpan Duration => _file.Properties.Duration;

        public string DurationAsString => Duration.DurationToString();

        public long FileSize => new System.IO.FileInfo(Path).Length;

        public string FileSizeAsString => FileSize.FileSizeToString();

        public void SaveTag() => _file.Save();

        public void RemoveTag()
        {
            Title = "";
            Artist = "";
            Album = "";
            Year = 0;
            Track = 0;
            AlbumArtist = "";
            Genre = "";
            Comment = "";
            SaveTag();
        }

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
                if (string.IsNullOrEmpty(Artist))
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

        public int CompareTo(MusicFile toCompare) => Filename.CompareTo(toCompare.Filename);

        public static bool operator ==(MusicFile a, MusicFile b) => a.Path == b.Path;

        public static bool operator !=(MusicFile a, MusicFile b) => a.Path != b.Path;

        public static bool operator <(MusicFile a, MusicFile b) => a.CompareTo(b) == -1;

        public static bool operator >(MusicFile a, MusicFile b) => a.CompareTo(b) == 1;
    }
}
