using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Controllers;
using System;
using System.Runtime.InteropServices;

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
    
    private LyricsDialogController _controller;

    [Gtk.Connect] private readonly Adw.EntryRow _languageRow;
    [Gtk.Connect] private readonly Adw.EntryRow _descriptionRow;
    [Gtk.Connect] private readonly Gtk.TextView _unsyncTextView;
    
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
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        OnCloseRequest += OnClose;
        //Load
        _languageRow.SetText(_controller.LanguageCode);
        _descriptionRow.SetText(_controller.Description);
        _unsyncTextView.GetBuffer().SetText(_controller.UnsynchronizedLyrics, _controller.UnsynchronizedLyrics.Length);
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
}