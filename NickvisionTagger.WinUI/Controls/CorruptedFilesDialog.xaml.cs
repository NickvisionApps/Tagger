using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using Windows.System;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public sealed partial class CorruptedFilesDialog : ContentDialog
{
    private readonly List<CorruptedMusicFile> _files;

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="parentPath">Path of the parent directory of corrupted files</param>
    /// <param name="files">List of corrupted files</param>
    public CorruptedFilesDialog(string parentPath, List<CorruptedMusicFile> files)
    {
        InitializeComponent();
        _files = files;
        //Localize Strings
        Title = _("Corrupted Files Found");
        CloseButtonText = _("OK");
        LblBtnHelp.Text = _("Help");
        LblMessage.Text = _("This music folder contains music files that have corrupted tags. The following files are affected and will be ignored by Tagger:");
        //Load
        foreach (var file in _files)
        {
            var path = file.Path.Remove(0, parentPath.Length);
            if (path[0] == '\\')
            {
                path = path.Remove(0, 1);
            }
            var button = new Button()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Content = new SymbolIcon(Symbol.Repair)
            };
            ToolTipService.SetToolTip(button, _("Fix File"));
            var card = new SettingsCard()
            {
                Header = path,
                Content = button
            };
            button.Click += async (sender, e) =>
            {
                card.Content = new ProgressRing()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    IsActive = true
                };
                var res = await file.FixAsync();
                card.Content = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = res ? _("File fixed successfully") : _("Unable to fix file")
                };
            };
            ToolTipService.SetToolTip(card, file.Path);
            ListFiles.Children.Add(card);
        }
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the help button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Help(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("corrupted")));
}
