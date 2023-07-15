using System.Collections.Generic;
using System.Runtime.InteropServices;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog to show info about corrupted files
/// </summary>
public partial class CorruptedFilesDialog
{
    private delegate void GAsyncReadyCallback(nint source, nint res, nint userData);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_launcher_new(nint file);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_file_launcher_open_containing_folder(nint fileLauncher, nint parent, nint cancellable, GAsyncReadyCallback callback, nint data);
    
    private readonly Adw.MessageDialog _dialog;
    
    /// <summary>
    /// Constructs CorruptedFilesDialog
    /// </summary>
    public CorruptedFilesDialog(Gtk.Window parent, string iconName, List<string> files)
    {
        _dialog = Adw.MessageDialog.New(parent, "Corrupted Files Found", _("This music folder contains music files that have corrupted tags. This means that they could be encoded improperly or have structural issues affecting playback.\nTry re-encoding the affected files to fix these issues.\n\nThe following files are affected and will be ignored by Tagger:"));
        _dialog.SetIconName(iconName);
        _dialog.AddResponse("ok", _("Continue"));
        _dialog.SetDefaultResponse("ok");
        var group = Adw.PreferencesGroup.New();
        _dialog.SetExtraChild(group);
        foreach (var path in files)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(path);
            row.SetTitleLines(1);
            var button = Gtk.Button.New();
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
            group.Add(row);
        }
    }
    
    /// <summary>
    /// Presents CorruptedFilesDialog
    /// </summary>
    public void Present() => _dialog.Present();
}