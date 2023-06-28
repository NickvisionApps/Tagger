using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of all properties of a music file
/// </summary>
public class PropertyMap
{
    /// <summary>
    /// The filename of the file
    /// </summary>
    public string Filename { get; set; }
    /// <summary>
    /// The title of the file
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The artist of the file
    /// </summary>
    public string Artist { get; set; }
    /// <summary>
    /// The album of the file
    /// </summary>
    public string Album { get; set; }
    /// <summary>
    /// The year of the file
    /// </summary>
    public string Year { get; set; }
    /// <summary>
    /// The track of the file
    /// </summary>
    public string Track { get; set; }
    /// <summary>
    /// The album artist of the file
    /// </summary>
    public string AlbumArtist { get; set; }
    /// <summary>
    /// The genre of the file
    /// </summary>
    public string Genre { get; set; }
    /// <summary>
    /// The comment of the file
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// The album art of the file
    /// </summary>
    public string AlbumArt { get; set; }
    /// <summary>
    /// The duration of the file
    /// </summary>
    public string Duration { get; set; }
    /// <summary>
    /// The fingerprint of the file
    /// </summary>
    public string Fingerprint { get; set; }
    /// <summary>
    /// The file size of the file
    /// </summary>
    public string FileSize { get; set; }
    
    /// <summary>
    /// Constructs a PropertyMap
    /// </summary>
    public PropertyMap()
    {
        Clear();
    }
    
    /// <summary>
    /// Resets the PropertyMap to default values
    /// <summary>
    public void Clear()
    {
        Filename = "";
        Title = "";
        Artist = "";
        Album = "";
        Year = "";
        Track = "";
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        AlbumArt = "";
        Duration = "00:00:00";
        Fingerprint = "";
        FileSize = _("0 MiB");
    }
    
    /// <summary>
    /// Gets a string representation of the PropertyMap
    /// <summary>
    /// <returns>The string representation of the PropertyMap</returns>
    public override string ToString()
    {
        var s = "===PropertyMap===\n";
        s += $"Filename: {Filename}\n";
        s += $"Title: {Title}\n";
        s += $"Artist: {Artist}\n";
        s += $"Album: {Album}\n";
        s += $"Year: {Year}\n";
        s += $"Track: {Track}\n";
        s += $"AlbumArtist: {AlbumArtist}\n";
        s += $"Genre: {Genre}\n";
        s += $"Comment: {Comment}\n";
        s += $"AlbumArt: {AlbumArt}\n";
        s += $"Duration: {Duration}\n";
        s += $"Fingerprint: {Fingerprint}\n";
        s += $"FileSize: {FileSize}\n";
        s += "=========";
        return s;
    }
}