using NickvisionTagger.GNOME.Controls;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// A dialog for managing lyrics of a music file
/// </summary>
public partial class LyricsDialog : Adw.Window
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TextIter
    {
        public nint dummy1;
        public nint dummy2;
        public int dummy3;
        public int dummy4;
        public int dummy5;
        public int dummy6;
        public int dummy7;
        public int dummy8;
        public nint dummy9;
        public nint dummy10;
        public int dummy11;
        public int dummy12;
        public int dummy13;
        public nint dummy14;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_text_buffer_get_bounds(nint buffer, ref TextIter startIter, ref TextIter endIter);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gtk_text_buffer_get_text(nint buffer, ref TextIter startIter, ref TextIter endIter, [MarshalAs(UnmanagedType.I1)] bool include_hidden_chars);

    private readonly LyricsDialogController _controller;
    private readonly Gtk.Window _parentWindow;
    private readonly string _iconName;
    private readonly Dictionary<int, Adw.EntryRow> _syncRows;

    [Gtk.Connect] private readonly Adw.ToastOverlay _toast;
    [Gtk.Connect] private readonly Adw.ComboRow _typeRow;
    [Gtk.Connect] private readonly Adw.EntryRow _languageRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Adw.EntryRow _syncOffsetRow;
    [Gtk.Connect] private readonly Adw.ViewStack _viewStack;
    [Gtk.Connect] private readonly Gtk.TextView _unsyncTextView;
    [Gtk.Connect] private readonly Gtk.Button _addSyncLyricButton;
    [Gtk.Connect] private readonly Gtk.Button _clearSyncButton;
    [Gtk.Connect] private readonly Gtk.Button _importSyncButton;
    [Gtk.Connect] private readonly Gtk.Button _exportSyncButton;
    [Gtk.Connect] private readonly Gtk.ListBox _syncList;

    /// <summary>
    /// Constructs a LyricsDialog
    /// </summary>
    /// <param name="controller">LyricsDialogController</param>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    private LyricsDialog(Gtk.Builder builder, LyricsDialogController controller, Gtk.Window parent, string iconName) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _parentWindow = parent;
        _iconName = iconName;
        _syncRows = new Dictionary<int, Adw.EntryRow>();
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        OnCloseRequest += OnClose;
        _typeRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                ChangeLyricsType();
            }
        };
        _syncOffsetRow.OnApply += (sender, e) =>
        {
            if (int.TryParse(_syncOffsetRow.GetText(), out var offset))
            {
                _controller.SynchronizedLyricsOffset = offset;
            }
            else
            {
                _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset!.Value.ToString());
                _syncOffsetRow.SetPosition(-1);
            }
        };
        _addSyncLyricButton.OnClicked += AddSyncLyric;
        _clearSyncButton.OnClicked += (sender, e) => _controller.ClearSynchronizedLyrics(); ;
        _importSyncButton.OnClicked += ImportSyncFromLRC;
        _exportSyncButton.OnClicked += ExportSyncToLRC;
        //Events
        _controller.SynchronizedLyricCreated += CreateSyncRow;
        _controller.SynchronizedLyricRemoved += RemoveSyncRow;
    }

    /// <summary>
    /// Constructs a LyricsDialog
    /// </summary>
    /// <param name="controller">LyricsDialogController</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    public LyricsDialog(LyricsDialogController controller, Gtk.Window parent, string iconName) : this(Builder.FromFile("lyrics_dialog.ui"), controller, parent, iconName)
    {

    }

    public new void Present()
    {
        base.Present();
        _controller.Startup();
        _typeRow.SetSelected((uint)_controller.LyricsType);
        _languageRow.SetText(_controller.LanguageCode);
        _descriptionRow.SetText(_controller.Description);
        if (_controller.LyricsType == LyricsType.Unsynchronized)
        {
            _viewStack.SetVisibleChildName("unsync");
            _unsyncTextView.GetBuffer().SetText(_controller.UnsynchronizedLyrics!, _controller.UnsynchronizedLyrics!.Length);
        }
        else
        {
            _syncOffsetRow.SetVisible(true);
            _viewStack.SetVisibleChildName("sync");
            _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset!.Value.ToString());
        }
    }

    public void ChangeLyricsType()
    {
        var newType = (LyricsType)_typeRow.GetSelected();
        if (_controller.LyricsType != newType)
        {
            _controller.LyricsType = newType;
            foreach (var row in _syncRows)
            {
                _syncList.Remove(row.Value);
            }
            _syncRows.Clear();
            _languageRow.SetText(_controller.LanguageCode);
            _descriptionRow.SetText(_controller.Description);
            if (_controller.LyricsType == LyricsType.Unsynchronized)
            {
                _syncOffsetRow.SetVisible(false);
                _viewStack.SetVisibleChildName("unsync");
                _unsyncTextView.GetBuffer().SetText(_controller.UnsynchronizedLyrics!, _controller.UnsynchronizedLyrics!.Length);
            }
            else
            {
                _syncOffsetRow.SetVisible(true);
                _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset!.Value.ToString());
                _viewStack.SetVisibleChildName("sync");
            }
        }
    }

    /// <summary>
    /// Occurs when a sync lyric is created
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SynchronizedLyricsEventArgs</param>
    private void CreateSyncRow(object? sender, SynchronizedLyricsEventArgs e)
    {
        if (!_syncRows.ContainsKey(e.Timestamp))
        {
            var row = new Adw.EntryRow();
            row.SetTitle(e.Timestamp.MillisecondsToTimecode());
            row.SetText(e.Lyric);
            row.SetShowApplyButton(true);
            var delete = new Gtk.Button();
            delete.SetValign(Gtk.Align.Center);
            delete.SetIconName("list-remove-symbolic");
            delete.SetTooltipText(_("Remove Lyric"));
            delete.AddCssClass("flat");
            delete.OnClicked += (s, ex) => _controller.RemoveSynchronizedLyric(e.Timestamp);
            row.AddSuffix(delete);
            row.OnApply += (s, ex) => _controller.SetSynchronizedLyric(e.Timestamp, s.GetText());
            _syncList.SetVisible(true);
            _syncList.Insert(row, e.Position);
            _syncRows.Add(e.Timestamp, row);
        }
    }

    /// <summary>
    /// Occurs when a sync lyric is removed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SynchronizedLyricsEventArgs</param>
    private void RemoveSyncRow(object? sender, SynchronizedLyricsEventArgs e)
    {
        if (_syncRows.ContainsKey(e.Timestamp))
        {
            _syncList.Remove(_syncRows[e.Timestamp]);
            _syncRows.Remove(e.Timestamp);
            _syncList.SetVisible(_syncRows.Count > 0);
        }
    }

    /// <summary>
    /// Gets the text of the unsync text view
    /// </summary>
    /// <returns>The unsynchronized lyrics text</returns>
    private string GetUnsyncText()
    {
        var iterStart = new TextIter();
        var iterEnd = new TextIter();
        gtk_text_buffer_get_bounds(_unsyncTextView.GetBuffer().Handle, ref iterStart, ref iterEnd);
        return gtk_text_buffer_get_text(_unsyncTextView.GetBuffer().Handle, ref iterStart, ref iterEnd, false);
    }

    /// <summary>
    /// Occurs when the window is closed
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private bool OnClose(Gtk.Widget sender, EventArgs e)
    {
        _controller.LanguageCode = _languageRow.GetText();
        _controller.Description = _descriptionRow.GetText();
        _controller.UnsynchronizedLyrics = GetUnsyncText();
        return false;
    }

    /// <summary>
    /// Occurs when the add synchronized lyric button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void AddSyncLyric(Gtk.Button sender, EventArgs e)
    {
        var entryDialog = new EntryDialog(this, _iconName, _("New Synchronized Lyric"), "", _("Timestamp (hh:mm:ss or mm:ss.xx)"), _("Cancel"), _("Add"))
        {
            Validator = x => x.TimecodeToMs() != -1
        };
        entryDialog.OnResponse += (s, ex) =>
        {
            if (!string.IsNullOrEmpty(entryDialog.Response) && entryDialog.Response != "NULL")
            {
                var res = entryDialog.Response.TimecodeToMs();
                if (res != -1)
                {
                    _controller.AddSynchronizedLyric(res);
                }
            }
            entryDialog.Destroy();
        };
        entryDialog.Present();
    }

    /// <summary>
    /// Occurs when the import synchronized lyrics button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void ImportSyncFromLRC(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileDialog.New();
        openFileDialog.SetTitle(_("Import from LRC"));
        var filterLRC = Gtk.FileFilter.New();
        filterLRC.SetName($"LRC (*.lrc)");
        filterLRC.AddPattern("*.lrc");
        filterLRC.AddPattern("*.LRC");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterLRC);
        openFileDialog.SetFilters(filters);
        try
        {
            var file = await openFileDialog.OpenAsync(_parentWindow);
            var messageDialog = Adw.MessageDialog.New(this, _("Existing Lyrics"), _("What would you like Tagger to do with lyrics found from the LRC file that conflict with existing lyrics of the same timestamp?"));
            messageDialog.SetIconName(_iconName);
            messageDialog.AddResponse("keep", _("Keep Tagger's lyrics"));
            messageDialog.SetDefaultResponse("keep");
            messageDialog.SetCloseResponse("keep");
            messageDialog.AddResponse("overwrite", _("Use LRC's lyrics"));
            messageDialog.SetResponseAppearance("overwrite", Adw.ResponseAppearance.Destructive);
            messageDialog.OnResponse += async (s, ex) =>
            {
                if (_controller.ImportFromLRC(file!.GetPath()!, ex.Response == "overwrite"))
                {
                    _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset!.Value.ToString());
                }
                else
                {
                    _toast.AddToast(Adw.Toast.New(_("Unable to import from LRC.")));
                }
                messageDialog.Destroy();
            };
            if (_controller.SynchronizedLyricsCount!.Value == 0)
            {
                messageDialog.Response("overwrite");
            }
            else
            {
                messageDialog.Present();
            }
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the export synchronized lyrics button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private async void ExportSyncToLRC(Gtk.Button sender, EventArgs e)
    {
        if (_controller.SynchronizedLyricsCount!.Value == 0)
        {
            _toast.AddToast(Adw.Toast.New(_("Nothing to export.")));
            return;
        }
        var saveFileDialog = Gtk.FileDialog.New();
        saveFileDialog.SetTitle(_("Export to LRC"));
        var filterLRC = Gtk.FileFilter.New();
        filterLRC.SetName($"LRC (*.lrc)");
        filterLRC.AddPattern("*.lrc");
        filterLRC.AddPattern("*.LRC");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filterLRC);
        saveFileDialog.SetFilters(filters);
        try
        {
            var file = await saveFileDialog.SaveAsync(_parentWindow);
            var path = file!.GetPath()!;
            if (Path.GetExtension(path).ToLower() != ".lrc")
            {
                path += ".lrc";
            }
            if (_controller.ExportToLRC(path))
            {
                _toast.AddToast(Adw.Toast.New(string.Format(_("Exported successfully to: {0}"), path)));
            }
            else
            {
                _toast.AddToast(Adw.Toast.New(_("Unable to export to LRC.")));
            }
        }
        catch { }
    }
}