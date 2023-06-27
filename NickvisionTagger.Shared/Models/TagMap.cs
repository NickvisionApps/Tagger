namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of all properties of a tag
/// </summary>
public class TagMap
{
    public string Filename { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public string Year { get; set; }
    public string Track { get; set; }
    public string AlbumArtist { get; set; }
    public string Genre { get; set; }
    public string Comment { get; set; }
    public string AlbumArt { get; set; }
    public string Duration { get; set; }
    public string Fingerprint { get; set; }
    public string FileSize { get; set; }
    
    /// <summary>
    /// Constructs a TagMap
    /// </summary>
    public TagMap()
    {
        
    }
}