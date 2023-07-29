using NickvisionTagger.GNOME.Helpers;
using System.Collections.Generic;
using static Nickvision.GirExt.GtkExt;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public partial class CorruptedFilesDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Button _helpButton;
    [Gtk.Connect] private readonly Gtk.ScrolledWindow _scrolledWindow;
    [Gtk.Connect] private readonly Adw.PreferencesGroup _filesGroup;

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
        _helpButton.OnClicked += (sender, e) => Gtk.Functions.ShowUri(this, Help.GetHelpURL("corrupted"), 0);
        
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
            button.SetTooltipText(_("Open Folder"));
            button.SetValign(Gtk.Align.Center);
            button.AddCssClass("flat");
            button.OnClicked += async (sender, e) =>
            {
                var file = Gio.FileHelper.NewForPath(path);
                var fileLauncher = Gtk.FileLauncher.New(file);
                try
                {
                    await fileLauncher.OpenContainingFolderAsync(this);
                }
                catch  { }
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
    /// <param name="parentPath">Path of the parent directory of corrupted files</param>
    /// <param name="files">List of corrupted files</param>
    public CorruptedFilesDialog(Gtk.Window parent, string iconName, string parentPath, List<string> files) : this(Builder.FromFile("corrupted_files_dialog.ui"), parent, iconName, parentPath, files)
    {
    }
}