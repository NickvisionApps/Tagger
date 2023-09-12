using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the options in creating a playlist
/// </summary>
public partial class CreatePlaylistDialog : Adw.Window
{
    private bool _constructing;
    
    [Gtk.Connect] private readonly Adw.EntryRow _nameRow;
    [Gtk.Connect] private readonly Adw.ComboRow _formatRow;
    [Gtk.Connect] private readonly Gtk.Switch _selectedFilesOnlySwitch;
    [Gtk.Connect] private readonly Gtk.Button _createButton;
    
    /// <summary>
    /// Occurs when the create button is clicked
    /// </summary>
    public event EventHandler<PlaylistOptions>? OnCreate;
    
    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="folderName">The name of the folder, if available</param>
    private CreatePlaylistDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string? folderName) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        if (!string.IsNullOrEmpty(folderName))
        {
            _nameRow.SetText(folderName);
        }
        _nameRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _createButton.OnClicked += Create;
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="folderName">The name of the folder, if available</param>
    public CreatePlaylistDialog(Gtk.Window parent, string iconName, string? folderName) : this(Builder.FromFile("create_playlist_dialog.ui"), parent, iconName, folderName)
    {
        
    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var valid = !string.IsNullOrEmpty(_nameRow.GetText());
        _nameRow.RemoveCssClass("error");
        _nameRow.SetTitle(_("Name"));
        if (!valid)
        {
            _nameRow.AddCssClass("error");
            _nameRow.SetTitle(_("Name (Empty)"));
        }
        _createButton.SetSensitive(valid);
    }

    /// <summary>
    /// Occurs when the create button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Create(Gtk.Button sender, EventArgs e)
    {
        OnCreate?.Invoke(this, new PlaylistOptions(_nameRow.GetText(), (PlaylistFormat)_formatRow.GetSelected(), _selectedFilesOnlySwitch.GetActive()));
        Destroy();
    }
}