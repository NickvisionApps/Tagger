using System.Collections.Generic;
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
    /// The number of total tracks of the file
    /// </summary>
    public string TrackTotal { get; set; }
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
    /// The BPM of the file
    /// </summary>
    public string BeatsPerMinute { get; set; }
    /// <summary>
    /// The composer of the file
    /// </summary>
    public string Composer { get; set; }
    /// <summary>
    /// The description of the file
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The publisher of the file
    /// </summary>
    public string Publisher { get; set; }
    /// <summary>
    /// The front album art of the file
    /// </summary>
    public string FrontAlbumArt { get; set; }
    /// <summary>
    /// The back album art of the file
    /// </summary>
    public string BackAlbumArt { get; set; }
    /// <summary>
    /// The custom properties of the file
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; init; }
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
        CustomProperties = new Dictionary<string, string>();
        Clear();
    }
    
    /// <summary>
    /// Resets the PropertyMap to default values
    /// </summary>
    public void Clear()
    {
        Filename = "";
        Title = "";
        Artist = "";
        Album = "";
        Year = "";
        Track = "";
        TrackTotal = "";
        AlbumArtist = "";
        Genre = "";
        Comment = "";
        BeatsPerMinute = "";
        Composer = "";
        Description = "";
        Publisher = "";
        FrontAlbumArt = "";
        BackAlbumArt = "";
        CustomProperties.Clear();
        Duration = "00:00:00";
        Fingerprint = "";
        FileSize = _("0 MiB");
    }
    
    /// <summary>
    /// Gets a string representation of the PropertyMap
    /// </summary>
    /// <returns>The string representation of the PropertyMap</returns>
    public override string ToString()
    {
        var s = "===PropertyMap===\n";
        s += $"Filename: {Filename}\n";
        s += $"Title: {Title}\n";
        s += $"Artist: {Artist}\n";
        s += $"Album: {Album}\n";
        s += $"Year: {Year}\n";
        s += $"Track: {Track} / {TrackTotal}\n";
        s += $"AlbumArtist: {AlbumArtist}\n";
        s += $"Genre: {Genre}\n";
        s += $"Comment: {Comment}\n";
        s += $"BeatsPerMinute: {BeatsPerMinute}\n";
        s += $"Composer: {Composer}\n";
        s += $"Description: {Description}\n";
        s += $"Publisher: {Publisher}\n";
        s += $"FrontAlbumArt: {FrontAlbumArt}\n";
        s += $"BackAlbumArt: {BackAlbumArt}\n";
        s += $"CustomPropertiesCount: {CustomProperties.Count}\n";
        s += $"Duration: {Duration}\n";
        s += $"Fingerprint: {Fingerprint}\n";
        s += $"FileSize: {FileSize}\n";
        s += "=========";
        return s;
    }
}