using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of a music folder
/// </summary>
public class MusicFolder : IDisposable
{
    private bool _disposed;
    
    /// <summary>
    /// The parent path of the music folder
    /// </summary>
    public string ParentPath { get; init; }
    /// <summary>
    /// Whether or not to include subfolders in scanning for music
    /// </summary>
    public bool IncludeSubfolders { get; set; }
    /// <summary>
    /// What to sort files in a music folder by
    /// </summary>
    public SortBy SortFilesBy { get; set; }
    /// <summary>
    /// The list of MusicFile objects from the folder
    /// </summary>
    public List<MusicFile> MusicFiles { get; init; }
    /// <summary>
    /// The list of paths of corrupted music files
    /// </summary>
    public List<string> CorruptedFiles { get; init; }
    /// <summary>
    /// Whether or not the folder contains music files that are read-only
    /// </summary>
    public bool ContainsReadOnlyFiles { get; private set; }
    /// <summary>
    /// A list of genres in the folder
    /// </summary>
    public List<string> Genres { get; init;  }
    
    /// <summary>
    /// Occurs when the loading progress is updated
    /// </summary>
    public event EventHandler<(int Value, int MaxValue, string Message)>? LoadingProgressUpdated;

    /// <summary>
    /// Constructs a MusicFolder
    /// </summary>
    /// <param name="path">The path of the music folder</param>
    public MusicFolder(string path)
    {
        _disposed = false;
        ParentPath = path;
        IncludeSubfolders = true;
        SortFilesBy = SortBy.Filename;
        MusicFiles = new List<MusicFile>();
        CorruptedFiles = new List<string>();
        ContainsReadOnlyFiles = false;
        Genres = new List<string>();
    }
    
    /// <summary>
    /// Finalizes the MusicFolder
    /// </summary>
    ~MusicFolder() => Dispose(false);
    
    /// <summary>
    /// Frees resources used by the MusicFolder object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MusicFolder object
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
    /// Scans the music folder for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
    /// </summary>
    /// <returns>Whether or not there are corrupted files</returns>
    public async Task<bool> ReloadMusicFilesAsync()
    {
        MusicFile.SortFilesBy = SortFilesBy;
        foreach (var file in MusicFiles)
        {
            file.Dispose();
        }
        MusicFiles.Clear();
        CorruptedFiles.Clear();
        ContainsReadOnlyFiles = false;
        Genres.Clear();
        if(Directory.Exists(ParentPath))
        {
            var supportedExtensions = new string[] { ".mp3", ".m4a", ".m4b", ".ogg", ".opus", ".oga", ".flac", ".wma", ".wav",
                ".aac", ".aax", ".aa", ".aif", ".aiff", ".aifc", ".dsd", ".dsf", ".ac3", ".gym", ".ape", ".mpv", ".mp+", ".ofr", ".ofs",
                ".psf", ".psf1", ".psf2", ".minipsf", ".minipsf1", ".minipsf2", ".ssf", ".minissf", ".minidsf", ".gsf", ".minigsf", ".qsf",
                ".miniqsf", ".spc", ".tak", ".tta", ".vqf", ".bwav", ".bwf", ".vgm", ".vgz", ".wv", ".asf" };
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(ParentPath, "*.*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(x => supportedExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();
                var i = 0;
                foreach(var path in files)
                {
                    try
                    {
                        var musicFile = new MusicFile(path);
                        MusicFiles.Add(musicFile);
                        if(musicFile.IsReadOnly)
                        {
                            ContainsReadOnlyFiles = true;
                        }
                        if (!Genres.Contains(musicFile.Genre))
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
        }
        return CorruptedFiles.Count > 0;
    }
}