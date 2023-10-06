using NickvisionTagger.Shared.Models;
using NickvisionTagger.GNOME.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// Music File Row
/// </summary>
public class MusicFileRow : Adw.ActionRow
{
    private byte[] _art;

    [Gtk.Connect] private readonly Gtk.Image _artImage;
    [Gtk.Connect] private readonly Gtk.Image _unsavedImage;
    
    /// <summary>
    /// Constructs MusicFileRow
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="musicFile">MusicFile</param>
    private MusicFileRow(Gtk.Builder builder, MusicFile musicFile) : base(builder.GetPointer("_root"), false)
    {
        _art = Array.Empty<byte>();
        builder.Connect(this);
        if (!string.IsNullOrEmpty(musicFile.Title))
        {
            SetTitle($"{(musicFile.Track != 0 ? $"{musicFile.Track:D2} - " : "")}{Regex.Replace(musicFile.Title, "\\&", "&amp;")}");
            SetSubtitle(Regex.Replace(musicFile.Filename, "\\&", "&amp;"));
        }
        else
        {
            SetTitle(Regex.Replace(musicFile.Filename, "\\&", "&amp;"));
            SetSubtitle("");
        }
        SetArtFromBytes(musicFile.FrontAlbumArt);
    }
    
    /// <summary>
    /// Constructs MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public MusicFileRow(MusicFile musicFile) : this(Builder.FromFile("music__file_row.ui"), musicFile)
    {
    }
    
    /// <summary>
    /// Sets art icon from bytes array
    /// </summary>
    /// <param name="art">Art as byte array</param>
    public void SetArtFromBytes(byte[] art)
    {
        if(_art.SequenceEqual(art))
        {
            return;
        }
        _art = art;
        if (_art.Length > 0)
        {
            _artImage.AddCssClass("list-icon");
            using var bytes = GLib.Bytes.From(_art.AsSpan());
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
    /// Sets unsaved icon visibility
    /// </summary>
    /// <param name="unsaved">Icon visibility</param>
    public void SetUnsaved(bool unsaved) => _unsavedImage.SetVisible(unsaved);
}