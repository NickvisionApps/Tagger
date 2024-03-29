using ATL;
using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NickvisionTagger.Shared.Models;

public enum LyricProviders
{
    Letras //unsync lyric provider
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
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
        //Remove accents. From: https://stackoverflow.com/a/2086575
        title = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-8").GetBytes(title));
        artist = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-8").GetBytes(artist));
        LyricsInfo? res = null;
        foreach (var provider in Enum.GetValues<LyricProviders>())
        {
            res = provider switch
            {
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
                UnsynchronizedLyrics = HttpUtility.HtmlDecode(lyricsHtml.InnerHtml.Replace("<p>", "").Replace("<br>", "\n").Replace("</p>", "\n")).Trim()
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