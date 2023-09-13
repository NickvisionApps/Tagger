using ATL.Playlist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Types of a music library
/// </summary>
public enum MusicLibraryType
{
    Folder,
    Playlist
}

/// <summary>
/// A model of a music library
/// </summary>
public class MusicLibrary : IDisposable
{
    private bool _disposed;
    private IPlaylistIO? _playlist;
    
    /// <summary>
    /// An array of supported extensions by Tagger
    /// </summary>
    public static string[] SupportedExtensions { get; }
    
    /// <summary>
    /// The type of the music library
    /// </summary>
    public MusicLibraryType Type { get; init; }
    /// <summary>
    /// The path of the music library
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// Whether or not to include subfolders in scanning for music
    /// </summary>
    public bool IncludeSubfolders { get; set; }
    /// <summary>
    /// What to sort files in a music library by
    /// </summary>
    public SortBy SortFilesBy { get; set; }
    /// <summary>
    /// The list of MusicFile objects from the library
    /// </summary>
    public List<MusicFile> MusicFiles { get; init; }
    /// <summary>
    /// The list of paths of corrupted music files
    /// </summary>
    public List<string> CorruptedFiles { get; init; }
    /// <summary>
    /// A list of genres in the library
    /// </summary>
    public List<string> Genres { get; init;  }

    /// <summary>
    /// The name of the library
    /// </summary>
    /// <remarks>Path for folder, file name for playlist</remarks>
    public string Name => Type == MusicLibraryType.Folder ? Path : System.IO.Path.GetFileNameWithoutExtension(Path);
    
    /// <summary>
    /// Occurs when the loading progress is updated
    /// </summary>
    public event EventHandler<(int Value, int MaxValue, string Message)>? LoadingProgressUpdated;

    /// <summary>
    /// Constructs a static MusicLibrary
    /// </summary>
    static MusicLibrary()
    {
        SupportedExtensions = new string[]
        {
            ".mp3", ".m4a", ".m4b", ".ogg", ".opus", ".oga", ".flac", ".wma", ".wav",
            ".aac", ".aax", ".aa", ".aif", ".aiff", ".aifc", ".dsd", ".dsf", ".ac3", 
            ".gym", ".ape", ".mpv", ".mp+", ".ofr", ".ofs", ".psf", ".psf1", ".psf2", 
            ".minipsf", ".minipsf1", ".minipsf2", ".ssf", ".minissf", ".minidsf", 
            ".gsf", ".minigsf", ".qsf", ".miniqsf", ".spc", ".tak", ".tta", ".vqf", 
            ".bwav", ".bwf", ".vgm", ".vgz", ".wv", ".asf"
        };
    }
    
    /// <summary>
    /// Constructs a MusicLibrary
    /// </summary>
    /// <param name="path">The path of the music library</param>
    /// <exception cref="ArgumentException">Thrown if the path points to an invalid library</exception>
    public MusicLibrary(string path)
    {
        _disposed = false;
        Path = path;
        IncludeSubfolders = true;
        SortFilesBy = SortBy.Filename;
        MusicFiles = new List<MusicFile>();
        CorruptedFiles = new List<string>();
        Genres = new List<string>();
        if (Directory.Exists(Path))
        {
            Type = MusicLibraryType.Folder;
        }
        else if (File.Exists(Path) && Enum.GetValues<PlaylistFormat>().Select(x => x.GetDotExtension()).Contains(System.IO.Path.GetExtension(Path)))
        {
            Type = MusicLibraryType.Playlist;
            _playlist = PlaylistIOFactory.GetInstance().GetPlaylistIO(Path, ATL.Playlist.PlaylistFormat.LocationFormatting.FilePath, ATL.Playlist.PlaylistFormat.FileEncoding.UTF8_NO_BOM);
        }
        else
        {
            throw new ArgumentException("Invalid library path");
        }
    }
    
    /// <summary>
    /// Finalizes the MusicLibrary
    /// </summary>
    ~MusicLibrary() => Dispose(false);
    
    /// <summary>
    /// Frees resources used by the MusicLibrary object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MusicLibrary object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        foreach (var file in MusicFiles)
        {
            file.Dispose();
        }
        _disposed = true;
    }
    
    /// <summary>
    /// Scans the music library for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
    /// </summary>
    /// <returns>Whether or not there are corrupted files</returns>
    public async Task<bool> ReloadMusicFilesAsync()
    {
        MusicFile.SortFilesBy = SortFilesBy;
        await Task.Run(() =>
        {
            foreach (var file in MusicFiles)
            {
                file.Dispose();
            }
        });
        MusicFiles.Clear();
        CorruptedFiles.Clear();
        Genres.Clear();
        await Task.Run(() =>
        {
            var files = Type switch
            {
                MusicLibraryType.Folder => Directory.GetFiles(Path, "*.*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => SupportedExtensions.Contains(System.IO.Path.GetExtension(x).ToLower())).ToList(),
                MusicLibraryType.Playlist => _playlist!.FilePaths.Where(x => File.Exists(x) && SupportedExtensions.Contains(System.IO.Path.GetExtension(x).ToLower())).ToList(),
                _ => new List<string>()
            };
            var i = 0;
            foreach(var path in files)
            {
                try
                {
                    var musicFile = new MusicFile(path);
                    MusicFiles.Add(musicFile);
                    if (!Genres.Contains(musicFile.Genre) && string.IsNullOrEmpty(musicFile.Genre))
                    {
                        Genres.Add(musicFile.Genre);
                    }
                }
                catch (FileLoadException e)
                {
                    CorruptedFiles.Add(path);
                    Console.WriteLine(e);
                }
                i++;
                LoadingProgressUpdated?.Invoke(this, (i, files.Count, $"{i}/{files.Count}"));
            }
            MusicFiles.Sort();
        });
        return CorruptedFiles.Count > 0;
    }

    /// <summary>
    /// Creates a playlist for the music library folder
    /// </summary>
    /// <param name="options">PlaylistOptions</param>
    /// <param name="selectedFiles">A list of indexes of selected files, if available</param>
    /// <returns>True if successful, else false</returns>
    public bool CreatePlaylist(PlaylistOptions options, List<int>? selectedFiles)
    {
        if (Type != MusicLibraryType.Folder || string.IsNullOrEmpty(options.Name))
        {
            return false;
        }
        var path = $"{Path}{System.IO.Path.DirectorySeparatorChar}{options.Name}{options.Format.GetDotExtension()}";
        var playlist = PlaylistIOFactory.GetInstance().GetPlaylistIO(path, ATL.Playlist.PlaylistFormat.LocationFormatting.FilePath, ATL.Playlist.PlaylistFormat.FileEncoding.UTF8_NO_BOM);
        var paths = new List<string>();
        if (options.IncludeOnlySelectedFiles)
        {
            if (selectedFiles == null || selectedFiles.Count == 0)
            {
                return false;
            }
            paths.AddRange(MusicFiles.Where(x => selectedFiles!.Contains(MusicFiles.IndexOf(x))).Select(x => x.Path));
        }
        else
        {
            if (MusicFiles.Count == 0)
            {
                return false;
            }
            paths.AddRange(MusicFiles.Select(x => x.Path));
        }
        playlist.FilePaths = paths;
        return true;
    }
    
    /// <summary>
    /// Adds a file to the playlist
    /// </summary>
    /// <param name="path">The path to the music folder</param>
    /// <returns>True if success, else false</returns>
    public bool AddFileToPlaylist(string path)
    {
        if (Type != MusicLibraryType.Playlist || !File.Exists(path) || !SupportedExtensions.Contains(System.IO.Path.GetExtension(path).ToLower()))
        {
            return false;
        }
        var paths = _playlist!.FilePaths;
        if (paths.Contains(path))
        {
            return false;
        }
        paths.Add(path);
        _playlist.FilePaths = paths;
        return true;
    }

    /// <summary>
    /// Removes files from the playlist
    /// </summary>
    /// <param name="indexes">The indexes of the files to remove</param>
    /// <returns>True if success, else false</returns>
    public bool RemoveFilesFromPlaylist(List<int> indexes)
    {
        if (Type != MusicLibraryType.Playlist || MusicFiles.Count == 0 || indexes.Count == 0)
        {
            return false;
        }
        var paths = _playlist!.FilePaths;
        foreach (var index in indexes)
        {
            try
            {
                paths.Remove(MusicFiles[index].Path);
            }
            catch { }
        }
        _playlist.FilePaths = paths;
        return true;
    }
}