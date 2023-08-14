using NickvisionTagger.GNOME.Controls;
using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static NickvisionTagger.Shared.Helpers.Gettext;

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
    private static partial string gtk_text_buffer_get_text(nint buffer, ref TextIter startIter, ref TextIter endIter, [MarshalAs(UnmanagedType.I1)]bool include_hidden_chars);
    
    private readonly LyricsDialogController _controller;
    private readonly string _iconName;
    private readonly List<Adw.EntryRow> _syncRows;

    [Gtk.Connect] private readonly Adw.EntryRow _languageRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.TextView _unsyncTextView;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _syncGroup;
    [Gtk.Connect] private readonly Gtk.Button _addSyncLyricButton;
    [Gtk.Connect] private readonly Gtk.Button _clearSyncButton;
    [Gtk.Connect] private readonly Gtk.Button _importSyncButton;
    [Gtk.Connect] private readonly Gtk.Button _exportSyncButton;
    [Gtk.Connect] private readonly Adw.EntryRow _syncOffsetRow;
    
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
        _iconName = iconName;
        _syncRows = new List<Adw.EntryRow>();
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        OnCloseRequest += OnClose;
        _addSyncLyricButton.OnClicked += AddSyncLyric;
        _clearSyncButton.OnClicked += ClearSyncLyrics;
        _importSyncButton.OnClicked += ImportSyncFromLRC;
        _exportSyncButton.OnClicked += ExportSyncFromLRC;
        _syncOffsetRow.OnApply += (sender, e) =>
        {
            if (int.TryParse(_syncOffsetRow.GetText(), out var offset))
            {
                _controller.SynchronizedLyricsOffset = offset;
            }
            else
            {
                _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset.ToString());
                _syncOffsetRow.SetPosition(-1);
            }
        };
        //Load
        _languageRow.SetText(_controller.LanguageCode);
        _descriptionRow.SetText(_controller.Description);
        _unsyncTextView.GetBuffer().SetText(_controller.UnsynchronizedLyrics, _controller.UnsynchronizedLyrics.Length);
        foreach (var pair in _controller.SynchronizedLyrics.OrderBy(x => x.Key))
        {
            AddSyncLyricRow(pair.Key, pair.Value);
        }
        _syncOffsetRow.SetText(_controller.SynchronizedLyricsOffset.ToString());
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
    
    /// <summary>
    /// Adds a sync lyric row
    /// </summary>
    /// <param name="key">The time of the lyric in ms</param>
    /// <param name="value">The string lyric</param>
    private void AddSyncLyricRow(int key, string value)
    {
        var row = new Adw.EntryRow();
        row.SetTitle(((int)TimeSpan.FromMilliseconds(key).TotalSeconds).ToDurationString());
        row.SetText(value);
        row.SetShowApplyButton(true);
        var delete = new Gtk.Button();
        delete.SetValign(Gtk.Align.Center);
        delete.SetIconName("list-remove-symbolic");
        delete.SetTooltipText(_("Remove Lyric"));
        delete.AddCssClass("flat");
        delete.OnClicked += (sender, e) =>
        {
            if (_controller.SynchronizedLyrics.Remove(key))
            {
                _syncGroup.Remove(row);
                _syncRows.Remove(row);
            }
        };
        row.AddSuffix(delete);
        row.OnApply += (sender, e) => _controller.SynchronizedLyrics[key] = sender.GetText();
        _syncGroup.Add(row);
        _syncRows.Add(row);
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
        var iterStart = new TextIter();
        var iterEnd = new TextIter();
        gtk_text_buffer_get_bounds(_unsyncTextView.GetBuffer().Handle, ref iterStart, ref iterEnd);
        _controller.UnsynchronizedLyrics = gtk_text_buffer_get_text(_unsyncTextView.GetBuffer().Handle, ref iterStart, ref iterEnd, false);
        return false;
    }

    /// <summary>
    /// Occurs when the add synchronized lyric button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void AddSyncLyric(Gtk.Button sender, EventArgs e)
    {
        var entryDialog = new EntryDialog(this, _iconName, _("New Synchronized Lyric"), "", _("Timestamp (hh:mm:ss)"), _("Cancel"), _("Add"))
        {
            Validator = x => TimeSpan.TryParse(x, out var _)
        };
        entryDialog.OnResponse += (s, ex) =>
        {
            if (!string.IsNullOrEmpty(entryDialog.Response))
            {
                var res = TimeSpan.TryParse(entryDialog.Response, out var span);
                if (res)
                {
                    _controller.SynchronizedLyrics[(int)span.TotalMilliseconds] = "";
                    AddSyncLyricRow((int)span.TotalMilliseconds, "");
                }
            }
            entryDialog.Destroy();
        };
        entryDialog.Present();
    }
    
    /// <summary>
    /// Occurs when the clear synchronized lyrics button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void ClearSyncLyrics(Gtk.Button sender, EventArgs e)
    {
        _controller.SynchronizedLyrics.Clear();
        foreach (var row in _syncRows)
        {
            _syncGroup.Remove(row);
        }
        _syncRows.Clear();
    }

    /// <summary>
    /// Occurs when the import synchronized lyrics button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void ImportSyncFromLRC(Gtk.Button sender, EventArgs e)
    {
        
    }
    
    /// <summary>
    /// Occurs when the export synchronized lyrics button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void ExportSyncFromLRC(Gtk.Button sender, EventArgs e)
    {
        
    }
}