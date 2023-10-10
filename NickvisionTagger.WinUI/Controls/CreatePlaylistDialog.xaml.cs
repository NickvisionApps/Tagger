using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A dialog showing the options in creating a playlist
/// </summary>
public sealed partial class CreatePlaylistDialog : ContentDialog
{
    private readonly Action<object> _initalizeWithWindow;
    private string _path;

    /// <summary>
    /// Constructs a CreatePlaylistDialog
    /// </summary>
    /// <param name="initalizeWithWindow">Action</param>
    public CreatePlaylistDialog(Action<object> initalizeWithWindow)
    {
        InitializeComponent();
        _initalizeWithWindow = initalizeWithWindow;
        _path = "";
        //Localize Strings
        Title = _("Playlist");
        PrimaryButtonText = _("Create");
        CloseButtonText = _("Cancel");
        CardPath.Header = _("Path");
        ToolTipService.SetToolTip(BtnSelectSaveLocation, _("Select Save Location"));
        CardFormat.Header = _("Format");
        CardRelative.Header = _("Use Relative Paths");
        TglRelative.OnContent = _("On");
        TglRelative.OffContent = _("Off");
        CardSelected.Header = _("Include Only Selected Files");
        TglSelected.OnContent = _("On");
        TglSelected.OffContent = _("Off");
        Validate();
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    /// <returns>PlaylistOptions</returns>
    public new async Task<PlaylistOptions?> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return new PlaylistOptions(_path, (PlaylistFormat)CmbFormat.SelectedIndex, TglRelative.IsOn, TglSelected.IsOn);
        }
        return null;
    }

    /// <summary>
    /// Validates the dialog's options
    /// </summary>
    private void Validate()
    {
        if (string.IsNullOrEmpty(_path))
        {
            LblPath.Text = _("No file selected");
        }
        IsPrimaryButtonEnabled = !string.IsNullOrEmpty(_path);
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the select save location button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void SelectSaveLocation(object sender, RoutedEventArgs e)
    {
        var playlistExtensions = Enum.GetValues<PlaylistFormat>().Select(x => x.GetDotExtension()).ToList();
        var filePicker = new FileSavePicker();
        _initalizeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        var exts = new List<string>();
        foreach (var ext in playlistExtensions)
        {
            exts.Add(ext);
            exts.Add(ext.ToUpper());
            if (ext == playlistExtensions[CmbFormat.SelectedIndex])
            {
                filePicker.DefaultFileExtension = ext;
            }
        }
        filePicker.FileTypeChoices.Add(_("All files"), exts);
        var file = await filePicker.PickSaveFileAsync();
        if (file != null)
        {
            _path = file.Path;
            var extIndex = playlistExtensions.IndexOf((Path.GetExtension(_path) ?? "").ToLower());
            if (extIndex != -1)
            {
                CmbFormat.SelectedIndex = extIndex;
            }
            LblPath.Text = Path.GetFileNameWithoutExtension(_path) ?? "";
            Validate();
        }
    }
}
