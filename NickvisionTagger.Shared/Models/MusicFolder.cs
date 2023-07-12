using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using File = System.IO.File;

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
    /// Constructs a MusicFolder
    /// </summary>
    /// <param name="path">The path of the music folder</param>
    public MusicFolder(string path)
    {
        ParentPath = path;
        IncludeSubfolders = true;
        SortFilesBy = SortBy.Filename;
        MusicFiles = new List<MusicFile>();
        CorruptedFiles = new List<string>();
    }
    
    /// <summary>
    /// Scans the music folder for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
    /// </summary>
    /// <returns>Whether or not there are corrupted files</returns>
    public async Task<bool> ReloadMusicFilesAsync()
    {
        MusicFiles.Clear();
        CorruptedFiles.Clear();
        MusicFile.SortFilesBy = SortFilesBy;
        if(Directory.Exists(ParentPath))
        {
            var supportedExtensions = new string[] { ".mp3", ".m4a", ".m4b", ".ogg", ".opus", ".oga", ".flac", ".wma", ".wav" };
            await Task.Run(() =>
            {
                foreach(var path in Directory.EnumerateFiles(ParentPath, "*.*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if(supportedExtensions.Any(x => Path.GetExtension(path).ToLower() == x))
                    {
                        try
                        {
                            MusicFiles.Add(new MusicFile(path));
                        }
                        catch (FileLoadException e)
                        {
                            CorruptedFiles.Add(path);
                            Console.WriteLine(e);
                        }
                    }
                }
                MusicFiles.Sort();
            });
        }
        return CorruptedFiles.Count > 1;
    }

    /// <summary>
    /// Clears the corrupted tags of affected music files
    /// </summary>
    public async Task ClearCorruptedTagsAsync()
    {
        await Task.Run(() =>
        {
            foreach(var corrupted in CorruptedFiles)
            {
                if(File.Exists(corrupted))
                {
                    using var file = TagLib.File.Create(corrupted);
                    file.RemoveTags(TagTypes.AllTags);
                }
            }
        });
        CorruptedFiles.Clear();
    }
}