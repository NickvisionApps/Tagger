using ATL;
using NickvisionTagger.Shared.Events;
using System;
using System.IO;
using System.Linq;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// Types of a Lyrics
/// </summary>
public enum LyricsType
{
    Unsynchronized = 0,
    Synchronized
}

/// <summary>
/// A controller for a LyricsDialog
/// </summary>
public class LyricsDialogController
{
    private readonly LyricsInfo _lyrics;
    private LyricsType _type;

    /// <summary>
    /// The Lyrics represented by the controller
    /// </summary>
    public LyricsInfo Lyrics => new LyricsInfo(_lyrics);

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
            _lyrics = new LyricsInfo();
            _type = LyricsType.Unsynchronized;
        }
        else
        {
            _lyrics = new LyricsInfo(lyrics);
            if (_lyrics.SynchronizedLyrics.Count > 0)
            {
                _type = LyricsType.Synchronized;
                if (!_lyrics.Metadata.ContainsKey("offset"))
                {
                    _lyrics.Metadata["offset"] = "0";
                }
                _lyrics.UnsynchronizedLyrics = "";
            }
            else
            {
                _type = LyricsType.Unsynchronized;
            }
        }
    }

    /// <summary>
    /// The type of lyrics stored
    /// </summary>
    public LyricsType LyricsType
    {
        get => _type;

        set
        {
            if (_type != value)
            {
                _type = value;
                _lyrics.LanguageCode = "";
                _lyrics.Description = "";
                if (_type == LyricsType.Synchronized)
                {
                    _lyrics.UnsynchronizedLyrics = "";
                    if (!_lyrics.Metadata.ContainsKey("offset"))
                    {
                        _lyrics.Metadata["offset"] = "0";
                    }
                }
                else
                {
                    _lyrics.SynchronizedLyrics.Clear();
                }
            }
        }
    }

    /// <summary>
    /// The language code of the lyrics
    /// </summary>
    public string LanguageCode
    {
        get => _lyrics.LanguageCode;

        set => _lyrics.LanguageCode = value;
    }

    /// <summary>
    /// The description of the lyrics
    /// </summary>
    public string Description
    {
        get => _lyrics.Description;

        set => _lyrics.Description = value;
    }

    /// <summary>
    /// The unsyncrhonized lyrics
    /// </summary>
    /// <remarks>If the LyricsType is not Unsynchronized, null will be returned</remarks>
    public string? UnsynchronizedLyrics
    {
        get
        {
            if (_type != LyricsType.Unsynchronized)
            {
                return null;
            }
            return _lyrics.UnsynchronizedLyrics ?? "";
        }

        set
        {
            if (_type == LyricsType.Unsynchronized)
            {
                _lyrics.UnsynchronizedLyrics = value;
            }
        }
    }

    /// <summary>
    /// The number of synchronized lyrics
    /// </summary>
    /// <remarks>If the LyricsType is not Synchronized, null will be returned</remarks>
    public int? SynchronizedLyricsCount
    {
        get
        {
            if (_type != LyricsType.Synchronized)
            {
                return null;
            }
            return _lyrics.SynchronizedLyrics.Count;
        }
    }

    /// <summary>
    /// The offset of the synchronized lyrics
    /// </summary>
    /// <remarks>If the LyricsType is not Synchronized, null will be returned</remarks>
    public int? SynchronizedLyricsOffset
    {
        get
        {
            if (_type != LyricsType.Synchronized)
            {
                return null;
            }
            return int.TryParse(_lyrics.Metadata["offset"], out var offsetInt) ? offsetInt : 0;
        }

        set
        {
            if (_type == LyricsType.Synchronized)
            {
                _lyrics.Metadata["offset"] = value.ToString();
            }
        }
    }

    /// <summary>
    /// Starts the dialog
    /// </summary>
    public void Startup()
    {
        if (_type == LyricsType.Synchronized)
        {
            foreach (var phrase in _lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs))
            {
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
            }
        }
    }
    
    /// <summary>
    /// Adds a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    /// <returns>True if successful, else false</returns>
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool AddSynchronizedLyric(int timestamp)
    {
        if (_type != LyricsType.Synchronized)
        {
            return false;
        }
        if (!_lyrics.SynchronizedLyrics.Any(x => x.TimestampMs == timestamp))
        {
            var phrase = new LyricsInfo.LyricsPhrase(timestamp, "");
            _lyrics.SynchronizedLyrics.Add(phrase);
            SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, "", _lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    /// <param name="lyric">The string lyrics</param>
    /// <returns>True if successful, else false</returns>
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool SetSynchronizedLyric(int timestamp, string lyric)
    {
        if (_type != LyricsType.Synchronized)
        {
            return false;
        }
        var phrase = _lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == timestamp);
        if (phrase != null)
        {
            _lyrics.SynchronizedLyrics[_lyrics.SynchronizedLyrics.IndexOf(phrase)] = new LyricsInfo.LyricsPhrase(phrase.TimestampMs, lyric);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a synchronized lyric
    /// </summary>
    /// <param name="timestamp">The timestamp of the lyric in milliseconds</param>
    /// <returns>True if successful, else false</returns>
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool RemoveSynchronizedLyric(int timestamp)
    {
        if (_type != LyricsType.Synchronized)
        {
            return false;
        }
        var phrase = _lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == timestamp);
        if (phrase != null)
        {
            _lyrics.SynchronizedLyrics.Remove(phrase);
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(timestamp, phrase.Text));
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Clears all synchronized lyrics
    /// </summary>
    /// <returns>True if successful, else false</returns>
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool ClearSynchronizedLyrics()
    {
        if (_type != LyricsType.Synchronized)
        {
            return false;
        }
        foreach (var phrase in _lyrics.SynchronizedLyrics)
        {
            SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
        }
        _lyrics.SynchronizedLyrics.Clear();
        return true;
    }

    /// <summary>
    /// Imports synchronized lyric data from an LRC file
    /// </summary>
    /// <param name="path">The path of the LRC file</param>
    /// <param name="overwrite">Whether or not to overwrite Tagger's data with LRC data</param>
    /// <returns>True if successful, else false</returns>
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool ImportFromLRC(string path, bool overwrite)
    {
        if (_type != LyricsType.Synchronized || string.IsNullOrEmpty(path) || Path.GetExtension(path).ToLower() != ".lrc")
        {
            return false;
        }
        var lrc = new LyricsInfo();
        lrc.ParseLRC(File.ReadAllText(path));
        foreach (var p in lrc.SynchronizedLyrics)
        {
            var phrase = _lyrics.SynchronizedLyrics.FirstOrDefault(x => x.TimestampMs == p.TimestampMs);
            if (phrase != null && overwrite)
            {
                SynchronizedLyricRemoved?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text));
                _lyrics.SynchronizedLyrics[_lyrics.SynchronizedLyrics.IndexOf(phrase)] = new LyricsInfo.LyricsPhrase(phrase.TimestampMs, p.Text);
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text, _lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
            }
            else if (phrase == null)
            {
                phrase = new LyricsInfo.LyricsPhrase(p.TimestampMs, p.Text);
                _lyrics.SynchronizedLyrics.Add(phrase);
                SynchronizedLyricCreated?.Invoke(this, new SynchronizedLyricsEventArgs(phrase.TimestampMs, phrase.Text, _lyrics.SynchronizedLyrics.OrderBy(x => x.TimestampMs).ToList().IndexOf(phrase)));
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
    /// <remarks>If the LyricsType is not Synchronized, false will be returned</remarks>
    public bool ExportToLRC(string path)
    {
        if (_type != LyricsType.Synchronized || string.IsNullOrEmpty(path) || _lyrics.SynchronizedLyrics.Count == 0)
        {
            return false;
        }
        if (Path.GetExtension(path).ToLower() != ".lrc")
        {
            path += ".lrc";
        }
        File.WriteAllText(path, _lyrics.FormatSynchToLRC());
        return true;
    }
}