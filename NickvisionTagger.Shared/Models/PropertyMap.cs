using System;
using System.Collections.Generic;
using System.Linq;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// A model of all properties of a music file
/// </summary>
public class PropertyMap : IEquatable<PropertyMap>
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

    /// <summary>
    /// Check equality of this PropertyMap with another one
    /// </summary>
    /// <param name="obj">object? to compare</param>
    /// <returns>True if PropertyMaps are equal, else false</returns>
    public override bool Equals(object? obj)
    {
        if(obj is PropertyMap map)
        {
            return Equals(map);
        }
        return false;
    }

    /// <summary>
    /// Check equality of this PropertyMap with another one
    /// </summary>
    /// <param name="obj">PropertyMap to compare</param>
    /// <returns>True if PropertyMaps are equal, else false</returns>
    public bool Equals(PropertyMap? obj)
    {
        return obj != null && obj.Filename == Filename &&
            obj.Title == Title &&
            obj.Artist == Artist &&
            obj.Album == Album &&
            obj.Year == Year &&
            obj.Track == Track &&
            obj.TrackTotal == TrackTotal &&
            obj.AlbumArtist == AlbumArtist &&
            obj.Genre == Genre &&
            obj.Comment == Comment &&
            obj.BeatsPerMinute == BeatsPerMinute &&
            obj.Composer == Composer &&
            obj.Description == Description &&
            obj.Publisher == Publisher &&
            obj.CustomProperties.Count == CustomProperties.Count && !obj.CustomProperties.Except(CustomProperties).Any();
    }

    /// <summary>
    /// Gets the hash code for the object
    /// </summary>
    /// <returns>The object's hash code</returns>
    public override int GetHashCode() => ToString().GetHashCode();

    /// <summary>
    /// Compares two PropertyMap via ==
    /// </summary>
    /// <param name="obj1">PropertyMap</param>
    /// <param name="obj2">PropertyMap</param>
    /// <returns>True if obj1 == obj2, else false</returns>
    public static bool operator ==(PropertyMap? obj1, PropertyMap? obj2)
    {
        if(obj1 is null || obj2 is null)
        {
            return obj1 is null && obj2 is null;
        }
        return obj1.Equals(obj2);
    }

    /// <summary>
    /// Compares two PropertyMap via !=
    /// </summary>
    /// <param name="obj1">PropertyMap</param>
    /// <param name="obj2">PropertyMap</param>
    /// <returns>True if obj1 != obj2, else false</returns>
    public static bool operator !=(PropertyMap? obj1, PropertyMap? obj2) => !(obj1 == obj2);
}