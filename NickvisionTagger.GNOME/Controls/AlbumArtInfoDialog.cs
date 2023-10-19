using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Models;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog for showing album art info
/// </summary>
public class AlbumArtInfoDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Adw.ActionRow _mimeTypeRow;
    [Gtk.Connect] private readonly Adw.ActionRow _widthRow;
    [Gtk.Connect] private readonly Adw.ActionRow _heightRow;

    /// <summary>
    /// Constructs a AlbumArtInfoDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="art">AlbumArt</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon</param>
    public AlbumArtInfoDialog(Gtk.Builder builder, AlbumArt art, Gtk.Window parent, string iconName) : base(builder.GetPointer("_root"), false)
    {
        //Build UI
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(iconName);
        //Load
        _titleLabel.SetLabel(art.Type == AlbumArtType.Front ? _("Front") : _("Back"));
        _mimeTypeRow.SetSubtitle(art.MimeType);
        _widthRow.SetSubtitle(_("{0} pixels", art.Width));
        _heightRow.SetSubtitle(_("{0} pixels", art.Width));
    }

    /// <summary>
    /// Constructs a AlbumArtInfoDialog
    /// </summary>
    /// <param name="art">AlbumArt</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon</param>
    public AlbumArtInfoDialog(AlbumArt art, Gtk.Window parent, string iconName) : this(Builder.FromFile("album_art_info_dialog.ui"), art, parent, iconName)
    {
    }
}
