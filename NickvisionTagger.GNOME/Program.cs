using NickvisionTagger.GNOME.Views;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
    public static int Main(string[] args) => new Program(args).Run();

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program(string[] args)
    {
        _application = Adw.Application.New("org.nickvision.tagger", Gio.ApplicationFlags.FlagsNone);
        _mainWindow = null;
        _mainWindowController = new MainWindowController(args);
        _mainWindowController.AppInfo.Changelog =
            @"* Added the option to use relative paths when creating a playlist. This means that Tagger also now supports opening playlists with relative paths
              * Added the Disc Number, Disc Total, and Publishing Date fields to additional properties
              * Added information dialog for album art
              * Added an option in Preferences to limit file name characters to those only supported by Windows
              * Tagger will now watch a music folder library for changes on disk and prompt the user to reload if necessary
              * Tagger will now display front album art within a music file row itself if available
              * Fixed an issue where downloaded lyrics would sometimes contain html encoded characters
              * Fixed an issue where file names containing the ""<"" character caused the music file row to not display
              * Improved create playlist dialog ux
              * Updated translations (Thanks everyone on Weblate!)";
        _application.OnActivate += OnActivate;
        if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.tagger.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.tagger.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
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
        if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/gdk-pixbuf-2.0/2.10.0/loaders/loaders.cache"))
        {
            GdkPixbuf.Pixbuf.InitModules(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/gdk-pixbuf-2.0/2.10.0/loaders/");
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run()
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
    /// <param name="sender">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private async void OnActivate(Gio.Application sender, EventArgs e)
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
