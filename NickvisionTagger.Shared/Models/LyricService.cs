using ATL;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionTagger.Shared.Models;

public enum LyricProviders
{
    Music163 = 0, //sync lyric provider
    Letras, //unsync lyric provider
}

/// <summary>
/// A service for looking up and downloading lyrics for a music file
/// </summary>
/// <remarks>Service based off that of GiveMeLyrics: https://github.com/muriloventuroso/givemelyrics/blob/master/src/Services/LyricsFetcher.vala</remarks>
public static class LyricService
{
    private static readonly HttpClient _http;

    /// <summary>
    /// Constructs a static LyricService
    /// </summary>
    static LyricService()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
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

        title = title.ToLower().Replace("ê", "e").Replace("á", "á").Replace("à", "à").Replace("ã", "a").Replace("ó", "o")
            .Replace("ç", "c").Replace("í", "i").Replace("ú", "u").Replace("å", "a").Replace("ö", "o");
        artist = artist.ToLower().Replace("ê", "e").Replace("á", "á").Replace("à", "à").Replace("ã", "a").Replace("ó", "o")
            .Replace("ç", "c").Replace("í", "i").Replace("ú", "u").Replace("å", "a").Replace("ö", "o");
        LyricsInfo? res = null;
        foreach (var provider in Enum.GetValues<LyricProviders>())
        {
            res = provider switch
            {
                LyricProviders.Music163 => await GetFromMusic163Async(title, artist),
                LyricProviders.Letras => await GetFromLetrasAsync(title, artist),
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
        var url = $"http://music.163.com/api/search/pc?offset=0&limit=1&type=1&s={title.Replace(" ", "")},{artist.Replace(" ", "")}";
        try
        {
            var searchResult = (await _http.GetStringAsync(url)).ToLower();
            using var searchJson = JsonDocument.Parse(searchResult);
            if (searchJson.RootElement.GetProperty("code").GetInt32() == 200)
            {
                if (searchJson.RootElement.GetProperty("result").GetProperty("songs").GetArrayLength() > 0)
                {
                    var first = searchJson.RootElement.GetProperty("result").GetProperty("songs").EnumerateArray().Current;
                    var lyricUrl = $"https://music.163.com/api/song/lyric?os=pc&lv=-1&kv=-1&tv=-1&id={first.GetProperty("id").GetInt32()}";
                    var lyricResult = (await _http.GetStringAsync(lyricUrl)).ToLower();
                    using var lyricJson = JsonDocument.Parse(lyricResult);
                    if (lyricJson.RootElement.GetProperty("code").GetInt32() == 200)
                    {
                        var lrc = lyricJson.RootElement.GetProperty("lrc").GetProperty("lyric").GetString() ?? "";
                        var lyricsInfo = new LyricsInfo();
                        lyricsInfo.ParseLRC(lrc);
                        return lyricsInfo;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        return null;
    }
    
    /// <summary>
    /// Gets lyrics from Letras
    /// </summary>
    /// <param name="title">The title of the song</param>
    /// <param name="artist">The artist of the song</param>
    /// <returns>The LyricInfo object if successful, else null</returns>
    private static async Task<LyricsInfo?> GetFromLetrasAsync(string title, string artist)
    {
        var url = $"https://letras.mus.br/winamp.php?t={artist.Replace(" ", "-").Replace("&apos;", "-").Replace("&amp;", "e")}-{title.Replace(" ", "-")}/";
        try
        {
            var searchResult = (await _http.GetStringAsync(url)).ToLower();
            var html = new HtmlDocument();
            html.LoadHtml(searchResult);
            var lyricsHtml = html.GetElementbyId("letra-cnt");
            return new LyricsInfo()
            {
                UnsynchronizedLyrics = lyricsHtml.InnerHtml.Replace("<p>", "").Replace("<br>", "\n").Replace("</p>", "\n").Trim()
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        return null;
    }
}