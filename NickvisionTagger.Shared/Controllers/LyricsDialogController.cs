using ATL;
using Microsoft.VisualBasic.CompilerServices;
using NickvisionTagger.Shared.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// A controller for a LyricsDialog
/// </summary>
public class LyricsDialogController
{
    /// <summary>
    /// The language code of the lyrics
    /// </summary>
    public string LanguageCode { get; set; }
    /// <summary>
    /// The description of the lyrics
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The unsynchronized lyrics
    /// </summary>
    public string UnsynchronizedLyrics { get; set; }
    /// <summary>
    /// The set of synchronized lyrics
    /// </summary>
    public Dictionary<int, string> SynchronizedLyrics { get; init; }
    /// <summary>
    /// The offset for SynchronizedLyrics (in milliseconds)
    /// </summary>
    public int SynchronizedLyricsOffset { get; set; }
    
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
    /// <param name="langCode">The language code of the lyrics</param>
    /// <param name="description">The description of the lyrics</param>
    /// <param name="unsync">The unsynchronized lyrics</param>
    /// <param name="sync">The set of synchronized lyrics</param>
    /// <param name="offset">The offset of synchronized lyrics (in milliseconds)</param>
    public LyricsDialogController(string langCode, string description, string unsync, Dictionary<int, string> sync, int offset)
    {
        LanguageCode = langCode;
        Description = description;
        UnsynchronizedLyrics = unsync;
        SynchronizedLyrics = sync.ToDictionary(x => x.Key, x => x.Value);
        SynchronizedLyricsOffset = offset;
    }

    public void Startup()
    {
        foreach (var pair in SynchronizedLyrics)
        {
            SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(pair.Key, pair.Value));
        }
    }
    
    public void AddSynchronizedLyric(int timestamp)
    {
        if (!SynchronizedLyrics.ContainsKey(timestamp))
        {
            SynchronizedLyrics[timestamp] = "";
            SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, ""));
        }
    }

    public void SetSynchronizedLyric(int timestamp, string lyric)
    {
        if (SynchronizedLyrics.ContainsKey(timestamp))
        {
            SynchronizedLyrics[timestamp] = lyric;
        }
    }

    public void RemoveSynchronizedLyric(int timestamp)
    {
        if (SynchronizedLyrics.ContainsKey(timestamp))
        {
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, SynchronizedLyrics[timestamp]));
            SynchronizedLyrics.Remove(timestamp);
        }
    }
    
    public void ClearSynchronizedLyrics()
    {
        foreach (var pair in SynchronizedLyrics)
        {
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(pair.Key, pair.Value));
        }
        SynchronizedLyrics.Clear();
    }

    public bool ImportFromLRC(string path, bool overwrite)
    {
        if (string.IsNullOrEmpty(path) || Path.GetExtension(path).ToLower() != ".lrc")
        {
            return false;
        }
        var lrc = File.ReadAllText(path);
        var lyricsInfo = new LyricsInfo();
        lyricsInfo.ParseLRC(lrc);
        foreach (var phase in lyricsInfo.SynchronizedLyrics)
        {
            if (SynchronizedLyrics.ContainsKey(phase.TimestampMs) && overwrite)
            {
                SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(phase.TimestampMs, SynchronizedLyrics[phase.TimestampMs]));
                SynchronizedLyrics[phase.TimestampMs] = phase.Text;
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phase.TimestampMs, phase.Text));
            }
            else if (!SynchronizedLyrics.ContainsKey(phase.TimestampMs))
            {
                SynchronizedLyrics.Add(phase.TimestampMs, phase.Text);
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phase.TimestampMs, phase.Text));
            }
        }
        var offset = lyricsInfo.Metadata.TryGetValue("offset", out var o) ? (int.TryParse(o, out var offsetInt) ? offsetInt : 0) : 0;
        if (offset != SynchronizedLyricsOffset && overwrite)
        {
            SynchronizedLyricsOffset = offset;
        }
        return false;
    }

    public bool ExportToLRC(string path)
    {
        if (string.IsNullOrEmpty(path) || SynchronizedLyrics.Count == 0)
        {
            return false;
        }
        if (Path.GetExtension(path).ToLower() != ".lrc")
        {
            path += ".lrc";
        }
        var lyricsInfo = new LyricsInfo()
        {
            SynchronizedLyrics = SynchronizedLyrics.Select(x => new LyricsInfo.LyricsPhrase(x.Key, x.Value)).ToList()
        };
        if (SynchronizedLyricsOffset != 0)
        {
            lyricsInfo.Metadata["offset"] = SynchronizedLyricsOffset.ToString();
        }
        File.WriteAllText(path, lyricsInfo.FormatSynchToLRC());
        return true;
    }
}