using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of a music folder
/// </summary>
public class MusicFolder
{
    /// <summary>
    /// The parent path of the music folder
    /// </summary>
    public string ParentPath { get; init; }
    /// <summary>
    /// Whether or not to include subfolders in scanning for music
    /// </summary>
    public bool IncludeSubfolders { get; set; }
    /// <summary>
    /// The list of MusicFile objects from the folder
    /// </summary>
    public List<MusicFile> MusicFiles { get; init; }
    
    /// <summary>
    /// Constructs a MusicFolder
    /// </summary>
    /// <param name="path">The path of the music folder</param>
    public MusicFolder(string path)
    {
        ParentPath = path;
        IncludeSubfolders = true;
        MusicFiles = new List<MusicFile>();
    }
    
    /// <summary>
    /// Scans the music folder for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
    /// </summary>
    public async Task ReloadMusicFilesAsync()
    {
        MusicFiles.Clear();
        if(Directory.Exists(ParentPath))
        {
            var supportedExtensions = new string[] { ".mp3", ".m4a", ".m4b", ".ogg", ".opus", ".oga", ".flac", ".wma", ".wav" };
            await Task.Run(() =>
            {
                foreach(var file in Directory.EnumerateFiles(ParentPath, "*.*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if(supportedExtensions.Any(x => Path.GetExtension(file).ToLower() == x))
                    {
                        MusicFiles.Add(new MusicFile(file));
                    }
                }
                MusicFiles.Sort();
            });
        }
    }
}