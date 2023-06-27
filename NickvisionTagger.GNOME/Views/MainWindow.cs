using NickvisionTagger.GNOME.Controls;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow : Adw.ApplicationWindow
{
    private delegate bool GSourceFunc(nint data);
    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_invoke(nint context, GSourceFunc function, nint data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool may_block);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_new();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_set_title(nint dialog, string title);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_dialog_select_folder(nint dialog, nint parent, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_dialog_select_folder_finish(nint dialog, nint result, nint error);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_file_new_for_path(string path);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_file_icon_new(nint gfile);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_notification_set_icon(nint notification, nint icon);

    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private GAsyncReadyCallback? _saveCallback;
    private readonly Gtk.DropTarget _dropTarget;
    private List<Adw.ActionRow> _listMusicFilesRows;
    private readonly GSourceFunc _musicFolderUpdatedFunc;
    private readonly GSourceFunc _musicFileSaveStatesFunc;

    [Gtk.Connect] private readonly Adw.HeaderBar _headerBar;
    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.Button _openFolderButton;
    [Gtk.Connect] private readonly Gtk.Button _reloadFolderButton;
    [Gtk.Connect] private readonly Gtk.Separator _headerEndSeparator;
    [Gtk.Connect] private readonly Gtk.Button _applyButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _tagActionsButton;
    [Gtk.Connect] private readonly Gtk.MenuButton _webServicesButton;
    [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Gtk.Label _loadingLabel;
    [Gtk.Connect] private readonly Adw.ViewStack _filesViewStack;
    [Gtk.Connect] private readonly Gtk.SearchEntry _musicFilesSearch;
    [Gtk.Connect] private readonly Gtk.Button _advancedSearchInfoButton;
    [Gtk.Connect] private readonly Gtk.ListBox _listMusicFiles;
    [Gtk.Connect] private readonly Adw.ViewStack _artViewStack;
    [Gtk.Connect] private readonly Gtk.Button _noAlbumArtButton;
    [Gtk.Connect] private readonly Gtk.Button _albumArtButton;
    [Gtk.Connect] private readonly Gtk.Image _albumArtImage;
    [Gtk.Connect] private readonly Gtk.Button _keepAlbumArtButton;
    [Gtk.Connect] private readonly Adw.EntryRow _filenameRow;
    [Gtk.Connect] private readonly Adw.EntryRow _titleRow;
    [Gtk.Connect] private readonly Adw.EntryRow _artistRow;
    [Gtk.Connect] private readonly Adw.EntryRow _albumRow;
    [Gtk.Connect] private readonly Adw.EntryRow _yearRow;
    [Gtk.Connect] private readonly Adw.EntryRow _trackRow;
    [Gtk.Connect] private readonly Adw.EntryRow _albumArtistRow;
    [Gtk.Connect] private readonly Adw.EntryRow _genreRow;
    [Gtk.Connect] private readonly Adw.EntryRow _commentRow;
    [Gtk.Connect] private readonly Adw.EntryRow _durationRow;
    [Gtk.Connect] private readonly Adw.EntryRow _fingerprintRow;
    [Gtk.Connect] private readonly Adw.EntryRow _fileSizeRow;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        _saveCallback = null;
        _listMusicFilesRows = new List<Adw.ActionRow>();
        _musicFolderUpdatedFunc =  MusicFolderUpdated;
        _musicFileSaveStatesFunc = (x) => MusicFileSaveStatesChanged();
        SetDefaultSize(800, 600);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        if (_controller.IsDevVersion)
        {
            AddCssClass("devel");
        }
        //Build UI
        builder.Connect(this);
        _title.SetTitle(_controller.AppInfo.ShortName);
        //Register Events
        OnCloseRequest += OnCloseRequested;
        _controller.NotificationSent += NotificationSent;
        _controller.ShellNotificationSent += ShellNotificationSent;
        _controller.MusicFolderUpdated += (sender, e) => g_main_context_invoke(0, _musicFolderUpdatedFunc, (IntPtr)GCHandle.Alloc(e));
        _controller.MusicFileSaveStatesChanged += (sender, e) => g_main_context_invoke(0, _musicFileSaveStatesFunc, 0);
        //Open Folder Action
        var actOpenFolder = Gio.SimpleAction.New("openFolder", null);
        actOpenFolder.OnActivate += OpenFolder;
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
        var actApply = Gio.SimpleAction.New("apply", null);
        actApply.OnActivate += Apply;
        AddAction(actApply);
        application.SetAccelsForAction("win.apply", new string[] { "<Ctrl>S" });
        //Discard Unapplied Changes Action
        var actDiscard = Gio.SimpleAction.New("discardUnappliedChanges", null);
        actDiscard.OnActivate += DiscardUnappliedChanges;
        AddAction(actDiscard);
        application.SetAccelsForAction("win.discardUnappliedChanges", new string[] { "<Ctrl>Z" });
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
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        AddAction(actAbout);
        application.SetAccelsForAction("win.about", new string[] { "F1" });
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
        _viewStack.SetVisibleChildName("Loading");
        _loadingLabel.SetText(_("Loading music files from folder..."));
        await _controller.StartupAsync();
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e)
    {
        var toast = Adw.Toast.New(e.Message);
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
            var iconHandle = g_file_icon_new(g_file_new_for_path($"{Environment.GetEnvironmentVariable("SNAP")}/usr/share/icons/hicolor/symbolic/apps/{_controller.AppInfo.ID}-symbolic.svg"));
            g_notification_set_icon(notification.Handle, iconHandle);
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
        if (!_controller.CanClose)
        {
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
            dialog.OnResponse += async (s, ex) =>
            {
                if(dialog.Response == MessageDialogResponse.Suggested)
                {
                    _viewStack.SetVisibleChildName("Loading");
                    _loadingLabel.SetText(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync();
                    Close();
                }
                else if(dialog.Response == MessageDialogResponse.Destructive)
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
            var path = g_file_get_path(obj.Handle);
            if (Directory.Exists(path))
            {
                _viewStack.SetVisibleChildName("Loading");
                _loadingLabel.SetText(_("Loading music files from folder..."));
                _controller.OpenFolderAsync(path).Wait();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Occurs when the open folder action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void OpenFolder(Gio.SimpleAction sender, EventArgs e)
    {
        var folderDialog = gtk_file_dialog_new();
        gtk_file_dialog_set_title(folderDialog, _("Open Folder"));
        _saveCallback = async (source, res, data) =>
        {
            var fileHandle = gtk_file_dialog_select_folder_finish(folderDialog, res, IntPtr.Zero);
            if (fileHandle != IntPtr.Zero)
            {
                var path = g_file_get_path(fileHandle);
                _viewStack.SetVisibleChildName("Loading");
                _loadingLabel.SetText(_("Loading music files from folder..."));
                await _controller.OpenFolderAsync(path);
            }
        };
        gtk_file_dialog_select_folder(folderDialog, Handle, IntPtr.Zero, _saveCallback, IntPtr.Zero);
    }

    /// <summary>
    /// Occurs when the reload folder action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void ReloadFolder(Gio.SimpleAction sender, EventArgs e)
    {
        if(!_controller.CanClose)
        {
            var dialog = new MessageDialog(this, _controller.AppInfo.ID, _("Apply Changes?"), _("Some music files still have changes waiting to be applied. What would you like to do?"), _("Cancel"), _("Discard"), _("Apply"));
            dialog.OnResponse += async (s, ex) =>
            {
                if(dialog.Response == MessageDialogResponse.Suggested)
                {
                    _viewStack.SetVisibleChildName("Loading");
                    _loadingLabel.SetText(_("Saving tags..."));
                    await _controller.SaveAllTagsAsync();
                }
                if(dialog.Response != MessageDialogResponse.Cancel)
                {
                    _viewStack.SetVisibleChildName("Loading");
                    _loadingLabel.SetText(_("Loading music files from folder..."));
                    await _controller.ReloadFolderAsync();
                }
                dialog.Destroy();
            };
            dialog.Present();
        }
        else
        {
            _viewStack.SetVisibleChildName("Loading");
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
        _viewStack.SetVisibleChildName("Loading");
        _loadingLabel.SetText(_("Saving tags..."));
        await _controller.SaveSelectedTagsAsync();
    }

    /// <summary>
    /// Occurs when the discard unapplied changes action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private async void DiscardUnappliedChanges(Gio.SimpleAction sender, EventArgs e)
    {
        _viewStack.SetVisibleChildName("Loading");
        _loadingLabel.SetText(_("Discarding unapplied changes..."));
        await _controller.DiscardSelectedUnappliedChanges();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.CreatePreferencesViewController(), _application, this);
        preferencesDialog.Present();
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
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.GetIsDevelVersion() ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetDebugInfo(debugInfo.ToString());
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2021-2023");
        dialog.SetWebsite("https://nickvision.org/");
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_("GitHub Repo"), _controller.AppInfo.GitHubRepo.ToString());
        dialog.AddLink(_("Matrix Chat"), "https://matrix.to/#/#nickvision:matrix.org");
        dialog.SetDevelopers(_("Nicholas Logozzo {0}\nContributors on GitHub ❤️ {1}", "https://github.com/nlogozzo", "https://github.com/NickvisionApps/Denaro/graphs/contributors").Split("\n"));
        dialog.SetDesigners(_("Nicholas Logozzo {0}\nFyodor Sobolev {1}\nDaPigGuy {2}", "https://github.com/nlogozzo", "https://github.com/fsobolev", "https://github.com/DaPigGuy").Split("\n"));
        dialog.SetArtists(_("David Lapshin {0}", "https://github.com/daudix-UFO").Split("\n"));
        dialog.SetTranslatorCredits(_("translator-credits"));
        dialog.SetReleaseNotes(_controller.AppInfo.Changelog);
        dialog.Present();
    }

    /// <summary>
    /// Occurs when the music folder is updated
    /// </summary>
    /// <param name="data">bool on whether or not to send a toast of loaded files</param>
    private bool MusicFolderUpdated(nint data)
    {
        var handle = GCHandle.FromIntPtr(data);
        var target = (bool?)handle.Target;
        if(target != null)
        {
            var sendToast = target.Value;
            foreach(var row in _listMusicFilesRows)
            {
                _listMusicFiles.Remove(row);
            }
            _listMusicFilesRows.Clear();
            if(!string.IsNullOrEmpty(_controller.MusicFolderPath))
            {
                foreach(var musicFile in _controller.MusicFiles)
                {
                    var row = Adw.ActionRow.New();
                    row.SetTitle(Regex.Replace(musicFile.Filename, "\\&", "&amp;"));
                    _listMusicFiles.Append(row);
                    _listMusicFilesRows.Add(row);
                }
                _headerBar.RemoveCssClass("flat");
                _title.SetSubtitle(_controller.MusicFolderPath);
                _openFolderButton.SetVisible(true);
                _reloadFolderButton.SetVisible(true);
                _applyButton.SetVisible(false);
                _tagActionsButton.SetVisible(false);
                _webServicesButton.SetVisible(false);
                _viewStack.SetVisibleChildName("Folder");
                _filesViewStack.SetVisibleChildName(_controller.MusicFiles.Count > 0 ? "Files" : "NoFiles");
                if(sendToast)
                {
                    _toastOverlay.AddToast(Adw.Toast.New(string.Format(_("Loaded {0} music files."), _controller.MusicFiles.Count)));
                }
            }
            else
            {
                _headerBar.AddCssClass("flat");
                _title.SetSubtitle(null);
                _openFolderButton.SetVisible(false);
                _reloadFolderButton.SetVisible(false);
                _applyButton.SetVisible(false);
                _tagActionsButton.SetVisible(false);
                _webServicesButton.SetVisible(false);
                _viewStack.SetVisibleChildName("NoFolder");
            }
        }
        handle.Free();
        return false;
    }

    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    private bool MusicFileSaveStatesChanged()
    {
        _viewStack.SetVisibleChildName("Folder");
        var i = 0;
        foreach(var saved in _controller.MusicFileSaveStates)
        {
            _listMusicFilesRows[i].SetIconName(!saved ? "document-modified-symbolic" : "");
            i++;
        }
        return false;
    }
}
