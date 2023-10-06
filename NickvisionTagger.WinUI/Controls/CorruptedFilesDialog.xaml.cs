using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.System;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A dialog showing the list of corrupted files
/// </summary>
public sealed partial class CorruptedFilesDialog : ContentDialog
{
    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="parentPath">Path of the parent directory of corrupted files</param>
    /// <param name="files">List of corrupted files</param>
    public CorruptedFilesDialog(string parentPath, List<string> files)
    {
        InitializeComponent();
        //Localize Strings
        Title = _("Corrupted Files Found");
        CloseButtonText = _("OK");
        LblBtnHelp.Text = _("Help");
        LblMessage.Text = _("This music folder contains music files that have corrupted tags. The following files are affected and will be ignored by Tagger:");
        //Load
        foreach (var path in files)
        {
            var p = path.Remove(0, parentPath.Length);
            if (p[0] == '/')
            {
                p = p.Remove(0, 1);
            }
            var button = new Button()
            {
                Content = new SymbolIcon(Symbol.OpenLocal)
            };
            ToolTipService.SetToolTip(button, _("Open Folder"));
            button.Click += async (sender, e) => await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(path));
            var card = new SettingsCard()
            {
                Header = p,
                Content = button
            };
            ToolTipService.SetToolTip(card, path);
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
