using ATL;
using NickvisionTagger.Shared.Events;
using System;
using System.IO;
using System.Linq;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// A controller for a LyricsDialog
/// </summary>
public class LyricsDialogController
{
    /// <summary>
    /// The LyricsInfo object for the lyrics
    /// </summary>
    public LyricsInfo Lyrics { get; init; }
    
    /// <summary>
    /// Occurs when a sync lyric is created
    /// </summary>
    public event EventHandler<SynchronizedLyricsEventArgs>? SynchronizedLyricCreated;
    /// <summary>
    /// Occurs when a sync lyric is removed
    /// </summary>
    public event EventHandler<SynchronizedLyricsEventArgs>? SynchronizedLyricRemoved;

    /// <summary>
    /// Constructs a LyricsDialogController
    /// </summary>
    /// <param name="lyrics">LyricsInfo?</param>
    public LyricsDialogController(LyricsInfo? lyrics)
    {
        if (lyrics == null)
        {
            Lyrics = new LyricsInfo();
            Lyrics.Metadata["offset"] = "0";
        }
        else
        {
            Lyrics = new LyricsInfo(lyrics);
        }
    }

    /// <summary>
    /// The offset for SynchronizedLyrics (in milliseconds)
    /// </summary>
    public int SynchronizedLyricsOffset
    {
        get => Lyrics.Metadata.TryGetValue("offset", out var o) ? (int.TryParse(o, out var offsetInt) ? offsetInt : 0) : 0;

        set => Lyrics.Metadata["offset"] = value.ToString();
    }

    /// <summary>
    /// Starts the dialog
    /// </summary>
    public void Startup()
    {
        foreach (var phrase in Lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs))
        {
            SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
        }
    }
    
    /// <summary>
    /// Adds a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    public void AddSynchronizedLyric(int timestamp)
    {
        if (!Lyrics.SynchronizedLyrics.Any(x => x.TimestampMs == timestamp))
        {
            var phrase = new LyricsInfo.LyricsPhrase(timestamp, "");
            Lyrics.SynchronizedLyrics.Add(phrase);
            SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, "", Lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
        }
    }

    /// <summary>
    /// Sets a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    /// <param name="lyric">The string lyrics</param>
    public void SetSynchronizedLyric(int timestamp, string lyric)
    {
        var phrase = Lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == timestamp);
        if (phrase != null)
        {
            Lyrics.SynchronizedLyrics[Lyrics.SynchronizedLyrics.IndexOf(phrase)] = new LyricsInfo.LyricsPhrase(phrase.TimestampMs, lyric);
        }
    }

    /// <summary>
    /// Removes a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    public void RemoveSynchronizedLyric(int timestamp)
    {
        var phrase = Lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == timestamp);
        if (phrase != null)
        {
            Lyrics.SynchronizedLyrics.Remove(phrase);
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, phrase.Text));
        }
    }
    
    /// <summary>
    /// Clears all synchronized lyrics
    /// </summary>
    public void ClearSynchronizedLyrics()
    {
        foreach (var phrase in Lyrics.SynchronizedLyrics)
        {
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
        }
        Lyrics.SynchronizedLyrics.Clear();
    }

    /// <summary>
    /// Imports synchronized lyric data from an LRC file
    /// </summary>
    /// <param name="path">The path of the LRC file</param>
    /// <param name="overwrite">Whether or not to overwrite Tagger's data with LRC data</param>
    /// <returns>True if successful, else false</returns>
    public bool ImportFromLRC(string path, bool overwrite)
    {
        if (string.IsNullOrEmpty(path) || Path.GetExtension(path).ToLower() != ".lrc")
        {
            return false;
        }
        var lrc = new LyricsInfo();
        lrc.ParseLRC(File.ReadAllText(path));
        foreach (var p in lrc.SynchronizedLyrics)
        {
            var phrase = Lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == p.TimestampMs);
            if (phrase != null && overwrite)
            {
                SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
                Lyrics.SynchronizedLyrics[Lyrics.SynchronizedLyrics.IndexOf(phrase)] = new LyricsInfo.LyricsPhrase(phrase.TimestampMs, p.Text);
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text, Lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
            }
            else if (phrase == null)
            {
                phrase = new LyricsInfo.LyricsPhrase(p.TimestampMs, p.Text);
                Lyrics.SynchronizedLyrics.Add(phrase);
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text, Lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
            }
        }
        var lrcOffset = lrc.Metadata.TryGetValue("offset", out var o) ? (int.TryParse(o, out var offsetInt) ? offsetInt : 0) : 0;
        if (lrcOffset != SynchronizedLyricsOffset && overwrite)
        {
            SynchronizedLyricsOffset = lrcOffset;
        }
        return true;
    }

    /// <summary>
    /// Exports synchronized lyric data to an LRC file
    /// </summary>
    /// <param name="path">The path of the LRC file</param>
    /// <returns>True if successful, else false</returns>
    public bool ExportToLRC(string path)
    {
        if (string.IsNullOrEmpty(path) || Lyrics.SynchronizedLyrics.Count == 0)
        {
            return false;
        }
        if (Path.GetExtension(path).ToLower() != ".lrc")
        {
            path += ".lrc";
        }
        File.WriteAllText(path, Lyrics.FormatSynchToLRC());
        return true;
    }
}