using NickvisionTagger.GNOME.Helpers;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System.Collections.Generic;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public partial class CorruptedFilesDialog : Adw.Window
{
    private readonly List<CorruptedMusicFile> _files;

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
    private CorruptedFilesDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string parentPath, List<CorruptedMusicFile> files) : base(builder.GetPointer("_root"), false)
    {
        _files = files;
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _helpButton.OnClicked += (sender, e) => Gtk.Functions.ShowUri(this, DocumentationHelpers.GetHelpURL("corrupted"), 0);
        foreach (var file in _files)
        {
            var row = Adw.ActionRow.New();
            var path = file.Path.Remove(0, parentPath.Length);
            if (path[0] == '/')
            {
                path = path.Remove(0, 1);
            }
            row.SetUseMarkup(false);
            row.SetTitle(path);
            row.SetTitleLines(1);
            row.SetTooltipText(path);
            var button = Gtk.Button.New();
            button.SetIconName("wrench-wide-symbolic");
            button.SetTooltipText(_("Fix File"));
            button.SetValign(Gtk.Align.Center);
            button.AddCssClass("flat");
            button.OnClicked += async (sender, e) =>
            {
                var spinner = Gtk.Spinner.New();
                spinner.SetValign(Gtk.Align.Center);
                spinner.SetSpinning(true);
                row.Remove(button);
                row.AddSuffix(spinner);
                var res = await file.FixAsync();
                var lbl = Gtk.Label.New(res ? _("File fixed successfully") : _("Unable to fix file"));
                lbl.SetValign(Gtk.Align.Center);
                row.Remove(spinner);
                row.AddSuffix(lbl);
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
    public CorruptedFilesDialog(Gtk.Window parent, string iconName, string parentPath, List<CorruptedMusicFile> files) : this(Builder.FromFile("corrupted_files_dialog.ui"), parent, iconName, parentPath, files)
    {
    }
}