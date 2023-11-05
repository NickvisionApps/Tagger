using Nickvision.Aura;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Types of corruption of a music file
/// </summary>
public enum CorruptionType
{
    InvalidTagData,
    InvalidAlubmArt
}

/// <summary>
/// A model of a corrupted music file
/// </summary>
public class CorruptedMusicFile
{
    /// <summary>
    /// The path of the corrupted music file
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// The type of corruption of the music file
    /// </summary>
    public CorruptionType CorruptionType { get; init; }

    /// <summary>
    /// Constructs a CorruptedMusicFile
    /// </summary>
    /// <param name="path">The path of the corrupted music file</param>
    /// <param name="corruptionType">The type of corruption of the music file</param>
    public CorruptedMusicFile(string path, CorruptionType corruptionType)
    {
        Path = path;
        CorruptionType = corruptionType;
    }

    /// <summary>
    /// Fixes the corruption of the music file
    /// </summary>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> FixAsync()
    {
        var fixedPath = $"{UserDirectories.ApplicationCache}{System.IO.Path.DirectorySeparatorChar}{Guid.NewGuid()}{System.IO.Path.GetExtension(Path)}";
        var command = CorruptionType switch
        {
            CorruptionType.InvalidTagData => $"-i \"{Path}\" \"{fixedPath}\"",
            CorruptionType.InvalidAlubmArt => $"-map 0:a -c:a copy -map_metadata -1 -i \"{Path}\" \"{fixedPath}\"",
            _ => ""
        };
        if (!string.IsNullOrEmpty(command))
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = DependencyLocator.Find("ffmpeg"),
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (File.Exists(fixedPath))
            {
                File.Move(fixedPath, Path, true);
                return true;
            }
            File.Delete(fixedPath);
        }
        return false;
    }
}
