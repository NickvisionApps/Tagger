using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using System;
using System.Threading.Tasks;
using static Nickvision.GirExt.GtkExt;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Adw.ComboRow _themeRow;
    [Gtk.Connect] private readonly Gtk.Switch _rememberLastOpenedFolderSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _includeSubfoldersSwitch;
    [Gtk.Connect] private readonly Adw.ComboRow _sortFilesRow;
    [Gtk.Connect] private readonly Gtk.Switch _preserveModificationTimestampSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteTagWithMusicBrainzSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteAlbumArtWithMusicBrainzSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _overwriteLyricsWithWebSwitch;
    [Gtk.Connect] private readonly Adw.EntryRow _acoustIdRow;
    [Gtk.Connect] private readonly Gtk.Button _acoustIdButton;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        SetTransientFor(parent);
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        _themeRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                OnThemeChanged();
            }
        };
        _acoustIdButton.OnClicked += async (sender, e) => await LaunchNewAcoustIdPage();
        OnHide += Hide;
        //Load Config
        _themeRow.SetSelected((uint)_controller.Theme);
        _rememberLastOpenedFolderSwitch.SetActive(_controller.RememberLastOpenedFolder);
        _includeSubfoldersSwitch.SetActive(_controller.IncludeSubfolders);
        _sortFilesRow.SetSelected((uint)_controller.SortFilesBy);
        _preserveModificationTimestampSwitch.SetActive(_controller.PreserveModificationTimestamp);
        _overwriteTagWithMusicBrainzSwitch.SetActive(_controller.OverwriteTagWithMusicBrainz);
        _overwriteAlbumArtWithMusicBrainzSwitch.SetActive(_controller.OverwriteAlbumArtWithMusicBrainz);
        _overwriteLyricsWithWebSwitch.SetActive(_controller.OverwriteLyricsWithWebService);
        _acoustIdRow.SetText(_controller.AcoustIdUserAPIKey);
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : this(Builder.FromFile("preferences_dialog.ui"), controller, application, parent)
    {
    }

    /// <summary>
    /// Occurs when the dialog is hidden
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private void Hide(Gtk.Widget sender, EventArgs e)
    {
        _controller.RememberLastOpenedFolder = _rememberLastOpenedFolderSwitch.GetActive();
        _controller.IncludeSubfolders = _includeSubfoldersSwitch.GetActive();
        _controller.SortFilesBy = (SortBy)_sortFilesRow.GetSelected();
        _controller.PreserveModificationTimestamp = _preserveModificationTimestampSwitch.GetActive();
        _controller.OverwriteTagWithMusicBrainz = _overwriteTagWithMusicBrainzSwitch.GetActive();
        _controller.OverwriteAlbumArtWithMusicBrainz = _overwriteAlbumArtWithMusicBrainzSwitch.GetActive();
        _controller.OverwriteLyricsWithWebService = _overwriteLyricsWithWebSwitch.GetActive();
        _controller.AcoustIdUserAPIKey = _acoustIdRow.GetText();
        _controller.SaveConfiguration();
        Destroy();
    }

    /// <summary>
    /// Occurs when the theme selection is changed
    /// </summary>
    private void OnThemeChanged()
    {
        _controller.Theme = (Theme)_themeRow.GetSelected();
        _application.StyleManager!.ColorScheme = _controller.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
    }

    /// <summary>
    /// Occurs when the AcoustId Get New API Key button is clicked
    /// </summary>
    private async Task LaunchNewAcoustIdPage()
    {
        var uriLauncher = Gtk.UriLauncher.New(_controller.AcoustIdUserAPIKeyLink);
        try
        {
            await uriLauncher.LaunchAsync(this);
        }
        catch { }
    }
}
