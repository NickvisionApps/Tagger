using NickvisionTagger.GNOME.Controls;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow : Adw.ApplicationWindow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TaggerDateTime
    {
        ulong Usec;
        nint Tz;
        int Interval;
        int Days;
        int RefCount;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_year(ref TaggerDateTime datetime);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_month(ref TaggerDateTime datetime);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int g_date_time_get_day_of_month(ref TaggerDateTime datetime);
    [DllImport("libadwaita-1.so.0")]
    private static extern ref TaggerDateTime g_date_time_new_local(int year, int month, int day, int hour, int minute, double seconds);
    [DllImport("libadwaita-1.so.0")]
    private static extern ref TaggerDateTime gtk_calendar_get_date(nint calendar);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_calendar_select_day(nint calendar, ref TaggerDateTime datetime);

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
    private readonly Gio.SimpleAction _infoAlbumArtAction;
    private readonly Gio.SimpleAction _lyricsAction;
    private readonly Gio.SimpleAction _musicBrainzAction;
    private readonly Gio.SimpleAction _downloadLyricsAction;
    private readonly Gio.SimpleAction _acoustIdAction;
    private readonly Gtk.EventControllerKey _filenameKeyController;
    private AlbumArtType _currentAlbumArtType;
    private string? _listHeader;
    private List<MusicFileRow> _listMusicFilesRows;
    private List<Adw.EntryRow> _customPropertyRows;
    private AutocompleteBox _autocompleteBox;
    private bool _isSelectionOccuring;
    private bool _updatePublishingDate;

    [Gtk.Connect] private readonly Adw.HeaderBar _headerBar;
    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.Button _libraryButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _flapToggleButton;
    [Gtk.Connect] private readonly Gtk.Image _imageLibraryMode;
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
    [Gtk.Connect] private readonly Adw.EntryRow _discNumberRow;
    [Gtk.Connect] private readonly Adw.EntryRow _discTotalRow;
    [Gtk.Connect] private readonly Adw.EntryRow _publisherRow;
    [Gtk.Connect] private readonly Gtk.MenuButton _publishingDateButton;
    [Gtk.Connect] private readonly Gtk.Calendar _publishingDateCalendar;
    [Gtk.Connect] private readonly Gtk.Button _clearPublishingDateButton;
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
        _listMusicFilesRows = new List<MusicFileRow>();
        _customPropertyRows = new List<Adw.EntryRow>();
        _isSelectionOccuring = false;
        _updatePublishingDate = true;
        SetDefaultSize(_controller.WindowWidth, _controller.WindowHeight);
        if (_controller.WindowMaximized)
        {
            Maximize();
        }
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
        _listMusicFiles.SetHeaderFunc((row, before) =>
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
                row.SetHeader(label);
                _listHeader = string.Empty;
            }
        });
        _filenameKeyController = Gtk.EventControllerKey.New();
        _filenameKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _filenameKeyController.OnKeyPressed += OnFilenameKeyPressed;
        _filenameRow.AddController(_filenameKeyController);
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
        _discNumberRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                TagPropertyChanged();
            }
        };
        _discTotalRow.OnNotify += (sender, e) =>
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
        _publishingDateCalendar.OnDaySelected += (sender, e) =>
        {
            if (_updatePublishingDate)
            {
                var glibDate = gtk_calendar_get_date(_publishingDateCalendar.Handle);
                var date = new DateTime(g_date_time_get_year(ref glibDate), g_date_time_get_month(ref glibDate), g_date_time_get_day_of_month(ref glibDate));
                if (date != DateTime.MinValue)
                {
                    _publishingDateButton.SetLabel(date.ToShortDateString());
                    TagPropertyChanged();
                }
                else
                {
                    _updatePublishingDate = false;
                    gtk_calendar_select_day(_publishingDateCalendar.Handle, ref g_date_time_new_local(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0));
                }
            }
            _updatePublishingDate = true;
        };
        _clearPublishingDateButton.OnClicked += (sender, e) =>
        {
            _updatePublishingDate = false;
            _publishingDateButton.SetLabel(_("Pick a date"));
            gtk_calendar_select_day(_publishingDateCalendar.Handle, ref g_date_time_new_local(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0));
            TagPropertyChanged();
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
        _controller.LoadingStateUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () =>
        {
            _viewStack.SetVisibleChildName("Loading");
            _loadingLabel.SetText(e);
            _loadingProgressBar.SetVisible(false);
            _loadingProgressLabel.SetVisible(false);
            _applyAction.SetEnabled(false);
            _tagActionsButton.SetSensitive(false);
            return false;
        });
        _controller.LoadingProgressUpdated += (sender, e) => GLib.Functions.IdleAdd(0, () =>
        {
            _loadingProgressBar.SetVisible(true);
            _loadingProgressLabel.SetVisible(true);
            _loadingProgressBar.SetFraction((double)e.Value / (double)e.MaxValue);
            _loadingProgressLabel.SetLabel(e.Message);
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
        //Insert Album Art Action
        _insertAlbumArtAction = Gio.SimpleAction.New("insertAlbumArt", null);
        _insertAlbumArtAction.OnActivate += async (sender, e) => await InsertAlbumArtAsync(_currentAlbumArtType);
        AddAction(_insertAlbumArtAction);
        //Remove Album Art Action
        _removeAlbumArtAction = Gio.SimpleAction.New("removeAlbumArt", null);
        _removeAlbumArtAction.OnActivate += async (sender, e) => await RemoveAlbumArtAsync(_currentAlbumArtType);
        AddAction(_removeAlbumArtAction);
        //Export Album Art Action
        _exportAlbumArtAction = Gio.SimpleAction.New("exportAlbumArt", null);
        _exportAlbumArtAction.OnActivate += async (sender, e) => await ExportAlbumArtAsync(_currentAlbumArtType);
        AddAction(_exportAlbumArtAction);
        //Info Album Art Action
        _infoAlbumArtAction = Gio.SimpleAction.New("infoAlbumArt", null);
        _infoAlbumArtAction.OnActivate += (sender, e) => AlbumArtInfo(_currentAlbumArtType);
        AddAction(_infoAlbumArtAction);
        //Insert Front Album Art Action
        var actInsertFrontAlbumArt = Gio.SimpleAction.New("insertFrontAlbumArt", null);
        actInsertFrontAlbumArt.OnActivate += async (sender, e) => await InsertAlbumArtAsync(AlbumArtType.Front);
        AddAction(actInsertFrontAlbumArt);
        application.SetAccelsForAction("win.insertFrontAlbumArt", new string[] { "<Ctrl>I" });
        //Remove Front Album Art Action
        var actRemoveFrontAlbumArt = Gio.SimpleAction.New("removeFrontAlbumArt", null);
        actRemoveFrontAlbumArt.OnActivate += async (sender, e) => await RemoveAlbumArtAsync(AlbumArtType.Front);
        AddAction(actRemoveFrontAlbumArt);
        application.SetAccelsForAction("win.removeFrontAlbumArt", new string[] { "<Ctrl>Delete" });
        //Export Front Album Art Action
        var actExportFrontAlbumArt = Gio.SimpleAction.New("exportFrontAlbumArt", null);
        actExportFrontAlbumArt.OnActivate += async (sender, e) => await ExportAlbumArtAsync(AlbumArtType.Front);
        AddAction(actExportFrontAlbumArt);
        application.SetAccelsForAction("win.exportFrontAlbumArt", new string[] { "<Ctrl>E" });
        //Info Front Album Art Action
        var actInfoFrontAlbumArt = Gio.SimpleAction.New("infoFrontAlbumArt", null);
        actInfoFrontAlbumArt.OnActivate += (sender, e) => AlbumArtInfo(AlbumArtType.Front);
        AddAction(actInfoFrontAlbumArt);
        //Insert Back Album Art Action
        var actInsertBackAlbumArt = Gio.SimpleAction.New("insertBackAlbumArt", null);
        actInsertBackAlbumArt.OnActivate += async (sender, e) => await InsertAlbumArtAsync(AlbumArtType.Back);
        AddAction(actInsertBackAlbumArt);
        application.SetAccelsForAction("win.insertBackAlbumArt", new string[] { "<Ctrl><Shift>I" });
        //Remove Back Album Art Action
        var actRemoveBackAlbumArt = Gio.SimpleAction.New("removeBackAlbumArt", null);
        actRemoveBackAlbumArt.OnActivate += async (sender, e) => await RemoveAlbumArtAsync(AlbumArtType.Back);
        AddAction(actRemoveBackAlbumArt);
        application.SetAccelsForAction("win.removeBackAlbumArt", new string[] { "<Ctrl><Shift>Delete" });
        //Export Front Album Art Action
        var actExportBackAlbumArt = Gio.SimpleAction.New("exportBackAlbumArt", null);
        actExportBackAlbumArt.OnActivate += async (sender, e) => await ExportAlbumArtAsync(AlbumArtType.Back);
        AddAction(actExportBackAlbumArt);
        application.SetAccelsForAction("win.exportBackAlbumArt", new string[] { "<Ctrl><Shift>E" });
        //Info Back Album Art Action
        var actInfoBackAlbumArt = Gio.SimpleAction.New("infoBackAlbumArt", null);
        actInfoBackAlbumArt.OnActivate += (sender, e) => AlbumArtInfo(AlbumArtType.Back);
        AddAction(actInfoBackAlbumArt);
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
        actHelp.OnActivate += (sender, e) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("index"), 0);
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
        if (e.Action == "reload")
        {
            toast.SetButtonLabel(_("Reload"));
            toast.OnButtonClicked += (_, _) => ReloadLibrary(_applyAction, e);
        }
        else if (e.Action == "unsupported")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("unsupported"), 0);
        }
        else if (e.Action == "format")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("format-strings"), 0);
        }
        else if (e.Action == "web")
        {
            toast.SetButtonLabel(_("Help"));
            toast.OnButtonClicked += (_, _) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("web-services"), 0);
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
        else if (e.Action == "open-playlist" && File.Exists(e.ActionParam))
        {
            toast.SetButtonLabel(_("Open"));
            toast.OnButtonClicked += async (_, _) =>
            {
                await _controller.OpenLibraryAsync(e.ActionParam);
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
        _controller.WindowMaximized = IsMaximized();
        _controller.SaveConfig();
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
            if (MusicLibrary.GetIsValidLibraryPath(path))
            {
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
        filterAll.SetName(_("All Supported Files"));
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
                    await _controller.SaveAllTagsAsync(false);
                }
                if (ea.Response != "cancel")
                {
                    await _controller.ReloadLibraryAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
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
        var createPlaylistDialog = new CreatePlaylistDialog(this, _controller.AppInfo.ID);
        createPlaylistDialog.OnCreate += (s, po) => _controller.CreatePlaylist(po);
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
                    await _controller.SaveAllTagsAsync(true);
                }
                else if (ea.Response == "discard")
                {
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (ea.Response != "cancel")
                {
                    createPlaylistDialog.Present();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            createPlaylistDialog.Present();
        }
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
        filterAll.SetName(_("All Supported Files"));
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
            var relativeDialog = Adw.MessageDialog.New(this, _("Use Relative Paths?"), _("Would you like to save the added file to the playlist using it's relative path?\nIf not, the full path will be used instead."));
            relativeDialog.SetIconName(_controller.AppInfo.ID);
            relativeDialog.AddResponse("no", _("No"));
            relativeDialog.SetDefaultResponse("no");
            relativeDialog.SetCloseResponse("no");
            relativeDialog.AddResponse("yes", _("Yes"));
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
                        await _controller.SaveAllTagsAsync(false);
                    }
                    if (eaa.Response == "discard")
                    {
                        await _controller.DiscardSelectedUnappliedChangesAsync();
                    }
                    if (eaa.Response != "cancel")
                    {
                        relativeDialog.OnResponse += async (ss, eaa) =>
                        {
                            await _controller.AddFileToPlaylistAsync(file.GetPath(), eaa.Response == "yes");
                            relativeDialog.Destroy();
                        };
                        relativeDialog.Present();

                    }
                    applyDialog.Destroy();
                };
                applyDialog.Present();
            }
            else
            {
                relativeDialog.OnResponse += async (ss, eaa) =>
                {
                    await _controller.AddFileToPlaylistAsync(file.GetPath(), eaa.Response == "yes");
                    relativeDialog.Destroy();
                };
                relativeDialog.Present();
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
                                await _controller.SaveAllTagsAsync(false);
                            }
                            if (eaa.Response == "discard")
                            {
                                await _controller.DiscardSelectedUnappliedChangesAsync();
                            }
                            if (eaa.Response != "cancel")
                            {
                                await _controller.RemoveSelectedFilesFromPlaylistAsync();
                            }
                            applyDialog.Destroy();
                        };
                        applyDialog.Present();
                    }
                    else
                    {
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
    private async void Apply(Gio.SimpleAction sender, EventArgs e) => await _controller.SaveSelectedTagsAsync();

    /// <summary>
    /// Occurs when the discard unapplied changes action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void DiscardUnappliedChanges(Gio.SimpleAction sender, EventArgs e) => await _controller.DiscardSelectedUnappliedChangesAsync();

    /// <summary>
    /// Occurs when the delete tags action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void DeleteTags(Gio.SimpleAction sender, EventArgs e) => await _controller.DeleteSelectedTagsAsync();

    /// <summary>
    /// Occurs when the filename to tag action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void FilenameToTag(Gio.SimpleAction sender, EventArgs e)
    {
        var dialog = new ComboBoxDialog(this, _controller.AppInfo.ID, _("File Name to Tag"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _controller.PreviousFTTFormatString, _("Cancel"), _("Convert"));
        dialog.OnResponse += async (s, ea) =>
        {
            if (!string.IsNullOrEmpty(dialog.Response))
            {
                await _controller.FilenameToTagAsync(dialog.Response);
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
        var dialog = new ComboBoxDialog(this, _controller.AppInfo.ID, _("Tag to File Name"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _controller.PreviousTTFFormatString, _("Cancel"), _("Convert"));
        dialog.OnResponse += async (s, ea) =>
        {
            if (!string.IsNullOrEmpty(dialog.Response))
            {
                await _controller.TagToFilenameAsync(dialog.Response);
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
            await _controller.InsertSelectedAlbumArtAsync(file.GetPath(), type);
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the remove album art action is triggered
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    private async Task RemoveAlbumArtAsync(AlbumArtType type) => await _controller.RemoveSelectedAlbumArtAsync(type);

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
        filter.AddMimeType(_controller.GetFirstAlbumArt(type).MimeType);
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
    /// Occurs wen the album art info action is triggered
    /// </summary>
    /// <param name="type">AlbumArtType</param>
    private void AlbumArtInfo(AlbumArtType type)
    {
        var dialog = new AlbumArtInfoDialog(_controller.GetFirstAlbumArt(type), this, _controller.AppInfo.ID);
        dialog.Present();
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
            if (!string.IsNullOrEmpty(entryDialog.Response) && entryDialog.Response != "NULL")
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
    private async void DownloadMusicBrainzMetadata(Gio.SimpleAction sender, EventArgs e) => await _controller.DownloadMusicBrainzMetadataAsync();

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
                    await _controller.SaveAllTagsAsync(false);
                }
                else if (ea.Response == "discard")
                {
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (ea.Response != "cancel")
                {
                    await _controller.DownloadLyricsAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
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
                            await _controller.SaveAllTagsAsync(false);
                        }
                        if (ea.Response == "discard")
                        {
                            await _controller.DiscardSelectedUnappliedChangesAsync();
                        }
                        if (ea.Response != "cancel")
                        {
                            await _controller.SubmitToAcoustIdAsync(entryDialog.Response == "NULL" ? null : entryDialog.Response);
                        }
                        dialog.Destroy();
                    };
                    dialog.Present();
                }
                else
                {
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
                    await _controller.SaveAllTagsAsync(true);
                }
                else if (ea.Response == "discard")
                {
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
        if (!OnCloseRequested(this, e))
        {
            _application.Quit();
        }
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
            GLib.Internal.MainContext.Iteration(GLib.Internal.MainContext.Default(), false);
        }
        _listMusicFilesRows.Clear();
        if (!string.IsNullOrEmpty(_controller.MusicLibraryName))
        {
            string? comparable = null;
            _loadingLabel.SetLabel(_("Making everything pretty..."));
            foreach (var musicFile in _controller.MusicFiles)
            {
                var row = new MusicFileRow(musicFile);
                var header = _controller.GetHeaderForMusicFile(musicFile);
                if (comparable != header)
                {
                    comparable = header;
                    _listHeader = header;
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
                GLib.Internal.MainContext.Iteration(GLib.Internal.MainContext.Default(), false);
                _listMusicFiles.Append(row);
                _listMusicFilesRows.Add(row);
                GLib.Internal.MainContext.Iteration(GLib.Internal.MainContext.Default(), false);
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
            _imageLibraryMode.SetFromIconName(_controller.MusicLibraryType == MusicLibraryType.Folder ? "folder-visiting-symbolic" : "playlist-symbolic");
            _imageLibraryMode.SetTooltipText(_controller.MusicLibraryType == MusicLibraryType.Folder ? _("Folder Mode") : _("Playlist Mode"));
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
            _libraryButton.SetVisible(false);
            _imageLibraryMode.SetFromIconName("");
            _imageLibraryMode.SetTooltipText("");
            _viewStack.SetVisibleChildName("NoLibrary");
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
            _listMusicFilesRows[i].SetUnsaved(!saved);
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
        _discNumberRow.SetText(_controller.SelectedPropertyMap.DiscNumber);
        _discTotalRow.SetText(_controller.SelectedPropertyMap.DiscTotal);
        _publisherRow.SetText(_controller.SelectedPropertyMap.Publisher);
        if (string.IsNullOrEmpty(_controller.SelectedPropertyMap.PublishingDate))
        {
            gtk_calendar_select_day(_publishingDateCalendar.Handle, ref g_date_time_new_local(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, 0, 0, 0));
            _publishingDateButton.SetLabel(_("Pick a date"));
        }
        else if (_controller.SelectedPropertyMap.PublishingDate == _("<keep>"))
        {
            gtk_calendar_select_day(_publishingDateCalendar.Handle, ref g_date_time_new_local(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, 0, 0, 0));
            _publishingDateButton.SetLabel(_("<keep>"));
        }
        else
        {
            var dateTime = DateTime.Parse(_controller.SelectedPropertyMap.PublishingDate);
            gtk_calendar_select_day(_publishingDateCalendar.Handle, ref g_date_time_new_local(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0));
            _publishingDateButton.SetLabel(_controller.SelectedPropertyMap.PublishingDate);
        }
        _durationFileSizeLabel.SetLabel($"{_controller.SelectedPropertyMap.Duration} • {_controller.SelectedPropertyMap.FileSize}");
        _fingerprintLabel.SetLabel(_controller.SelectedPropertyMap.Fingerprint);
        var albumArt = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedPropertyMap.FrontAlbumArt : _controller.SelectedPropertyMap.BackAlbumArt;
        if (albumArt == "hasArt")
        {
            _artViewStack.SetVisibleChildName("Image");
            var art = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedMusicFiles.First().Value.FrontAlbumArt : _controller.SelectedMusicFiles.First().Value.BackAlbumArt;
            if (art.IsEmpty)
            {
                _albumArtImage.SetPaintable(null);
            }
            else
            {
                using var bytes = GLib.Bytes.New(art.Image.AsSpan());
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
        _infoAlbumArtAction.SetEnabled(albumArt == "hasArt");
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
            _listMusicFilesRows[pair.Key].Update(pair.Value);
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
    private void AdvancedSearchInfo(Gtk.Button sender, EventArgs e) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("search"), 0);

    /// <summary>
    /// Occurs when a key is pressed on the filename entry
    /// </summary>
    /// <param name="sender">Gtk.EventControllerKey</param>
    /// <param name="e">Gtk.EventControllerKey.KeyPressedSignalArgs</param>
    private bool OnFilenameKeyPressed(Gtk.EventControllerKey sender, Gtk.EventControllerKey.KeyPressedSignalArgs e)
    {
        var res = e.Keyval == 0x2f; // '/'
        if (!res && _controller.LimitFilenameCharacters)
        {
            res = e.Keyval switch
            {
                0x22 or 0x3c or 0x3e or 0x3a or 0x5c or 0x7c or 0x3f or 0x2a => true, // '"', '<', '>', ':', '\\', '|', '?', '*'
                _ => false
            };
        }
        return res;
    }

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
                DiscNumber = _discNumberRow.GetText(),
                DiscTotal = _discTotalRow.GetText(),
                Publisher = _publisherRow.GetText(),
                PublishingDate = _publishingDateButton.GetLabel() == _("Pick a date") ? "" : _publishingDateButton.GetLabel()
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
                _listMusicFilesRows[pair.Key].Update(pair.Value);
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
