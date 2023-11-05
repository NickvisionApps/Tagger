using Microsoft.UI.Xaml;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using NickvisionTagger.WinUI.Views;
using System;

namespace NickvisionTagger.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private MainWindow? _mainWindow;
    private MainWindowController _controller;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        _controller = new MainWindowController(Array.Empty<string>());
        _controller.AppInfo.Changelog =
            @"- Added the ability to specify ""\"" in a Tag to File Name format string to move files to a new directory when renaming files
- Tagger will now display files with corrupted album art as corrupted files
- Fixed an issue where some custom properties for vorbis and wav files could not be removed
- Updated translations (Thanks everyone on Weblate!)";
        if (_controller.Theme != Theme.System)
        {
            RequestedTheme = _controller.Theme switch
            {
                Theme.Light => ApplicationTheme.Light,
                Theme.Dark => ApplicationTheme.Dark,
                _ => ApplicationTheme.Light
            };
        }
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow(_controller);
        _mainWindow.Activate();
    }
}
