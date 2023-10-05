using NickvisionTagger.GNOME.Helpers;
using System;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// Music File Row
/// </summary>
public class MusicRow: Adw.ActionRow
{
    [Gtk.Connect] private readonly Gtk.Image _artImage;
    [Gtk.Connect] private readonly Gtk.Image _unsavedImage;
    
    private MusicRow(Gtk.Builder builder) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
    }
    
    /// <summary>
    /// Constructs MusicRow
    /// </summary>
    public MusicRow() : this(Builder.FromFile("music_row.ui"))
    {
    }
    
    /// <summary>
    /// Sets art icon from bytes array
    /// </summary>
    /// <param name="art">Art as byte array</param>
    public void SetArtFromBytes(byte[] art)
    {
        if (art.Length > 0)
        {
            _artImage.AddCssClass("list-icon");
            using var bytes = GLib.Bytes.From(art.AsSpan());
            using var texture = Gdk.Texture.NewFromBytes(bytes);
            _artImage.SetFromPaintable(texture);
        }
        else
        {
            _artImage.RemoveCssClass("list-icon");
            _artImage.SetFromIconName("audio-x-generic-symbolic");
        }
    }

    /// <summary>
    /// Sets art icon from file
    /// </summary>
    /// <param name="path">File path</param>
    public void SetArtFromFile(string path) => _artImage.SetFromFile(path);
    
    /// <summary>
    /// Sets unsaved icon visibility
    /// </summary>
    /// <param name="unsaved">Icon visibility</param>
    public void SetUnsaved(bool unsaved) => _unsavedImage.SetVisible(unsaved);
}