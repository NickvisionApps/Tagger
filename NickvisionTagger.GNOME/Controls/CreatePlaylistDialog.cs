using NickvisionTagger.GNOME.Helpers;
using System;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the options in creating a playlist
/// </summary>
public partial class CreatePlaylistDialog : Adw.Window
{
    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Gtk.ComboBox _formatRow;
    [Gtk.Connect] private readonly Gtk.Switch _selectedFilesOnlySwitch;
    [Gtk.Connect] private readonly Gtk.Button _createButton;
    
    /// <summary>
    /// Occurs when the create button is clicked
    /// </summary>
    public event EventHandler<EventArgs>? OnCreate;
    
    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    private CreatePlaylistDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string folderName) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
    }

    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    public CreatePlaylistDialog(Gtk.Window parent, string iconName, string folderName) : this(Builder.FromFile("create_playlist_dialog.ui"), parent, iconName, folderName)
    {
    }
}