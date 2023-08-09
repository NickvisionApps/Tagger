using System.Collections.Generic;

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
    /// Constructs a LyricsDialogController
    /// </summary>
    /// <param name="langCode">The language code of the lyrics</param>
    /// <param name="description">The description of the lyrics</param>
    /// <param name="unsync">The unsynchronized lyrics</param>
    /// <param name="sync">The set of synchronized lyrics</param>
    public LyricsDialogController(string langCode, string description, string unsync, Dictionary<int, string> sync)
    {
        LanguageCode = langCode;
        Description = description;
        UnsynchronizedLyrics = unsync;
        SynchronizedLyrics = sync;
    }
}