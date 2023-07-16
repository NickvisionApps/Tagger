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
    private static partial nint gtk_file_launcher_new(nint file);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_launcher_open_containing_folder(nint fileLauncher, nint parent, nint cancellable, GAsyncReadyCallback callback, nint data);

    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _filesGroup;

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="files">List of corrupted files</param>
    private CorruptedFilesDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, List<string> files) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        foreach (var path in files)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(path);
            row.SetTitleLines(1);
            row.SetTooltipText(path);
            var button = new Gtk.Button();
            button.SetIconName("folder-symbolic");
            button.SetTooltipText("Open Folder");
            button.SetValign(Gtk.Align.Center);
            button.AddCssClass("flat");
            button.OnClicked += (sender, e) =>
            {
                var file = Gio.FileHelper.NewForPath(path);
                var fileLauncher = gtk_file_launcher_new(file.Handle);
                gtk_file_launcher_open_containing_folder(fileLauncher, 0, 0, (source, res, data) => { }, 0);
            };
            row.AddSuffix(button);
            row.SetActivatableWidget(button);
            _filesGroup.Add(row);
        }
    }

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="files">List of corrupted files</param>
    public CorruptedFilesDialog(Gtk.Window parent, string iconName, List<string> files) : this(Builder.FromFile("corrupted_files_dialog.ui"), parent, iconName, files)
    {
    }
}