using NickvisionTagger.GNOME.Helpers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

﻿namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public partial class CorruptedFilesDialog : Adw.Window
{
    private delegate void GAsyncReadyCallback(nint source, nint res, nint userData);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_uri_launcher_launch(nint uriLauncher, nint parent, nint cancellable, GAsyncReadyCallback callback, nint data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_launcher_open_containing_folder(nint fileLauncher, nint parent, nint cancellable, GAsyncReadyCallback callback, nint data);

    [Gtk.Connect] private readonly Gtk.Button _helpButton;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _filesGroup;

    private readonly Gtk.ShortcutController _shortcutController;

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="parentPath">Path of the parent directory of corrupted files</param>
    /// <param name="files">List of corrupted files</param>
    private CorruptedFilesDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string parentPath, List<string> files) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _helpButton.OnClicked += (sender, e) =>
        {
            var uriLauncher = Gtk.UriLauncher.New("help:tagger/corrupted");
            gtk_uri_launcher_launch(uriLauncher.Handle, 0, 0, (source, res, data) => { }, 0);
        };
        foreach (var path in files)
        {
            var row = Adw.ActionRow.New();
            var p = path.Remove(0, parentPath.Length);
            if(p[0] == '/')
            {
                p = p.Remove(0, 1);
            }
            row.SetTitle(p);
            row.SetTitleLines(1);
            row.SetTooltipText(path);
            var button = Gtk.Button.New();
            button.SetIconName("folder-symbolic");
            button.SetTooltipText("Open Folder");
            button.SetValign(Gtk.Align.Center);
            button.AddCssClass("flat");
            button.OnClicked += (sender, e) =>
            {
                var file = Gio.FileHelper.NewForPath(path);
                var fileLauncher = Gtk.FileLauncher.New(file);
                gtk_file_launcher_open_containing_folder(fileLauncher.Handle, 0, 0, (source, res, data) => { }, 0);
            };
            row.AddSuffix(button);
            row.SetActivatableWidget(button);
            _filesGroup.Add(row);
        }
        //Shortcut Controller
        _shortcutController = Gtk.ShortcutController.New();
        _shortcutController.SetScope(Gtk.ShortcutScope.Managed);
        _shortcutController.AddShortcut(Gtk.Shortcut.New(Gtk.ShortcutTrigger.ParseString("Escape"), Gtk.CallbackAction.New(OnEscapeKey)));
        AddController(_shortcutController);
    }

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="parentPath">Path of the parent directory of corrupted files</param>
    /// <param name="files">List of corrupted files</param>
    public CorruptedFilesDialog(Gtk.Window parent, string iconName, string parentPath, List<string> files) : this(Builder.FromFile("corrupted_files_dialog.ui"), parent, iconName, parentPath, files)
    {
    }

    /// <summary>
    /// Occurs when the escape key is pressed on the window
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">GLib.Variant</param>
    private bool OnEscapeKey(Gtk.Widget sender, GLib.Variant e)
    {
        Close();
        return true;
    }
}