using Nickvision.Aura;
using NickvisionTagger.GNOME.Controls;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NickvisionTagger.Shared.Models;
using static NickvisionTagger.Shared.Helpers.Gettext;
using static Nickvision.GirExt.GtkExt;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow : Adw.ApplicationWindow
{
    private delegate void GtkListBoxUpdateHeaderFunc(nint row, nint before, nint data);

    [LibraryImport("libadwaita-1.so.0")]
    private static partial void gtk_list_box_set_header_func(nint box, GtkListBoxUpdateHeaderFunc updateHeader, nint data, nint destroy);
    [LibraryImport("libadwaita-1.so.0")]
    private static partial void gtk_list_box_row_set_header(nint row, nint header);


    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private readonly Gtk.DropTarget _dropTarget;
    private readonly Gio.SimpleAction _createPlaylistAction;
    private readonly Gio.SimpleAction _addToPlaylistAction;
    private readonly Gio.SimpleAction _removeFromPlaylistAction;
    private readonly Gio.SimpleAction _applyAction;
    private readonly Gio.SimpleAction _insertAlbumArtAction;
    private readonly Gio.SimpleAction _removeAlbumArtAction;
    private readonly Gio.SimpleAction _exportAlbumArtAction;
    private readonly Gio.SimpleAction _lyricsAction;
    private readonly Gio.SimpleAction _musicBrainzAction;
    private readonly Gio.SimpleAction _downloadLyricsAction;
    private readonly Gio.SimpleAction _acoustIdAction;
    private readonly GtkListBoxUpdateHeaderFunc _updateHeaderFunc;
    private AlbumArtType _currentAlbumArtType;
    private string _listHeader;
    private List<Adw.ActionRow> _listMusicFilesRows;
    private List<Adw.EntryRow> _customPropertyRows;
    private AutocompleteBox _autocompleteBox;
    private bool _isSelectionOccuring;

    [Gtk.Connect] private readonly Adw.HeaderBar _headerBar;
    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.Button _libraryButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _flapToggleButton;
    [Gtk.Connect] private readonly Gtk.Label _selectionLabel;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _tagActionsButton;
    [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Gtk.Label _loadingLabel;
    [Gtk.Connect] private readonly Gtk.ProgressBar _loadingProgressBar;
    [Gtk.Connect] private readonly Gtk.Label _loadingProgressLabel;
    [Gtk.Connect] private readonly Adw.Flap _libraryFlap;
    [Gtk.Connect] private readonly Adw.ViewStack _filesViewStack;
    [Gtk.Connect] private readonly Gtk.SearchEntry _musicFilesSearch;
    [Gtk.Connect] private readonly Gtk.Button _advancedSearchInfoButton;
    [Gtk.Connect] private readonly Gtk.Button _selectAllButton;
    [Gtk.Connect] private readonly Gtk.Separator _searchSeparator;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindowMusicFiles;
    [Gtk.Connect] private readonly Gtk.ListBox _listMusicFiles;
    [Gtk.Connect] private readonly Adw.ViewStack _selectedViewStack;
    [Gtk.Connect] private readonly Gtk.Label _artTypeLabel;
    [Gtk.Connect] private readonly Adw.ViewStack _artViewStack;
    [Gtk.Connect] private readonly Gtk.Button _insertAlbumArtButton;
    [Gtk.Connect] private readonly Gtk.Button _removeAlbumArtButton;
    [Gtk.Connect] private readonly Gtk.Button _exportAlbumArtButton;
    [Gtk.Connect] private readonly Gtk.Button _switchAlbumArtButton;
    [Gtk.Connect] private readonly Adw.ButtonContent _switchAlbumArtButtonContent;
    [Gtk.Connect] private readonly Gtk.Picture _albumArtImage;
    [Gtk.Connect] private readonly Gtk.Label _durationFileSizeLabel;
    [Gtk.Connect] private readonly Adw.EntryRow _filenameRow;
    [Gtk.Connect] private readonly Gtk.Overlay _mainPropOverlay;
    [Gtk.Connect] private readonly Adw.EntryRow _titleRow;
    [Gtk.Connect] private readonly Adw.EntryRow _artistRow;
    [Gtk.Connect] private readonly Adw.EntryRow _albumRow;
    [Gtk.Connect] private readonly Adw.EntryRow _yearRow;
    [Gtk.Connect] private readonly Adw.EntryRow _trackRow;
    [Gtk.Connect] private readonly Adw.EntryRow _trackTotalRow;
    [Gtk.Connect] private readonly Adw.EntryRow _albumArtistRow;
    [Gtk.Connect] private readonly Adw.EntryRow _genreRow;
    [Gtk.Connect] private readonly Adw.EntryRow _commentRow;
    [Gtk.Connect] private readonly Adw.EntryRow _bpmRow;
    [Gtk.Connect] private readonly Adw.EntryRow _composerRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Adw.EntryRow _publisherRow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _customPropertiesGroup;
    [Gtk.Connect] private readonly Gtk.Label _fingerprintLabel;
    [Gtk.Connect] private readonly Gtk.Button _copyFingerprintButton;
    [Gtk.Connect] private readonly Gtk.Spinner _fingerprintSpinner;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        _currentAlbumArtType = AlbumArtType.Front;
        _listMusicFilesRows = new List<Adw.ActionRow>();
        _customPropertyRows = new List<Adw.EntryRow>();
        _isSelectionOccuring = false;
        SetDefaultSize(_controller.WindowWidth, _controller.WindowHeight);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        if (_controller.AppInfo.IsDevVersion)
        {
            AddCssClass("devel");
        }
        //Build UI
        builder.Connect(this);
        _title.SetTitle(_controller.AppInfo.ShortName);
        _musicFilesSearch.OnSearchChanged += SearchChanged;
        _advancedSearchInfoButton.OnClicked += AdvancedSearchInfo;
        _selectAllButton.OnClicked += (sender, e) => _listMusicFiles.SelectAll();
        var musicFilesVadjustment = _scrolledWindowMusicFiles.GetVadjustment();
        musicFilesVadjustment.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "value")
            {
                _searchSeparator.SetVisible(musicFilesVadjustment.GetValue() > 0);
            }
        };
        _updateHeaderFunc = (rowPtr, _, _) =>
        {
            if (!string.IsNullOrEmpty(_listHeader))
            {
                var label = Gtk.Label.New(_listHeader);
                label.AddCssClass("dim-label");
                label.AddCssClass("title-4");
                label.SetMarginTop(6);
                label.SetMarginStart(6);
                label.SetMarginEnd(6);
                label.SetMarginBottom(6);
                label.SetHalign(Gtk.Align.Start);
                label.SetEllipsize(Pango.EllipsizeMode.End);
                gtk_list_box_row_set_header(rowPtr, label.Handle);
                _listHeader = string.Empty;
            }
        };
        gtk_list_box_set_header_func(_listMusicFiles.Handle, _updateHeaderFunc, IntPtr.Zero, IntPtr.Zero);
        _listMusicFiles.OnSelectedRowsChanged += ListMusicFiles_SelectionChanged;
        var switchAlbumArtLabel = (Gtk.Label)_switchAlbumArtButtonContent.GetLastChild();
        switchAlbumArtLabel.SetWrap(true);
        switchAlbumArtLabel.SetJustify(Gtk.Justification.Center);
        _artTypeLabel.SetLabel(_currentAlbumArtType == AlbumArtType.Front ? _("Front") : _("Back"));
        _filenameRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _titleRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _artistRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _albumRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _yearRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _trackRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _trackTotalRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _albumArtistRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        //Genre and Autocomplete
        _autocompleteBox = new AutocompleteBox(_genreRow);
        _autocompleteBox.SetSizeRequest(327, -1);
        _autocompleteBox.SetMarginTop(320);
        _autocompleteBox.SuggestionAccepted += (sender, e) =>
        {
            _genreRow.SetText(e);
            _genreRow.GrabFocus();
            _genreRow.SetPosition(-1);
        };
        _mainPropOverlay.AddOverlay(_autocompleteBox);
        _genreRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                var matchingGenres = _controller.GetGenreSuggestions(_genreRow.GetText());
                if (matchingGenres.Count > 0)
                {
                    _autocompleteBox.UpdateSuggestions(matchingGenres);
                }
                _autocompleteBox.SetVisible(matchingGenres.Count > 0);
                TagPropertyChanged();
            }
        };
        _commentRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _bpmRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _composerRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _descriptionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _publisherRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _fingerprintLabel.SetEllipsize(Pango.EllipsizeMode.End);
        _copyFingerprintButton.OnClicked += CopyFingerprintToClipboard;
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width")
            {
                _autocompleteBox.SetSizeRequest(_genreRow.GetAllocatedWidth() - 24, -1);
            }
        };
        //Register Events
        OnCloseRequest += OnCloseRequested;
        _controller.NotificationSent += NotificationSent;
        _controller.ShellNotificationSent += ShellNotificationSent;
        _controller.LoadingStateUpdated += (sender, e) => SetLoadingState(e);
        _controller.LoadingProgressUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () =>
        {
            UpdateLoadingProgress(e);
            return false;
        });
        _controller.MusicLibraryUpdated += (sender, e) => GLib.Functions.IdleAdd(0, MusicLibraryUpdated);
        _controller.MusicFileSaveStatesChanged += (sender, e) => GLib.Functions.IdleAdd(0, () => MusicFileSaveStatesChanged(e));
        _controller.SelectedMusicFilesPropertiesChanged += (sender, e) => GLib.Functions.IdleAdd(0, SelectedMusicFilesPropertiesChanged);
        _controller.FingerprintCalculated += (sender, e) => GLib.Functions.IdleAdd(0, UpdateFingerprint);
        _controller.CorruptedFilesFound += (sender, e) => GLib.Functions.IdleAdd(0, CorruptedFilesFound);
        //Open Folder Action
        var actOpenFolder = Gio.SimpleAction.New("openFolder", null);
        actOpenFolder.OnActivate += async (sender, e) => await OpenFolderAsync();
        AddAction(actOpenFolder);
        application.SetAccelsForAction("win.openFolder", new string[] { "<Ctrl>O" });
        //Open Playlist Action
        var actOpenPlaylist = Gio.SimpleAction.New("openPlaylist", null);
        actOpenPlaylist.OnActivate += async (sender, e) => await OpenPlaylistAsync();
        AddAction(actOpenPlaylist);
        application.SetAccelsForAction("win.openPlaylist", new string[] { "<Shift><Ctrl>O" });
        //Close Library Action
        var actCloseLibrary = Gio.SimpleAction.New("closeLibrary", null);
        actCloseLibrary.OnActivate += (sender, e) => _controller.CloseLibrary();
        AddAction(actCloseLibrary);
        application.SetAccelsForAction("win.actCloseLibrary", new string[] { "<Ctrl>W" });
        //Reload Library Action
        var actReloadLibrary = Gio.SimpleAction.New("reloadLibrary", null);
        actReloadLibrary.OnActivate += ReloadLibrary;
        AddAction(actReloadLibrary);
        application.SetAccelsForAction("win.reloadLibrary", new string[] { "F5" });
        //Create Playlist Action
        _createPlaylistAction = Gio.SimpleAction.New("createPlaylist", null);
        _createPlaylistAction.OnActivate += CreatePlaylist;
        AddAction(_createPlaylistAction);
        application.SetAccelsForAction("win.createPlaylist", new string[] { "<Ctrl>P" });
        //Add To Playlist Action
        _addToPlaylistAction = Gio.SimpleAction.New("addToPlaylist", null);
        _addToPlaylistAction.OnActivate += AddToPlaylist;
        AddAction(_addToPlaylistAction);
        application.SetAccelsForAction("win.addToPlaylist", new string[] { "<Alt>O" });
        //Remove From Playlist Action
        _removeFromPlaylistAction = Gio.SimpleAction.New("removeFromPlaylist", null);
        _removeFromPlaylistAction.OnActivate += RemoveFromPlaylist;
        application.SetAccelsForAction("win.removeFromPlaylist", new string[] { "<Alt>Delete" });
        AddAction(_removeFromPlaylistAction);
        //Apply Action
        _applyAction = Gio.SimpleAction.New("apply", null);
        _applyAction.OnActivate += Apply;
        AddAction(_applyAction);
        application.SetAccelsForAction("win.apply", new string[] { "<Ctrl>S" });
        //Discard Unapplied Changes Action
        var actDiscard = Gio.SimpleAction.New("discardUnappliedChanges", null);
        actDiscard.OnActivate += DiscardUnappliedChanges;
        AddAction(actDiscard);
        application.SetAccelsForAction("win.discardUnappliedChanges", new string[] { "<Ctrl>Z" });
        //Delete Tags Action
        var actDeleteTags = Gio.SimpleAction.New("deleteTags", null);
        actDeleteTags.OnActivate += DeleteTags;
        AddAction(actDeleteTags);
        application.SetAccelsForAction("win.deleteTags", new string[] { "<Shift>Delete" });
        //Convert Filename To Tag Action
        var actFTT = Gio.SimpleAction.New("filenameToTag", null);
        actFTT.OnActivate += FilenameToTag;
        AddAction(actFTT);
        application.SetAccelsForAction("win.filenameToTag", new string[] { "<Ctrl>F" });
        //Convert Tag To Filename Action
        var actTTF = Gio.SimpleAction.New("tagToFilename", null);
        actTTF.OnActivate += TagToFilename;
        AddAction(actTTF);
        application.SetAccelsForAction("win.tagToFilename", new string[] { "<Ctrl>T" });
        //Switch Album Art Action
        var actSwitchAlbumArt = Gio.SimpleAction.New("switchAlbumArt", null);
        actSwitchAlbumArt.OnActivate += SwitchAlbumArt;
        AddAction(actSwitchAlbumArt);
        application.SetAccelsForAction("win.switchAlbumArt", new string[] { "<Ctrl><Shift>S" });
        _switchAlbumArtButton.SetDetailedActionName("win.switchAlbumArt");
        //Insert Album Art Action
        _insertAlbumArtAction = Gio.SimpleAction.New("insertAlbumArt", null);
        _insertAlbumArtAction.OnActivate += async (sender, e) => await InsertAlbumArtAsync(_currentAlbumArtType);
        AddAction(_insertAlbumArtAction);
        _insertAlbumArtButton.SetDetailedActionName("win.insertAlbumArt");
        //Remove Album Art Action
        _removeAlbumArtAction = Gio.SimpleAction.New("removeAlbumArt", null);
        _removeAlbumArtAction.OnActivate += (sender, e) => RemoveAlbumArt(_currentAlbumArtType);
        AddAction(_removeAlbumArtAction);
        _removeAlbumArtButton.SetDetailedActionName("win.removeAlbumArt");
        //Export Album Art Action
        _exportAlbumArtAction = Gio.SimpleAction.New("exportAlbumArt", null);
        _exportAlbumArtAction.OnActivate += async (sender, e) => await ExportAlbumArtAsync(_currentAlbumArtType);
        AddAction(_exportAlbumArtAction);
        _exportAlbumArtButton.SetDetailedActionName("win.exportAlbumArt");
        //Insert Front Album Art Action
        var actInsertFrontAlbumArt = Gio.SimpleAction.New("insertFrontAlbumArt", null);
        actInsertFrontAlbumArt.OnActivate += async (sender, e) => await InsertAlbumArtAsync(AlbumArtType.Front);
        AddAction(actInsertFrontAlbumArt);
        application.SetAccelsForAction("win.insertFrontAlbumArt", new string[] { "<Ctrl>I" });
        //Remove Front Album Art Action
        var actRemoveFrontAlbumArt = Gio.SimpleAction.New("removeFrontAlbumArt", null);
        actRemoveFrontAlbumArt.OnActivate += (sender, e) => RemoveAlbumArt(AlbumArtType.Front);
        AddAction(actRemoveFrontAlbumArt);
        application.SetAccelsForAction("win.removeFrontAlbumArt", new string[] { "<Ctrl>Delete" });
        //Export Front Album Art Action
        var actExportFrontAlbumArt = Gio.SimpleAction.New("exportFrontAlbumArt", null);
        actExportFrontAlbumArt.OnActivate += async (sender, e) => await ExportAlbumArtAsync(AlbumArtType.Front);
        AddAction(actExportFrontAlbumArt);
        application.SetAccelsForAction("win.exportFrontAlbumArt", new string[] { "<Ctrl>E" });
        //Insert Back Album Art Action
        var actInsertBackAlbumArt = Gio.SimpleAction.New("insertBackAlbumArt", null);
        actInsertBackAlbumArt.OnActivate += async (sender, e) => await InsertAlbumArtAsync(AlbumArtType.Back);
        AddAction(actInsertBackAlbumArt);
        application.SetAccelsForAction("win.insertBackAlbumArt", new string[] { "<Ctrl><Shift>I" });
        //Remove Back Album Art Action
        var actRemoveBackAlbumArt = Gio.SimpleAction.New("removeBackAlbumArt", null);
        actRemoveBackAlbumArt.OnActivate += (sender, e) => RemoveAlbumArt(AlbumArtType.Back);
        AddAction(actRemoveBackAlbumArt);
        application.SetAccelsForAction("win.removeBackAlbumArt", new string[] { "<Ctrl><Shift>Delete" });
        //Export Front Album Art Action
        var actExportBackAlbumArt = Gio.SimpleAction.New("exportBackAlbumArt", null);
        actExportBackAlbumArt.OnActivate += async (sender, e) => await ExportAlbumArtAsync(AlbumArtType.Back);
        AddAction(actExportBackAlbumArt);
        application.SetAccelsForAction("win.exportBackAlbumArt", new string[] { "<Ctrl><Shift>E" });
        //Lyrics Action
        _lyricsAction = Gio.SimpleAction.New("lyrics", null);
        _lyricsAction.OnActivate += Lyrics;
        AddAction(_lyricsAction);
        application.SetAccelsForAction("win.lyrics", new string[] { "<Ctrl>L" });
        //Add Custom Property Action
        var actAddCustomProperty = Gio.SimpleAction.New("addCustomProperty", null);
        actAddCustomProperty.OnActivate += AddCustomProperty;
        AddAction(actAddCustomProperty);
        //Download MusicBrainz Metadata Action
        _musicBrainzAction = Gio.SimpleAction.New("downloadMusicBrainzMetadata", null);
        _musicBrainzAction.OnActivate += DownloadMusicBrainzMetadata;
        AddAction(_musicBrainzAction);
        application.SetAccelsForAction("win.downloadMusicBrainzMetadata", new string[] { "<Ctrl>m" });
        //Download Lyrics Action
        _downloadLyricsAction = Gio.SimpleAction.New("downloadLyrics", null);
        _downloadLyricsAction.OnActivate += DownloadLyrics;
        AddAction(_downloadLyricsAction);
        application.SetAccelsForAction("win.downloadLyrics", new string[] { "<Ctrl><Shift>l" });
        //Submit to AcoustId Action
        _acoustIdAction = Gio.SimpleAction.New("submitToAcoustId", null);
        _acoustIdAction.OnActivate += SubmitToAcoustId;
        AddAction(_acoustIdAction);
        application.SetAccelsForAction("win.submitToAcoustId", new string[] { "<Ctrl>u" });
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += Preferences;
        AddAction(actPreferences);
        application.SetAccelsForAction("win.preferences", new string[] { "<Ctrl>comma" });
        //Keyboard Shortcuts Action
        var actKeyboardShortcuts = Gio.SimpleAction.New("keyboardShortcuts", null);
        actKeyboardShortcuts.OnActivate += KeyboardShortcuts;
        AddAction(actKeyboardShortcuts);
        application.SetAccelsForAction("win.keyboardShortcuts", new string[] { "<Ctrl>question" });
        //Quit Action
        var actQuit = Gio.SimpleAction.New("quit", null);
        actQuit.OnActivate += Quit;
        AddAction(actQuit);
        application.SetAccelsForAction("win.quit", new string[] { "<Ctrl>q" });
        //Help Action
        var actHelp = Gio.SimpleAction.New("help", null);
        actHelp.OnActivate += (sender, e) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("index"), 0);
        AddAction(actHelp);
        application.SetAccelsForAction("win.help", new string[] { "F1" });
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        AddAction(actAbout);
        //Drop Target
        _dropTarget = Gtk.DropTarget.New(Gio.FileHelper.GetGType(), Gdk.DragAction.Copy);
        _dropTarget.OnDrop += OnDrop;
        AddController(_dropTarget);
    }

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="controller">The MainWindowController</param>
    /// <param name="application">The Adw.Application</param>
    public MainWindow(MainWindowController controller, Adw.Application application) : this(Builder.FromFile("window.ui"), controller, application)
    {
    }

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public async Task StartAsync()
    {
        _application.AddWindow(this);
        Present();
        SetLoadingState(_("Loading music files from library..."));
        await _controller.StartupAsync();
        _controller.NetworkMonitor!.StateChanged += (sender, state) =>
        {
            _musicBrainzAction.SetEnabled(state);
            _downloadLyricsAction.SetEnabled(state);
            _acoustIdAction.SetEnabled(state);
        };
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e)
    {
        var toast = Adw.Toast.New(e.Message);
        if (e.Action == "unsupported")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("unsupported"), 0);
        }
        else if (e.Action == "format")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("format-string"), 0);
        }
        else if (e.Action == "web")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("web-services"), 0);
        }
        else if (e.Action == "musicbrainz" && !string.IsNullOrWhiteSpace(e.ActionParam))
        {
            toast.SetButtonLabel(_("Info"));
            toast.OnButtonClicked += (_, _) =>
            {
                var messageDialog = new ScrollingMessageDialog(this, _controller.AppInfo.ID, _("Failed MusicBrainz Lookups"), e.ActionParam);
                messageDialog.Present();
            };
        }
        _toastOverlay.AddToast(toast);
    }

    /// <summary>
    /// Occurs when a shell notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">ShellNotificationSentEventArgs</param>
    private void ShellNotificationSent(object? sender, ShellNotificationSentEventArgs? e)
    {
        var notification = Gio.Notification.New(e.Title);
        notification.SetBody(e.Message);
        notification.SetPriority(e.Severity switch
        {
            NotificationSeverity.Success => Gio.NotificationPriority.High,
            NotificationSeverity.Warning => Gio.NotificationPriority.Urgent,
            NotificationSeverity.Error => Gio.NotificationPriority.Urgent,
            _ => Gio.NotificationPriority.Normal
        });
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            notification.SetIcon(Gio.ThemedIcon.New($"{_controller.AppInfo.ID}-symbolic"));
        }
        else
        {
            var fileIcon = Gio.FileIcon.New(Gio.FileHelper.NewForPath($"{Environment.GetEnvironmentVariable("SNAP")}/usr/share/icons/hicolor/symbolic/apps/{_controller.AppInfo.ID}-symbolic.svg"));
            notification.SetIcon(fileIcon);
        }
        _application.SendNotification(_controller.AppInfo.ID, notification);
    }

    /// <summary>
    /// Sets the app into a loading state
    /// </summary>
    /// <param name="message">The message to show on the loading screen</param>
    private void SetLoadingState(string message)
    {
        _viewStack.SetVisibleChildName("Loading");
        _loadingLabel.SetText(message);
        _loadingProgressBar.SetVisible(false);
        _loadingProgressLabel.SetVisible(false);
        _applyAction.SetEnabled(false);
        _tagActionsButton.SetSensitive(false);
    }

    /// <summary>
    /// Updates the progress of the loading state
    /// </summary>
    /// <param name="e">(int Value, int MaxValue, string Message)</param>
    private void UpdateLoadingProgress((int Value, int MaxValue, string Message) e)
    {
        _loadingProgressBar.SetVisible(true);
        _loadingProgressLabel.SetVisible(true);
        _loadingProgressBar.SetFraction((double)e.Value / (double)e.MaxValue);
        _loadingProgressLabel.SetLabel(e.Message);
    }

    /// <summary>
    /// Occurs when the window tries to close
    /// </summary>
    /// <param name="sender">Gtk.Window</param>
    /// <param name="e">EventArgs</param>
    /// <returns>True to stop close, else false</returns>
    private bool OnCloseRequested(Gtk.Window sender, EventArgs e)
    {
        GetDefaultSize(out var width, out var height);
        _controller.WindowWidth = width;
        _controller.WindowHeight = height;
        Aura.Active.SaveConfig("config");
        if (!_controller.CanClose)
        {
            var dialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("cancel", _("Cancel"));
            dialog.SetDefaultResponse("cancel");
            dialog.SetCloseResponse("cancel");
            dialog.AddResponse("discard", _("Discard"));
            dialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
            dialog.AddResponse("apply", _("Apply"));
            dialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
            dialog.OnResponse += async (s, ea) =>
            {
                if (ea.Response == "apply")
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(false);
                    Close();
                }
                else if (ea.Response == "discard")
                {
                    _controller.ForceAllowClose();
                    Close();
                }
                dialog.Destroy();
            };
            dialog.Present();
            return true;
        }
        _listMusicFiles.UnselectAll();
        _controller.Dispose();
        return false;
    }

    /// <summary>
    /// Occurs when something is dropped onto the window
    /// </summary>
    /// <param name="sender">Gtk.DropTarget</param>
    /// <param name="e">Gtk.DropTarget.DropSignalArgs</param>
    private bool OnDrop(Gtk.DropTarget sender, Gtk.DropTarget.DropSignalArgs e)
    {
        var obj = e.Value.GetObject();
        if (obj is Gio.FileHelper file)
        {
            var path = file.GetPath() ?? "";
            if (Directory.Exists(path) || (File.Exists(path) && Enum.GetValues<PlaylistFormat>().Select(x => x.GetDotExtension()).Contains(Path.GetExtension(path))))
            {
                SetLoadingState(_("Loading music files from library..."));
                _controller.OpenLibraryAsync(path).Wait();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Occurs when the open folder action is triggered
    /// </summary>
    private async Task OpenFolderAsync()
    {
        var folderDialog = Gtk.FileDialog.New();
        folderDialog.SetTitle(_("Open Folder"));
        try
        {
            var file = await folderDialog.SelectFolderAsync(this);
            SetLoadingState(_("Loading music files from folder..."));
            await _controller.OpenLibraryAsync(file.GetPath());
        }
        catch { }
    }
    
    /// <summary>
    /// Occurs when the open playlist action is triggered
    /// </summary>
    private async Task OpenPlaylistAsync()
    {
        var fileDialog = Gtk.FileDialog.New();
        fileDialog.SetTitle(_("Open Playlist"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName(_("All Files"));
        foreach (var format in Enum.GetValues<PlaylistFormat>())
        {
            var filter = Gtk.FileFilter.New();
            var extension = format.GetDotExtension();
            filter.SetName($"{format.ToString()} (*{extension})");
            filter.AddPattern($"*{extension}");
            filter.AddPattern($"*{extension.ToUpper()}");
            filterAll.AddPattern($"*{extension}");
            filterAll.AddPattern($"*{extension.ToUpper()}");
            filters.Append(filter);
        }
        filters.Insert(0, filterAll);
        fileDialog.SetFilters(filters);
        try
        {
            var file = await fileDialog.OpenAsync(this);
            SetLoadingState(_("Loading music files from playlist..."));
            await _controller.OpenLibraryAsync(file.GetPath());
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the reload library action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void ReloadLibrary(Gio.SimpleAction sender, EventArgs e)
    {
        if (!_controller.CanClose)
        {
            var dialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("cancel", _("Cancel"));
            dialog.SetDefaultResponse("cancel");
            dialog.SetCloseResponse("cancel");
            dialog.AddResponse("discard", _("Discard"));
            dialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
            dialog.AddResponse("apply", _("Apply"));
            dialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
            dialog.OnResponse += async (s, ea) =>
            {
                if (ea.Response == "apply")
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(false);
                }
                if (ea.Response != "cancel")
                {
                    SetLoadingState(_("Loading music files from library..."));
                    await _controller.ReloadLibraryAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            SetLoadingState(_("Loading music files from library..."));
            await _controller.ReloadLibraryAsync();
        }
    }

    /// <summary>
    /// Occurs when the remove to playlist action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void CreatePlaylist(Gio.SimpleAction sender, EventArgs e)
    {
        var createPlaylistDialog = new CreatePlaylistDialog(this, _controller.AppInfo.ID, Path.GetFileName(_controller.MusicLibraryName));
        createPlaylistDialog.OnCreate += (s, po) => _controller.CreatePlaylist(po);
        createPlaylistDialog.Present();
    }
    
    /// <summary>
    /// Occurs when the create playlist action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void AddToPlaylist(Gio.SimpleAction sender, EventArgs e)
    {
        var fileDialog = Gtk.FileDialog.New();
        fileDialog.SetTitle(_("Open Music File"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        var filterAll = Gtk.FileFilter.New();
        filterAll.SetName(_("All Files"));
        foreach (var ext in MusicLibrary.SupportedExtensions)
        {
            var filter = Gtk.FileFilter.New();
            filter.SetName($"{ext.Replace(".", "").ToUpper()} (*{ext})");
            filter.AddPattern($"*{ext}");
            filter.AddPattern($"*{ext.ToUpper()}");
            filterAll.AddPattern($"*{ext}");
            filterAll.AddPattern($"*{ext.ToUpper()}");
            filters.Append(filter);
        }
        filters.Insert(0, filterAll);
        fileDialog.SetFilters(filters);
        try
        {
            var file = await fileDialog.OpenAsync(this);
            if (!_controller.CanClose)
            {
                var applyDialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
                applyDialog.SetIconName(_controller.AppInfo.ID);
                applyDialog.AddResponse("cancel", _("Cancel"));
                applyDialog.SetDefaultResponse("cancel");
                applyDialog.SetCloseResponse("cancel");
                applyDialog.AddResponse("discard", _("Discard"));
                applyDialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
                applyDialog.AddResponse("apply", _("Apply"));
                applyDialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
                applyDialog.OnResponse += async (ss, eaa) =>
                {
                    if (eaa.Response == "apply")
                    {
                        SetLoadingState(_("Saving tags..."));
                        await _controller.SaveAllTagsAsync(false);
                    }
                    if (eaa.Response == "discard")
                    {
                        SetLoadingState(_("Discarding tags..."));
                        await _controller.DiscardSelectedUnappliedChangesAsync();
                    }
                    if (eaa.Response != "cancel")
                    {
                        SetLoadingState(_("Loading music file..."));
                        await _controller.AddFileToPlaylist(file.GetPath());
                    }
                    applyDialog.Destroy();
                };
                applyDialog.Present();
            }
            else
            {
                SetLoadingState(_("Loading music file..."));
                await _controller.AddFileToPlaylist(file.GetPath());
            }
        }
        catch { }
    }
    
    /// <summary>
    /// Occurs when the add to playlist action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void RemoveFromPlaylist(Gio.SimpleAction sender, EventArgs e)
    {
        if (_controller.SelectedMusicFiles.Count == 0)
        {
            NotificationSent(this, new NotificationSentEventArgs(_("No files selected for removal."), NotificationSeverity.Error));
        }
        else
        {
            var dialog = Adw.MessageDialog.New(this, _("Remove Files?"), _("The selected files will not be deleted from disk but will be removed from this playlist."));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("no", _("No"));
            dialog.SetDefaultResponse("no");
            dialog.SetCloseResponse("no");
            dialog.AddResponse("yes", _("Yes"));
            dialog.SetResponseAppearance("yes", Adw.ResponseAppearance.Destructive);
            dialog.OnResponse += async (s, ea) =>
            {
                if (ea.Response == "yes")
                {
                    if (!_controller.CanClose)
                    {
                        var applyDialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
                        applyDialog.SetIconName(_controller.AppInfo.ID);
                        applyDialog.AddResponse("cancel", _("Cancel"));
                        applyDialog.SetDefaultResponse("cancel");
                        applyDialog.SetCloseResponse("cancel");
                        applyDialog.AddResponse("discard", _("Discard"));
                        applyDialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
                        applyDialog.AddResponse("apply", _("Apply"));
                        applyDialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
                        applyDialog.OnResponse += async (ss, eaa) =>
                        {
                            if (eaa.Response == "apply")
                            {
                                SetLoadingState(_("Saving tags..."));
                                await _controller.SaveAllTagsAsync(false);
                            }
                            if (eaa.Response == "discard")
                            {
                                SetLoadingState(_("Discarding tags..."));
                                await _controller.DiscardSelectedUnappliedChangesAsync();
                            }
                            if (eaa.Response != "cancel")
                            {
                                SetLoadingState(_("Removing files from playlist..."));
                                await _controller.RemoveSelectedFilesFromPlaylistAsync();
                            }
                            applyDialog.Destroy();
                        };
                        applyDialog.Present();
                    }
                    else
                    {
                        SetLoadingState(_("Removing files from playlist..."));
                        await _controller.RemoveSelectedFilesFromPlaylistAsync();
                    }
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
    }

    /// <summary>
    /// Occurs when the apply action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void Apply(Gio.SimpleAction sender, EventArgs e)
    {
        SetLoadingState(_("Saving tags..."));
        await _controller.SaveSelectedTagsAsync();
    }

    /// <summary>
    /// Occurs when the discard unapplied changes action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void DiscardUnappliedChanges(Gio.SimpleAction sender, EventArgs e)
    {
        SetLoadingState(_("Discarding unapplied changes..."));
        await _controller.DiscardSelectedUnappliedChangesAsync();
    }

    /// <summary>
    /// Occurs when the delete tags action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void DeleteTags(Gio.SimpleAction sender, EventArgs e) => _controller.DeleteSelectedTags();

    /// <summary>
    /// Occurs when the filename to tag action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void FilenameToTag(Gio.SimpleAction sender, EventArgs e)
    {
        var dialog = new ComboBoxDialog(this, _controller.AppInfo.ID, _("File Name to Tag"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _("Cancel"), _("Convert"));
        dialog.OnResponse += (s, ea) =>
        {
            if (!string.IsNullOrEmpty(dialog.Response))
            {
                _controller.FilenameToTag(dialog.Response);
            }
            dialog.Destroy();
        };
        dialog.Present();
    }

    /// <summary>
    /// Occurs when the tag to filename action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void TagToFilename(Gio.SimpleAction sender, EventArgs e)
    {
        var dialog = new ComboBoxDialog(this, _controller.AppInfo.ID, _("Tag to File Name"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _("Cancel"), _("Convert"));
        dialog.OnResponse += (s, ea) =>
        {
            if (!string.IsNullOrEmpty(dialog.Response))
            {
                _controller.TagToFilename(dialog.Response);
            }
            dialog.Destroy();
        };
        dialog.Present();
    }

    /// <summary>
    /// Occurs when the switch album art action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void SwitchAlbumArt(Gio.SimpleAction sender, EventArgs e)
    {
        _currentAlbumArtType = _currentAlbumArtType == AlbumArtType.Front ? AlbumArtType.Back : AlbumArtType.Front;
        _switchAlbumArtButtonContent.SetLabel(_currentAlbumArtType == AlbumArtType.Front ? _("Switch to Back Cover") : _("Switch to Front Cover"));
        _artTypeLabel.SetLabel(_currentAlbumArtType == AlbumArtType.Front ? _("Front") : _("Back"));
        SelectedMusicFilesPropertiesChanged();
    }

    /// <summary>
    /// Occurs when the insert album art action is triggered
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    private async Task InsertAlbumArtAsync(AlbumArtType type)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(type == AlbumArtType.Front ? _("Insert Front Album Art") : _("Insert Back Album Art"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        var filterImages = Gtk.FileFilter.New();
        filterImages.AddMimeType("image/jpeg");
        filterImages.AddMimeType("image/png");
        filterImages.AddMimeType("image/bmp");
        filterImages.AddMimeType("image/webp");
        filters.Append(filterImages);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(this);
            _controller.InsertSelectedAlbumArt(file.GetPath(), type);
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the remove album art action is triggered
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    private void RemoveAlbumArt(AlbumArtType type) => _controller.RemoveSelectedAlbumArt(type);

    /// <summary>
    /// Occurs when the export album art action is triggered
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    private async Task ExportAlbumArtAsync(AlbumArtType type)
    {
        var albumArt = type == AlbumArtType.Front ? _controller.SelectedPropertyMap.FrontAlbumArt : _controller.SelectedPropertyMap.BackAlbumArt;
        if (albumArt != "hasArt")
        {
            return;
        }
        var saveFileDialog = Gtk.FileDialog.New();
        saveFileDialog.SetTitle(type == AlbumArtType.Front ? _("Export Front Album Art") : _("Export Back Album Art"));
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        var filter = Gtk.FileFilter.New();
        filter.AddMimeType(_controller.GetFirstAlbumArtMimeType(type));
        filters.Append(filter);
        saveFileDialog.SetFilters(filters);
        try
        {
            var file = await saveFileDialog.SaveAsync(this);
            _controller.ExportSelectedAlbumArt(file.GetPath(), type);
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the lyrics action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Lyrics(Gio.SimpleAction sender, EventArgs e)
    {
        var controller = _controller.CreateLyricsDialogController();
        var lyricsDialog = new LyricsDialog(controller, this, _controller.AppInfo.ID);
        lyricsDialog.OnHide += (s, ea) =>
        {
            _controller.UpdateLyrics(controller.Lyrics);
            lyricsDialog.Destroy();
        };
        lyricsDialog.Present();
    }

    /// <summary>
    /// Occurs when the add custom property action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void AddCustomProperty(Gio.SimpleAction sender, EventArgs e)
    {
        var entryDialog = new EntryDialog(this, _controller.AppInfo.ID, _("New Custom Property"), "", _("Property Name"), _("Cancel"), _("Add"));
        entryDialog.OnResponse += (s, ea) =>
        {
            if (!string.IsNullOrEmpty(entryDialog.Response))
            {
                _controller.AddCustomProperty(entryDialog.Response);
            }
            entryDialog.Destroy();
        };
        entryDialog.Present();
    }

    /// <summary>
    /// Occurs when the download musicbrainz metadata action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void DownloadMusicBrainzMetadata(Gio.SimpleAction sender, EventArgs e)
    {
        SetLoadingState(_("Downloading MusicBrainz metadata..."));
        await _controller.DownloadMusicBrainzMetadataAsync();
    }

    private async void DownloadLyrics(Gio.SimpleAction sender, EventArgs e)
    {
        if (!_controller.CanClose)
        {
            var dialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("cancel", _("Cancel"));
            dialog.SetDefaultResponse("cancel");
            dialog.SetCloseResponse("cancel");
            dialog.AddResponse("discard", _("Discard"));
            dialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
            dialog.AddResponse("apply", _("Apply"));
            dialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
            dialog.OnResponse += async (s, ea) =>
            {
                if (ea.Response == "apply")
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(false);
                }
                else if (ea.Response == "discard")
                {
                    SetLoadingState(_("Discarding tags..."));
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (ea.Response != "cancel")
                {
                    SetLoadingState(_("Downloading lyrics..."));
                    await _controller.DownloadLyricsAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            SetLoadingState(_("Downloading lyrics..."));
            await _controller.DownloadLyricsAsync();
        }
    }

    /// <summary>
    /// Occurs when the submit to acoustid action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void SubmitToAcoustId(Gio.SimpleAction sender, EventArgs e)
    {
        if (_controller.SelectedMusicFiles.Count > 1)
        {
            var dialog = Adw.MessageDialog.New(this, _("Too Many Files Selected"), _("Only one file can be submitted to AcoustID at a time. Please select only one file and try again."));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("ok", _("OK"));
            dialog.SetDefaultResponse("ok");
            dialog.SetCloseResponse("ok");
            dialog.OnResponse += (s, ea) => dialog.Destroy();
            dialog.Present();
            return;
        }
        var entryDialog = new EntryDialog(this, _controller.AppInfo.ID, _("Submit to AcoustId"), _("AcoustId can associate a song's fingerprint with a MusicBrainz Recording Id for easy identification.\n\nIf you have a MusicBrainz Recording Id for this song, please provide it below.\n\nIf none is provided, Tagger will submit your tag's metadata in association with the fingerprint instead."), _("MusicBrainz Recording Id"), _("Cancel"), _("Submit"));
        entryDialog.OnResponse += async (_, _) =>
        {
            if (!string.IsNullOrEmpty(entryDialog.Response))
            {
                if (!_controller.CanClose)
                {
                    var dialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
                    dialog.SetIconName(_controller.AppInfo.ID);
                    dialog.AddResponse("cancel", _("Cancel"));
                    dialog.SetDefaultResponse("cancel");
                    dialog.SetCloseResponse("cancel");
                    dialog.AddResponse("discard", _("Discard"));
                    dialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
                    dialog.AddResponse("apply", _("Apply"));
                    dialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
                    dialog.OnResponse += async (s, ea) =>
                    {
                        if (ea.Response == "apply")
                        {
                            SetLoadingState(_("Saving tags..."));
                            await _controller.SaveAllTagsAsync(false);
                        }
                        if (ea.Response == "discard")
                        {
                            SetLoadingState(_("Discarding tags..."));
                            await _controller.DiscardSelectedUnappliedChangesAsync();
                        }
                        if (ea.Response != "cancel")
                        {
                            SetLoadingState(_("Submitting data to AcoustId..."));
                            await _controller.SubmitToAcoustIdAsync(entryDialog.Response == "NULL" ? null : entryDialog.Response);
                        }
                        dialog.Destroy();
                    };
                    dialog.Present();
                }
                else
                {
                    SetLoadingState(_("Submitting data to AcoustId..."));
                    await _controller.SubmitToAcoustIdAsync(entryDialog.Response == "NULL" ? null : entryDialog.Response);
                }
            }
            entryDialog.Destroy();
        };
        entryDialog.Present();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.CreatePreferencesViewController(), _application, this);
        if (!_controller.CanClose)
        {
            var dialog = Adw.MessageDialog.New(this, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"));
            dialog.SetIconName(_controller.AppInfo.ID);
            dialog.AddResponse("cancel", _("Cancel"));
            dialog.SetDefaultResponse("cancel");
            dialog.SetCloseResponse("cancel");
            dialog.AddResponse("discard", _("Discard"));
            dialog.SetResponseAppearance("discard", Adw.ResponseAppearance.Destructive);
            dialog.AddResponse("apply", _("Apply"));
            dialog.SetResponseAppearance("apply", Adw.ResponseAppearance.Suggested);
            dialog.OnResponse += async (s, ea) =>
            {
                if (ea.Response == "apply")
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(true);
                }
                else if (ea.Response == "discard")
                {
                    SetLoadingState(_("Discarding tags..."));
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (ea.Response != "cancel")
                {
                    preferencesDialog.Present();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            preferencesDialog.Present();
        }
    }

    /// <summary>
    /// Occurs when the keyboard shortcuts action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void KeyboardShortcuts(Gio.SimpleAction sender, EventArgs e)
    {
        var builder = Builder.FromFile("shortcuts_dialog.ui");
        var shortcutsWindow = (Gtk.ShortcutsWindow)builder.GetObject("_shortcuts")!;
        shortcutsWindow.SetTransientFor(this);
        shortcutsWindow.SetIconName(_controller.AppInfo.ID);
        shortcutsWindow.Present();
    }

    /// <summary>
    /// Occurs when quit action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Quit(Gio.SimpleAction sender, EventArgs e)
    {
        GetDefaultSize(out var width, out var height);
        _controller.WindowWidth = width;
        _controller.WindowHeight = height;
        Aura.Active.SaveConfig("config");
        _application.Quit();
    }

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine(_controller.AppInfo.ID);
        debugInfo.AppendLine(_controller.AppInfo.Version);
        debugInfo.AppendLine($"GTK {Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}");
        debugInfo.AppendLine($"libadwaita {Adw.Functions.GetMajorVersion()}.{Adw.Functions.GetMinorVersion()}.{Adw.Functions.GetMicroVersion()}");
        if (File.Exists("/.flatpak-info"))
        {
            debugInfo.AppendLine("Flatpak");
        }
        else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            debugInfo.AppendLine("Snap");
        }
        debugInfo.AppendLine(CultureInfo.CurrentCulture.ToString());
        var localeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "locale",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        try
        {
            localeProcess.Start();
            var localeString = localeProcess.StandardOutput.ReadToEnd().Trim();
            localeProcess.WaitForExit();
            debugInfo.AppendLine(localeString);
        }
        catch
        {
            debugInfo.AppendLine("Unknown locale");
        }
        var dialog = Adw.AboutWindow.New();
        dialog.SetTransientFor(this);
        dialog.SetIconName(_controller.AppInfo.ID);
        dialog.SetApplicationName(_controller.AppInfo.ShortName);
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.IsDevVersion ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetDebugInfo(debugInfo.ToString());
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2021-2023");
        dialog.SetWebsite("https://nickvision.org/");
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_("GitHub Repo"), _controller.AppInfo.SourceRepo.ToString());
        foreach (var pair in _controller.AppInfo.ExtraLinks)
        {
            dialog.AddLink(pair.Key, pair.Value.ToString());
        }
        dialog.SetDevelopers(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Developers));
        dialog.SetDesigners(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Designers));
        dialog.SetArtists(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Artists));
        dialog.SetTranslatorCredits(_controller.AppInfo.TranslatorCredits);
        dialog.SetReleaseNotes(_controller.AppInfo.HTMLChangelog);
        dialog.Present();
    }

    /// <summary>
    /// Occurs when the music library is updated
    /// </summary>
    private bool MusicLibraryUpdated()
    {
        _listMusicFiles.UnselectAll();
        foreach (var row in _listMusicFilesRows)
        {
            _listMusicFiles.Remove(row);
        }
        _listMusicFilesRows.Clear();
        if (!string.IsNullOrEmpty(_controller.MusicLibraryName))
        {
            string? comparable = null;
            foreach (var musicFile in _controller.MusicFiles)
            {
                var row = Adw.ActionRow.New();
                if (!string.IsNullOrEmpty(musicFile.Title))
                {
                    row.SetTitle($"{(musicFile.Track != 0 ? $"{musicFile.Track:D2} - " : "")}{Regex.Replace(musicFile.Title, "\\&", "&amp;")}");
                    row.SetSubtitle(Regex.Replace(musicFile.Filename, "\\&", "&amp;"));
                }
                else
                {
                    row.SetTitle(Regex.Replace(musicFile.Filename, "\\&", "&amp;"));
                    row.SetSubtitle("");
                }
                row.AddCssClass("card");
                var compareTo = _controller.SortFilesBy switch
                {
                    SortBy.Album => musicFile.Album,
                    SortBy.Artist => musicFile.Artist,
                    SortBy.Genre => musicFile.Genre,
                    SortBy.Path => Path.GetDirectoryName(musicFile.Path)!.Replace(_controller.MusicLibraryType == MusicLibraryType.Folder ? _controller.MusicLibraryName : UserDirectories.Home, _controller.MusicLibraryType == MusicLibraryType.Folder ? "" : "~"),
                    SortBy.Year => musicFile.Year.ToString(),
                    _ => null
                };
                if (!string.IsNullOrEmpty(compareTo) && compareTo[0] == Path.DirectorySeparatorChar && !Directory.Exists(compareTo))
                {
                    compareTo = compareTo.Remove(0, 1);
                }
                if (compareTo == string.Empty)
                {
                    compareTo = _controller.SortFilesBy != SortBy.Path ? _("Unknown") : "";
                }
                if (comparable != compareTo)
                {
                    comparable = compareTo;
                    _listHeader = compareTo!;
                    if (_listMusicFilesRows.Any())
                    {
                        if (_listMusicFilesRows[^1].HasCssClass("start-row"))
                        {
                            _listMusicFilesRows[^1].RemoveCssClass("start-row");
                            _listMusicFilesRows[^1].AddCssClass("single-row");
                        }
                        else
                        {
                            _listMusicFilesRows[^1].AddCssClass("end-row");
                        }
                    }
                    row.AddCssClass("start-row");
                }
                _listMusicFiles.Append(row);
                _listMusicFilesRows.Add(row);
            }
            if (_listMusicFilesRows.Any())
            {
                if (!_listMusicFilesRows[0].HasCssClass("start-row"))
                {
                    _listMusicFilesRows[0].AddCssClass("start-row");
                }
                if (_listMusicFilesRows[^1].HasCssClass("start-row"))
                {
                    _listMusicFilesRows[^1].RemoveCssClass("start-row");
                    _listMusicFilesRows[^1].AddCssClass("single-row");
                }
                else
                {
                    _listMusicFilesRows[^1].AddCssClass("end-row");
                }
            }
            _headerBar.RemoveCssClass("flat");
            _title.SetSubtitle(_controller.MusicLibraryName);
            _libraryButton.SetVisible(true);
            _createPlaylistAction.SetEnabled(_controller.MusicLibraryType == MusicLibraryType.Folder);
            _addToPlaylistAction.SetEnabled(_controller.MusicLibraryType == MusicLibraryType.Playlist);
            _removeFromPlaylistAction.SetEnabled(_controller.MusicLibraryType == MusicLibraryType.Playlist);
            _applyAction.SetEnabled(false);
            _tagActionsButton.SetSensitive(false);
            _viewStack.SetVisibleChildName("Library");
            _libraryFlap.SetFoldPolicy(_controller.MusicFiles.Count > 0 ? Adw.FlapFoldPolicy.Auto : Adw.FlapFoldPolicy.Always);
            _libraryFlap.SetRevealFlap(true);
            _flapToggleButton.SetSensitive(_controller.MusicFiles.Count > 0);
            _filesViewStack.SetVisibleChildName(_controller.MusicFiles.Count > 0 ? "Files" : "NoFiles");
        }
        else
        {
            _headerBar.AddCssClass("flat");
            _title.SetSubtitle("");
            _viewStack.SetVisibleChildName("NoLibrary");
            _libraryButton.SetVisible(false);
        }
        _selectionLabel.SetLabel(_("{0} of {1} selected", _controller.SelectedMusicFiles.Count, _controller.MusicFiles.Count));
        return false;
    }

    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    /// <param name="pending">Whether or not there are unsaved changes</param>
    private bool MusicFileSaveStatesChanged(bool pending)
    {
        _viewStack.SetVisibleChildName("Library");
        _libraryButton.SetVisible(true);
        _flapToggleButton.SetSensitive(_controller.MusicFiles.Count > 0);
        _applyAction.SetEnabled(pending);
        _tagActionsButton.SetSensitive(_controller.SelectedMusicFiles.Count != 0);
        var i = 0;
        foreach (var saved in _controller.MusicFileSaveStates)
        {
            _listMusicFilesRows[i].SetIconName(!saved ? "document-modified-symbolic" : "");
            i++;
        }
        return false;
    }

    /// <summary>
    /// Occurs when the selected music files' properties are changed
    /// </summary>
    private bool SelectedMusicFilesPropertiesChanged()
    {
        _isSelectionOccuring = true;
        //Update Properties
        _selectionLabel.SetLabel(_("{0} of {1} selected", _controller.SelectedMusicFiles.Count, _controller.MusicFiles.Count));
        _applyAction.SetEnabled(_controller.SelectedMusicFiles.Count != 0 && _controller.SelectedHasUnsavedChanges);
        _tagActionsButton.SetSensitive(_controller.SelectedMusicFiles.Count != 0);
        _filenameRow.SetEditable(_controller.SelectedMusicFiles.Count < 2);
        if (_controller.SelectedMusicFiles.Count == 0)
        {
            _musicFilesSearch.SetText("");
        }
        _filenameRow.SetText(_controller.SelectedPropertyMap.Filename);
        _titleRow.SetText(_controller.SelectedPropertyMap.Title);
        _artistRow.SetText(_controller.SelectedPropertyMap.Artist);
        _albumRow.SetText(_controller.SelectedPropertyMap.Album);
        _yearRow.SetText(_controller.SelectedPropertyMap.Year);
        _trackRow.SetText(_controller.SelectedPropertyMap.Track);
        _trackTotalRow.SetText(_controller.SelectedPropertyMap.TrackTotal);
        _albumArtistRow.SetText(_controller.SelectedPropertyMap.AlbumArtist);
        _genreRow.SetText(_controller.SelectedPropertyMap.Genre);
        _autocompleteBox.SetVisible(false);
        _commentRow.SetText(_controller.SelectedPropertyMap.Comment);
        _bpmRow.SetText(_controller.SelectedPropertyMap.BeatsPerMinute);
        _composerRow.SetText(_controller.SelectedPropertyMap.Composer);
        _descriptionRow.SetText(_controller.SelectedPropertyMap.Description);
        _publisherRow.SetText(_controller.SelectedPropertyMap.Publisher);
        _durationFileSizeLabel.SetLabel($"{_controller.SelectedPropertyMap.Duration} • {_controller.SelectedPropertyMap.FileSize}");
        _fingerprintLabel.SetLabel(_controller.SelectedPropertyMap.Fingerprint);
        var albumArt = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedPropertyMap.FrontAlbumArt : _controller.SelectedPropertyMap.BackAlbumArt;
        _filenameRow.SetEditable(true);
        _titleRow.SetEditable(true);
        _artistRow.SetEditable(true);
        _albumRow.SetEditable(true);
        _yearRow.SetEditable(true);
        _trackRow.SetEditable(true);
        _trackTotalRow.SetEditable(true);
        _albumArtistRow.SetEditable(true);
        _genreRow.SetEditable(true);
        _commentRow.SetEditable(true);
        _bpmRow.SetEditable(true);
        _composerRow.SetEditable(true);
        _descriptionRow.SetEditable(true);
        _publisherRow.SetEditable(true);
        if (albumArt == "hasArt")
        {
            _artViewStack.SetVisibleChildName("Image");
            var art = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedMusicFiles.First().Value.FrontAlbumArt : _controller.SelectedMusicFiles.First().Value.BackAlbumArt;
            if (art.Length == 0)
            {
                _albumArtImage.SetPaintable(null);
            }
            else
            {
                using var bytes = GLib.Bytes.From(art.AsSpan());
                using var texture = Gdk.Texture.NewFromBytes(bytes);
                _albumArtImage.SetPaintable(texture);
            }
        }
        else if (albumArt == "keepArt")
        {
            _artViewStack.SetVisibleChildName("KeepImage");
            _albumArtImage.SetPaintable(null);
        }
        else
        {
            _artViewStack.SetVisibleChildName("NoImage");
            _albumArtImage.SetPaintable(null);
        }
        _insertAlbumArtAction.SetEnabled(true);
        _removeAlbumArtAction.SetEnabled(_artViewStack.GetVisibleChildName() != "NoImage");
        _exportAlbumArtAction.SetEnabled(albumArt == "hasArt");
        //Update Custom Properties
        foreach (var row in _customPropertyRows)
        {
            _customPropertiesGroup.Remove(row);
        }
        _customPropertyRows.Clear();
        _customPropertiesGroup.SetVisible(_controller.SelectedMusicFiles.Count == 1);
        if (_controller.SelectedMusicFiles.Count == 1)
        {
            foreach (var pair in _controller.SelectedPropertyMap.CustomProperties)
            {
                var row = Adw.EntryRow.New();
                row.SetTitle(pair.Key);
                row.SetText(pair.Value);
                var removeButton = Gtk.Button.New();
                removeButton.SetValign(Gtk.Align.Center);
                removeButton.SetTooltipText(_("Remove Custom Property"));
                removeButton.SetIconName("user-trash-symbolic");
                removeButton.AddCssClass("flat");
                removeButton.OnClicked += (sender, e) => _controller.RemoveCustomProperty(pair.Key);
                row.AddSuffix(removeButton);
                row.OnNotify += (sender, e) =>
                {
                    if (e.Pspec.GetName() == "text")
                    {
                        TagPropertyChanged();
                    }
                };
                _customPropertyRows.Add(row);
                _customPropertiesGroup.Add(row);
            }
        }
        //Update Rows
        foreach (var pair in _controller.SelectedMusicFiles)
        {
            if (!string.IsNullOrEmpty(pair.Value.Title))
            {
                _listMusicFilesRows[pair.Key].SetTitle($"{(pair.Value.Track != 0 ? $"{pair.Value.Track:D2} - " : "")}{Regex.Replace(pair.Value.Title, "\\&", "&amp;")}");
                _listMusicFilesRows[pair.Key].SetSubtitle(Regex.Replace(pair.Value.Filename, "\\&", "&amp;"));
            }
            else
            {
                _listMusicFilesRows[pair.Key].SetTitle(Regex.Replace(pair.Value.Filename, "\\&", "&amp;"));
                _listMusicFilesRows[pair.Key].SetSubtitle("");
            }
        }
        _isSelectionOccuring = false;
        return false;
    }

    /// <summary>
    /// Occurs when the _musicFilesSearch's text is changed
    /// </summary>
    /// <param name="sender">Gtk.SearchEntry</param>
    /// <param name="e">EventArgs</param>
    private void SearchChanged(Gtk.SearchEntry sender, EventArgs e)
    {
        var search = _musicFilesSearch.GetText().ToLower();
        if (!string.IsNullOrEmpty(search) && search[0] == '!')
        {
            _advancedSearchInfoButton.SetVisible(true);
            var result = _controller.AdvancedSearch(search);
            if (!result.Success)
            {
                _musicFilesSearch.RemoveCssClass("success");
                _musicFilesSearch.AddCssClass("error");
                _listMusicFiles.SetFilterFunc((row) => true);
            }
            else
            {
                _musicFilesSearch.RemoveCssClass("error");
                _musicFilesSearch.AddCssClass("success");
                _listMusicFiles.SetFilterFunc((row) =>
                {
                    if (result.LowerFilenames!.Count == 0)
                    {
                        return false;
                    }
                    var rowFilename = (row as Adw.ActionRow)!.GetSubtitle() ?? "";
                    if (string.IsNullOrEmpty(rowFilename))
                    {
                        rowFilename = (row as Adw.ActionRow)!.GetTitle();
                    }
                    rowFilename = rowFilename.ToLower();
                    return result.LowerFilenames.Contains(rowFilename);
                });
            }
        }
        else
        {
            _advancedSearchInfoButton.SetVisible(false);
            _musicFilesSearch.RemoveCssClass("success");
            _musicFilesSearch.RemoveCssClass("error");
            _listMusicFiles.SetFilterFunc((row) =>
            {
                var rowFilename = (row as Adw.ActionRow)!.GetSubtitle() ?? "";
                if (string.IsNullOrEmpty(rowFilename))
                {
                    rowFilename = (row as Adw.ActionRow)!.GetTitle();
                }
                rowFilename = rowFilename.ToLower();
                if (string.IsNullOrEmpty(search) || rowFilename.Contains(search))
                {
                    return true;
                }
                return false;
            });
        }
    }

    /// <summary>
    /// Occurs when the _advancedSearchInfoButton is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void AdvancedSearchInfo(Gtk.Button sender, EventArgs e) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("search"), 0);
    
    /// <summary>
    /// Occurs when the _listMusicFiles's selection is changed
    /// </summary>
    /// <param name="sender">Gtk.ListBox</param>
    /// <param name="e">EventArgs</param>
    private void ListMusicFiles_SelectionChanged(Gtk.ListBox sender, EventArgs e)
    {
        _isSelectionOccuring = true;
        var selectedIndexes = sender.GetSelectedRowsIndices();
        _selectedViewStack.SetVisibleChildName(selectedIndexes.Count > 0 ? "Selected" : "NoSelected");
        if (_currentAlbumArtType != AlbumArtType.Front)
        {
            SwitchAlbumArt(null, e);
        }
        _lyricsAction.SetEnabled(selectedIndexes.Count == 1);
        _controller.UpdateSelectedMusicFiles(selectedIndexes);
        if (string.IsNullOrEmpty(_fingerprintLabel.GetLabel()))
        {
            _fingerprintSpinner.SetVisible(true);
            _fingerprintSpinner.SetSpinning(true);
            _copyFingerprintButton.SetVisible(false);
        }
        _isSelectionOccuring = false;
    }

    /// <summary>
    /// Occurs when a tag property's row text is changed
    /// </summary>
    private void TagPropertyChanged()
    {
        if (!_isSelectionOccuring && _controller.SelectedMusicFiles.Count > 0)
        {
            //Update Tags
            var propMap = new PropertyMap()
            {
                Filename = _filenameRow.GetText(),
                Title = _titleRow.GetText(),
                Artist = _artistRow.GetText(),
                Album = _albumRow.GetText(),
                Year = _yearRow.GetText(),
                Track = _trackRow.GetText(),
                TrackTotal = _trackTotalRow.GetText(),
                AlbumArtist = _albumArtistRow.GetText(),
                Genre = _genreRow.GetText(),
                Comment = _commentRow.GetText(),
                BeatsPerMinute = _bpmRow.GetText(),
                Composer = _composerRow.GetText(),
                Description = _descriptionRow.GetText(),
                Publisher = _publisherRow.GetText()
            };
            if (_controller.SelectedMusicFiles.Count == 1)
            {
                foreach (var row in _customPropertyRows)
                {
                    propMap.CustomProperties.Add(row.GetTitle(), row.GetText());
                }
            }
            _controller.UpdateTags(propMap, false);
            //Update Rows
            foreach (var pair in _controller.SelectedMusicFiles)
            {
                if (!string.IsNullOrEmpty(pair.Value.Title))
                {
                    _listMusicFilesRows[pair.Key].SetTitle($"{(pair.Value.Track != 0 ? $"{pair.Value.Track:D2} - " : "")}{Regex.Replace(pair.Value.Title, "\\&", "&amp;")}");
                    _listMusicFilesRows[pair.Key].SetSubtitle(Regex.Replace(pair.Value.Filename, "\\&", "&amp;"));
                }
                else
                {
                    _listMusicFilesRows[pair.Key].SetTitle(Regex.Replace(pair.Value.Filename, "\\&", "&amp;"));
                    _listMusicFilesRows[pair.Key].SetSubtitle("");
                }
            }
        }
    }

    /// <summary>
    /// Occurs when fingerprint is ready to be shown
    /// </summary>
    private bool UpdateFingerprint()
    {
        if (!string.IsNullOrEmpty(_controller.SelectedPropertyMap.Fingerprint) && _controller.SelectedPropertyMap.Fingerprint != _("Calculating..."))
        {
            _fingerprintLabel.SetLabel(_controller.SelectedPropertyMap.Fingerprint);
            _fingerprintSpinner.SetVisible(false);
            _copyFingerprintButton.SetVisible(true);
        }
        return false;
    }

    /// <summary>
    /// Occurs when the _copyFingerprintButton is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void CopyFingerprintToClipboard(Gtk.Button sender, EventArgs e)
    {
        _fingerprintLabel.GetClipboard().SetText(_fingerprintLabel.GetText());
        _toastOverlay.AddToast(Adw.Toast.New(_("Fingerprint was copied to clipboard.")));
    }

    /// <summary>
    /// Occurs when there are corrupted music files found in a music library
    /// </summary>
    private bool CorruptedFilesFound()
    {
        var dialog = new CorruptedFilesDialog(this, _controller.AppInfo.ID, _controller.MusicLibraryName, _controller.CorruptedFiles);
        dialog.Present();
        return false;
    }
}
