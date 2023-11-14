using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using System;
using System.Threading.Tasks;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Adw.ComboRow _themeRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _rememberLastOpenedFolderRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _includeSubfoldersRow;
    [Gtk.Connect] private readonly Adw.ComboRow _sortFilesRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _preserveModificationTimestampRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _limitCharactersRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _overwriteTagWithMusicBrainzRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _overwriteAlbumArtWithMusicBrainzRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _overwriteLyricsWithWebRow;
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
        _rememberLastOpenedFolderRow.SetActive(_controller.RememberLastOpenedFolder);
        _includeSubfoldersRow.SetActive(_controller.IncludeSubfolders);
        _sortFilesRow.SetSelected((uint)_controller.SortFilesBy);
        _preserveModificationTimestampRow.SetActive(_controller.PreserveModificationTimestamp);
        _limitCharactersRow.SetActive(_controller.LimitFilenameCharacters);
        _overwriteTagWithMusicBrainzRow.SetActive(_controller.OverwriteTagWithMusicBrainz);
        _overwriteAlbumArtWithMusicBrainzRow.SetActive(_controller.OverwriteAlbumArtWithMusicBrainz);
        _overwriteLyricsWithWebRow.SetActive(_controller.OverwriteLyricsWithWebService);
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
        _controller.RememberLastOpenedFolder = _rememberLastOpenedFolderRow.GetActive();
        _controller.IncludeSubfolders = _includeSubfoldersRow.GetActive();
        _controller.SortFilesBy = (SortBy)_sortFilesRow.GetSelected();
        _controller.PreserveModificationTimestamp = _preserveModificationTimestampRow.GetActive();
        _controller.LimitFilenameCharacters = _limitCharactersRow.GetActive();
        _controller.OverwriteTagWithMusicBrainz = _overwriteTagWithMusicBrainzRow.GetActive();
        _controller.OverwriteAlbumArtWithMusicBrainz = _overwriteAlbumArtWithMusicBrainzRow.GetActive();
        _controller.OverwriteLyricsWithWebService = _overwriteLyricsWithWebRow.GetActive();
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
