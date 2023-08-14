using System;

namespace NickvisionTagger.Shared.Events;

/// <summary>
/// Event args when working with synchronized lyrics
/// </summary>
public class SynchronizedLyricsEventArgs : EventArgs
{
    /// <summary>
    /// The timestamp of the sync lyric in milliseconds
    /// </summary>
    public int Timestamp { get; set; }
    /// <summary>
    /// The string lyric
    /// </summary>
    public string Lyric { get; set; }

    /// <summary>
    /// Constructs a SynchronizedLyricsEventArgs
    /// </summary>
    /// <param name="timestamp">The timestamp of the sync lyric in milliseconds</param>
    /// <param name="lyric">The string lyric</param>
    public SynchronizedLyricsEventArgs(int timestamp, string lyric)
    {
        Timestamp = timestamp;
        Lyric = lyric;
    }
}