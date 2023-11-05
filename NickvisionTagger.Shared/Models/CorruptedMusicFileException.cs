using System;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// An exception for when a music file is corrupted
/// </summary>
public class CorruptedMusicFileException : Exception
{
    /// <summary>
    /// The type of corruption
    /// </summary>
    public CorruptionType CorruptionType { get; init; }

    /// <summary>
    /// Constructs a CorruptedMusicFileException
    /// </summary>
    /// <param name="corruptionType">The type of corruption</param>
    /// <param name="message">The message of the exception</param>
    public CorruptedMusicFileException(CorruptionType corruptionType, string message = "") : base(message)
    {
        CorruptionType = corruptionType;
    }
}
