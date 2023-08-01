using NickvisionTagger.GNOME.Views;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME;

/// <summary>
/// The Program 
/// </summary>
public partial class Program
{
    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;
    private MainWindowController _mainWindowController;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program().Run(args);

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program()
    {
        _application = Adw.Application.New("org.nickvision.tagger", Gio.ApplicationFlags.FlagsNone);
        _mainWindow = null;
        _mainWindowController = new MainWindowController();
        _mainWindowController.AppInfo.Changelog =
            @"* Added support for the TrackTotal and BeatsPerMinute tag property
              * Fixed an issue where clearing a tag did not clear all fields
              * Fixed an issue where single album art from other programs was not read by Tagger
              * Fixed an issue where docs were not available when running Tagger via snap
              * Empty Year, Track, and BPM fields will show an empty string instead of 0
              * Updated translations (Thanks everyone on Weblate!)";
        _application.OnActivate += OnActivate;
        if (File.Exists(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.tagger.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.tagger.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
               "/usr"
            };
            foreach (var prefix in prefixes)
            {
                if (File.Exists(prefix + "/share/org.nickvision.tagger/org.nickvision.tagger.gresource"))
                {
                    Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/org.nickvision.tagger/org.nickvision.tagger.gresource")));
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run(string[] args)
    {
        try
        {
            return _application.RunWithSynchronizationContext();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"\n\n{ex.StackTrace}");
            return -1;
        }
    }

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sedner">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private async void OnActivate(Gio.Application sedner, EventArgs e)
    {
        //Set Adw Theme
        _application.StyleManager!.ColorScheme = _mainWindowController.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
        //Main Window
        if (_mainWindow != null)
        {
            _mainWindow.Present();
            return;
        }
        _mainWindow = new MainWindow(_mainWindowController, _application);
        await _mainWindow.StartAsync();
    }
}
