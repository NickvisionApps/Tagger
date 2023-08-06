using Nickvision.Aura.Network;
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
    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private readonly Gtk.DropTarget _dropTarget;
    private readonly Gio.SimpleAction _applyAction;
    private readonly Gio.SimpleAction _insertAlbumArtAction;
    private readonly Gio.SimpleAction _removeAlbumArtAction;
    private readonly Gio.SimpleAction _exportAlbumArtAction;
    private readonly Gio.SimpleAction _musicBrainzAction;
    private readonly Gio.SimpleAction _acoustIdAction;
    private AlbumArtType _currentAlbumArtType;
    private List<Adw.ActionRow> _listMusicFilesRows;
    private List<Adw.EntryRow> _customPropertyRows;
    private bool _isSelectionOccuring;

    [Gtk.Connect] private readonly Adw.HeaderBar _headerBar;
    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.Button _openFolderButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _flapToggleButton;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _tagActionsButton;
    [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Gtk.Label _loadingLabel;
    [Gtk.Connect] private readonly Gtk.ProgressBar _loadingProgressBar;
    [Gtk.Connect] private readonly Gtk.Label _loadingProgressLabel;
    [Gtk.Connect] private readonly Adw.Flap _folderFlap;
    [Gtk.Connect] private readonly Adw.ViewStack _filesViewStack;
    [Gtk.Connect] private readonly Gtk.SearchEntry _musicFilesSearch;
    [Gtk.Connect] private readonly Gtk.Button _advancedSearchInfoButton;
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
    [Gtk.Connect] private readonly Adw.EntryRow _filenameRow;
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
    [Gtk.Connect] private readonly Gtk.Label _durationLabel;
    [Gtk.Connect] private readonly Gtk.Label _fingerprintLabel;
    [Gtk.Connect] private readonly Gtk.Button _copyFingerprintButton;
    [Gtk.Connect] private readonly Gtk.Spinner _fingerprintSpinner;
    [Gtk.Connect] private readonly Gtk.Label _fileSizeLabel;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        _currentAlbumArtType = AlbumArtType.Front;
        _listMusicFilesRows = new List<Adw.ActionRow>();
        _customPropertyRows = new List<Adw.EntryRow>();
        _isSelectionOccuring = false;
        SetDefaultSize(800, 600);
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
        var musicFilesVadjustment = _scrolledWindowMusicFiles.GetVadjustment();
        musicFilesVadjustment.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "value")
            {
                _searchSeparator.SetVisible(musicFilesVadjustment.GetValue() > 0);
            }
        };
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
        _genreRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
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
        _controller.MusicFolderUpdated += (sender, e) => GLib.Functions.IdleAdd(0, MusicFolderUpdated);
        _controller.MusicFileSaveStatesChanged += (sender, e) => GLib.Functions.IdleAdd(0, () => MusicFileSaveStatesChanged(e));
        _controller.SelectedMusicFilesPropertiesChanged += (sender, e) => GLib.Functions.IdleAdd(0, SelectedMusicFilesPropertiesChanged);
        _controller.FingerprintCalculated += (sender, e) => GLib.Functions.IdleAdd(0, UpdateFingerprint);
        _controller.CorruptedFilesFound += (sender, e) => GLib.Functions.IdleAdd(0, CorruptedFilesFound);
        //Open Folder Action
        var actOpenFolder = Gio.SimpleAction.New("openFolder", null);
        actOpenFolder.OnActivate += async (sender, e) => await OpenFolderAsync();
        AddAction(actOpenFolder);
        application.SetAccelsForAction("win.openFolder", new string[] { "<Ctrl>O" });
        //Close Folder Action
        var actCloseFolder = Gio.SimpleAction.New("closeFolder", null);
        actCloseFolder.OnActivate += (sender, e) => _controller.CloseFolder();
        AddAction(actCloseFolder);
        application.SetAccelsForAction("win.closeFolder", new string[] { "<Ctrl>W" });
        //Reload Folder Action
        var actReloadFolder = Gio.SimpleAction.New("reloadFolder", null);
        actReloadFolder.OnActivate += ReloadFolder;
        AddAction(actReloadFolder);
        application.SetAccelsForAction("win.reloadFolder", new string[] { "F5" });
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
        //Add Custom Property Action
        var actAddCustomProperty = Gio.SimpleAction.New("addCustomProperty", null);
        actAddCustomProperty.OnActivate += (sender, e) => AddCustomProperty();
        AddAction(actAddCustomProperty);
        //Download MusicBrainz Metadata Action
        _musicBrainzAction = Gio.SimpleAction.New("downloadMusicBrainzMetadata", null);
        _musicBrainzAction.OnActivate += DownloadMusicBrainzMetadata;
        AddAction(_musicBrainzAction);
        application.SetAccelsForAction("win.downloadMusicBrainzMetadata", new string[] { "<Ctrl>m" });
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
        SetLoadingState(_("Loading music files from folder..."));
        await _controller.StartupAsync();
        _controller.NetworkMonitor!.StateChanged += (sender, state) =>
        {
            _musicBrainzAction.SetEnabled(state);
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
        if (!_controller.CanClose)
        {
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
            dialog.OnResponse += async (s, ex) =>
            {
                if (dialog.Response == MessageDialogResponse.Suggested)
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(false);
                    Close();
                }
                else if (dialog.Response == MessageDialogResponse.Destructive)
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
        if (obj != null)
        {
            var path = ((Gio.File)obj).GetPath();
            if (Directory.Exists(path))
            {
                SetLoadingState(_("Loading music files from folder..."));
                _controller.OpenFolderAsync(path).Wait();
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
            await _controller.OpenFolderAsync(file.GetPath());
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the reload folder action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void ReloadFolder(Gio.SimpleAction sender, EventArgs e)
    {
        if (!_controller.CanClose)
        {
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
            dialog.OnResponse += async (s, ex) =>
            {
                if (dialog.Response == MessageDialogResponse.Suggested)
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(false);
                }
                if (dialog.Response != MessageDialogResponse.Cancel)
                {
                    SetLoadingState(_("Loading music files from folder..."));
                    await _controller.ReloadFolderAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            SetLoadingState(_("Loading music files from folder..."));
            await _controller.ReloadFolderAsync();
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
        dialog.OnResponse += (s, ex) =>
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
        dialog.OnResponse += (s, ex) =>
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
        filterImages.AddMimeType("image/*");
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
    /// Occurs when the add custom property action is triggered
    /// </summary>
    private void AddCustomProperty()
    {
        var entryDialog = new EntryDialog(this, _controller.AppInfo.ID, _("New Custom Property"), "", _("Property Name"), _("Cancel"), _("Add"));
        entryDialog.OnResponse += (sender, e) =>
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

    /// <summary>
    /// Occurs when the submit to acoustid action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void SubmitToAcoustId(Gio.SimpleAction sender, EventArgs e)
    {
        if (_controller.SelectedMusicFiles.Count > 1)
        {
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Too Many Files Selected"), _("Only one file can be submitted to AcoustID at a time. Please select only one file and try again."), _("OK"));
            dialog.OnResponse += (s, ex) => dialog.Destroy();
            dialog.Present();
            return;
        }
        var entryDialog = new EntryDialog(this, _controller.AppInfo.ID, _("Submit to AcoustId"), _("AcoustId can associate a song's fingerprint with a MusicBrainz Recording Id for easy identification.\n\nIf you have a MusicBrainz Recording Id for this song, please provide it below.\n\nIf none is provided, Tagger will submit your tag's metadata in association with the fingerprint instead."), _("MusicBrainz Recording Id"), _("Cancel"), _("Submit"));
        entryDialog.OnResponse += async (s, ex) =>
        {
            if (!string.IsNullOrEmpty(entryDialog.Response))
            {
                if (!_controller.CanClose)
                {
                    var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
                    dialog.OnResponse += async (ss, exx) =>
                    {
                        if (dialog.Response == MessageDialogResponse.Suggested)
                        {
                            SetLoadingState(_("Saving tags..."));
                            await _controller.SaveAllTagsAsync(false);
                        }
                        if (dialog.Response != MessageDialogResponse.Cancel)
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
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
            dialog.OnResponse += async (ss, exx) =>
            {
                if (dialog.Response != MessageDialogResponse.Cancel)
                {
                    preferencesDialog.Present();
                }
                if (dialog.Response == MessageDialogResponse.Suggested)
                {
                    SetLoadingState(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync(true);
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
        var shortcutsWindow = (Gtk.ShortcutsWindow)builder.GetObject("_shortcuts");
        shortcutsWindow.SetTransientFor(this);
        shortcutsWindow.SetIconName(_controller.AppInfo.ID);
        shortcutsWindow.Present();
    }

    /// <summary>
    /// Occurs when quit action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Quit(Gio.SimpleAction sender, EventArgs e) => _application.Quit();

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
    /// Occurs when the music folder is updated
    /// </summary>
    private bool MusicFolderUpdated()
    {
        _listMusicFiles.UnselectAll();
        foreach (var row in _listMusicFilesRows)
        {
            _listMusicFiles.Remove(row);
        }
        _listMusicFilesRows.Clear();
        if (!string.IsNullOrEmpty(_controller.MusicFolderPath))
        {
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
                _listMusicFiles.Append(row);
                _listMusicFilesRows.Add(row);
            }
            _headerBar.RemoveCssClass("flat");
            _title.SetSubtitle(_controller.MusicFolderPath);
            _openFolderButton.SetVisible(true);
            _applyAction.SetEnabled(false);
            _tagActionsButton.SetSensitive(false);
            _viewStack.SetVisibleChildName("Folder");
            _folderFlap.SetFoldPolicy(_controller.MusicFiles.Count > 0 ? Adw.FlapFoldPolicy.Auto : Adw.FlapFoldPolicy.Always);
            _folderFlap.SetRevealFlap(true);
            _flapToggleButton.SetSensitive(_controller.MusicFiles.Count > 0);
            _filesViewStack.SetVisibleChildName(_controller.MusicFiles.Count > 0 ? "Files" : "NoFiles");
        }
        else
        {
            _headerBar.AddCssClass("flat");
            _title.SetSubtitle("");
            _viewStack.SetVisibleChildName("NoFolder");
            _openFolderButton.SetVisible(false);
        }
        return false;
    }

    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    /// <param name="pending">Whether or not there are unsaved changes</param>
    private bool MusicFileSaveStatesChanged(bool pending)
    {
        _viewStack.SetVisibleChildName("Folder");
        _openFolderButton.SetVisible(true);
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
        _commentRow.SetText(_controller.SelectedPropertyMap.Comment);
        _bpmRow.SetText(_controller.SelectedPropertyMap.BeatsPerMinute);
        _composerRow.SetText(_controller.SelectedPropertyMap.Composer);
        _descriptionRow.SetText(_controller.SelectedPropertyMap.Description);
        _publisherRow.SetText(_controller.SelectedPropertyMap.Publisher);
        _durationLabel.SetLabel(_controller.SelectedPropertyMap.Duration);
        _fingerprintLabel.SetLabel(_controller.SelectedPropertyMap.Fingerprint);
        _fileSizeLabel.SetLabel(_controller.SelectedPropertyMap.FileSize);
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
        if (_controller.SelectedMusicFiles.Count == 1 && _controller.SelectedMusicFiles.First().Value.IsReadOnly)
        {
            _filenameRow.SetEditable(false);
            _titleRow.SetEditable(false);
            _artistRow.SetEditable(false);
            _albumRow.SetEditable(false);
            _yearRow.SetEditable(false);
            _trackRow.SetEditable(false);
            _trackTotalRow.SetEditable(false);
            _albumArtistRow.SetEditable(false);
            _genreRow.SetEditable(false);
            _commentRow.SetEditable(false);
            _bpmRow.SetEditable(false);
            _composerRow.SetEditable(false);
            _descriptionRow.SetEditable(false);
            _publisherRow.SetEditable(false);
            _insertAlbumArtAction.SetEnabled(false);
            _removeAlbumArtAction.SetEnabled(false);
        }
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
                if (_controller.SelectedMusicFiles.First().Value.IsReadOnly)
                {
                    row.SetEditable(false);
                }
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
        _selectedViewStack.SetVisibleChildName(selectedIndexes.Any() ? "Selected" : "NoSelected");
        if (_currentAlbumArtType != AlbumArtType.Front)
        {
            SwitchAlbumArt(null, e);
        }
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
        _fingerprintLabel.SetLabel(_controller.SelectedPropertyMap.Fingerprint);
        _fingerprintSpinner.SetVisible(false);
        _copyFingerprintButton.SetVisible(true);
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
    /// Occurs when there are corrupted music files found in a music folder
    /// </summary>
    private bool CorruptedFilesFound()
    {
        var dialog = new CorruptedFilesDialog(this, _controller.AppInfo.ID, _controller.MusicFolderPath, _controller.CorruptedFiles);
        dialog.Present();
        return false;
    }
}
