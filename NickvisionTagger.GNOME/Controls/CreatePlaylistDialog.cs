using Nickvision.GirExt;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.IO;
using System.Linq;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the options in creating a playlist
/// </summary>
public partial class CreatePlaylistDialog : Adw.Window
{
    private bool _constructing;
    private string _path;

    [Gtk.Connect] private readonly Adw.EntryRow _pathRow;
    [Gtk.Connect] private readonly Gtk.Button _selectSaveLocationButton;
    [Gtk.Connect] private readonly Adw.ComboRow _formatRow;
    [Gtk.Connect] private readonly Gtk.Switch _relativePathsSwitch;
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
    private CreatePlaylistDialog(Gtk.Builder builder, Gtk.Window parent, string iconName) : base(builder.GetPointer("_root"), false)
    {
        _constructing = true;
        _path = "";
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _pathRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _selectSaveLocationButton.OnClicked += SelectSaveLocation;
        _createButton.OnClicked += Create;
        Validate();
        _constructing = false;
    }

    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    public CreatePlaylistDialog(Gtk.Window parent, string iconName) : this(Builder.FromFile("create_playlist_dialog.ui"), parent, iconName)
    {

    }

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        _pathRow.RemoveCssClass("error");
        _pathRow.SetTitle(_("Path"));
        var empty = string.IsNullOrEmpty(_pathRow.GetText());
        if (empty)
        {
            _pathRow.AddCssClass("error");
            _pathRow.SetTitle(_("Path (Empty)"));
        }
        _createButton.SetSensitive(!empty);
    }

    /// <summary>
    /// Occurs when the select save location button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void SelectSaveLocation(Gtk.Button sender, EventArgs e)
    {
        var playlistExtensions = Enum.GetValues<PlaylistFormat>().Select(x => x.GetDotExtension()).ToList();
        var fileDialog = Gtk.FileDialog.New();
        fileDialog.SetTitle(_("Save Playlist"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        Gtk.FileFilter? defaultFilter = null;
        foreach (var ext in playlistExtensions)
        {
            var filter = Gtk.FileFilter.New();
            filter.SetName(_("{0} Playlist (*{1})", ext.Replace(".", "").ToUpper(), ext));
            filter.AddPattern($"*{ext}");
            filter.AddPattern($"*{ext.ToUpper()}");
            filters.Append(filter);
            if (ext == playlistExtensions[(int)_formatRow.GetSelected()])
            {
                defaultFilter = filter;
            }
        }
        fileDialog.SetFilters(filters);
        fileDialog.SetDefaultFilter(defaultFilter);
        try
        {
            var file = await fileDialog.SaveAsync(this);
            _path = file.GetPath();
            var extIndex = playlistExtensions.IndexOf((Path.GetExtension(_path) ?? "").ToLower());
            if (extIndex != -1)
            {
                _formatRow.SetSelected((uint)extIndex);
            }
            _pathRow.SetText(Path.GetFileNameWithoutExtension(_path) ?? "");
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the create button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Create(Gtk.Button sender, EventArgs e)
    {
        OnCreate?.Invoke(this, new PlaylistOptions(_path, (PlaylistFormat)_formatRow.GetSelected(), _relativePathsSwitch.GetActive(), _selectedFilesOnlySwitch.GetActive()));
        Destroy();
    }
}