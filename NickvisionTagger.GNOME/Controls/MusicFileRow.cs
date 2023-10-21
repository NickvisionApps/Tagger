using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Text.RegularExpressions;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// Music File Row
/// </summary>
public class MusicFileRow : Adw.ActionRow
{
    private AlbumArt _art;

    [Gtk.Connect] private readonly Gtk.Image _artImage;
    [Gtk.Connect] private readonly Gtk.Image _unsavedImage;

    /// <summary>
    /// Constructs MusicFileRow
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="musicFile">MusicFile</param>
    private MusicFileRow(Gtk.Builder builder, MusicFile musicFile) : base(builder.GetPointer("_root"), false)
    {
        _art = new AlbumArt(Array.Empty<byte>(), AlbumArtType.Front);
        builder.Connect(this);
        Update(musicFile);
    }

    /// <summary>
    /// Constructs MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public MusicFileRow(MusicFile musicFile) : this(Builder.FromFile("music_file_row.ui"), musicFile)
    {
    }

    /// <summary>
    /// Sets art icon from AlbmArt object
    /// </summary>
    /// <param name="art">AlbumArt</param>
    public void SetArtFromAlbumArt(AlbumArt art)
    {
        if (_art == art)
        {
            return;
        }
        _art = art;
        if (!_art.IsEmpty)
        {
            _artImage.AddCssClass("list-icon");
            using var bytes = GLib.Bytes.From(_art.Icon.AsSpan());
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

    /// <summary>
    /// Updates the MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public void Update(MusicFile musicFile)
    {
        if (!string.IsNullOrEmpty(musicFile.Title))
        {
            SetTitle($"{(musicFile.Track != 0 ? $"{musicFile.Track:D2} - " : "")}{musicFile.Title}");
            SetSubtitle(musicFile.Filename);
        }
        else
        {
            SetTitle(musicFile.Filename);
            SetSubtitle("");
        }
        SetArtFromAlbumArt(musicFile.FrontAlbumArt);
    }
}
