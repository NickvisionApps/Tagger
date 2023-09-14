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
            @"* Added the ability to open, manage, and create playlists within Tagger. As a result, the ""Music Folder"" has now become the ""Music Library"" that can be opened from a folder or a supported playlist file
              * Synchronized lyric timestamps will now be shown in the mm:ss.xx format as per the LRC specification. When creating a new lryic, both hh:mm:ss and mm:ss.xx can be specified and Tagger will convert them appropriately
              * Tagger will display headers in the list of music files when sorting to provide a more organized view of files
              * Fixes an issue where lrc files were not importing correctly
              * Tagger's main window size will be remembered and restored on application restart
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
