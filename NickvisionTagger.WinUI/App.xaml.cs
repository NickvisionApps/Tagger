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
            @"- Tagger is now available for Windows using Windows App SDK and WinUI 3
- Added the option to use relative paths when creating a playlist. This means that Tagger also now supports opening playlists with relative paths
- Added the Disc Number, Disc Total, and Publishing Date fields to additional properties
- Added information dialog for album art
- Tagger will now watch a music folder library for changes on disk and prompt the user to reload if necessary
- Tagger will now display front album art within a music file row itself if available
- Tagger will now remember previously used format strings for file name to tag and tag to file name conversions
- Fixed an issue where downloaded lyrics would sometimes contain html encoded characters
- Improved create playlist dialog ux
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
