using ATL;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

public enum LyricProviders
{
    Music163,
    LetrasMus,
    LyricsWikia,
    ApiSeeds
}

/// <summary>
/// A service for looking up and downloading lyrics for a music file
/// </summary>
public static class LyricService
{
    private static readonly HttpClient _http;

    /// <summary>
    /// Constructs a static LyricService
    /// </summary>
    static LyricService()
    {
        _http = new HttpClient();
    }

    /// <summary>
    /// Gets a LyricsInfo object for a song
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    public static async Task<LyricsInfo?> GetAsync(string title, string artist)
    {
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
        {
            return null;
        }
        title = title.Normalize(NormalizationForm.FormKD);
        artist = artist.Normalize(NormalizationForm.FormKD);
        LyricsInfo? res = null;
        foreach (var provider in Enum.GetValues<LyricProviders>())
        {
            res = provider switch
            {
                LyricProviders.Music163 => await GetFromMusic163Async(title, artist),
                LyricProviders.LetrasMus => await GetFromLetrasMusAsync(title, artist),
                LyricProviders.LyricsWikia => await GetFromLyricsWikiaAsync(title, artist),
                LyricProviders.ApiSeeds => await GetFromApiSeedsAsync(title, artist),
                _ => null
            };
            if (res != null)
            {
                break;
            }
        }
        return res;
    }

    /// <summary>
    /// Gets lyrics from Music163
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    private static async Task<LyricsInfo?> GetFromMusic163Async(string title, string artist)
    {
        return null;
    }
    
    /// <summary>
    /// Gets lyrics from LetrasMus
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    private static async Task<LyricsInfo?> GetFromLetrasMusAsync(string title, string artist)
    {
        return null;
    }
    
    /// <summary>
    /// Gets lyrics from LyricsWikia
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    private static async Task<LyricsInfo?> GetFromLyricsWikiaAsync(string title, string artist)
    {
        return null;
    }
    
    /// <summary>
    /// Gets lyrics from ApiSeeds
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    private static async Task<LyricsInfo?> GetFromApiSeedsAsync(string title, string artist)
    {
        return null;
    }
}