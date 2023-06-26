using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Models;
using System;
using System.IO;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    /// <summary>
    /// The path of the folder opened
    /// </summary>
    public string FolderPath { get; private set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// Whether or not the folder is opened
    /// </summary>
    public bool IsFolderOpened => FolderPath != "No Folder Opened";

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when a shell notification is sent
    /// </summary>
    public event EventHandler<ShellNotificationSentEventArgs>? ShellNotificationSent;
    /// <summary>
    /// Occurs when a folder is opened or closed
    /// </summary>
    public event EventHandler? FolderChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        FolderPath = "No Folder Opened";
    }

    /// <summary>
    /// The string for greeting on the home page
    /// </summary>
    public string Greeting
    {
        get
        {
            return DateTime.Now.Hour switch
            {
                >= 0 and < 6 => _p("Night", "Good Morning!"),
                < 12 => _p("Morning", "Good Morning!"),
                < 18 => _("Good Afternoon!"),
                < 24 => _("Good Evening!"),
                _ => _("Good Day!")
            };
        }
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Opens a folder
    /// </summary>
    /// <param name="folderPath">The path of the folder to open</param>
    /// <returns>True if folder opened, else false</returns>
    public bool OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            FolderPath = folderPath;
            NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Folder Opened: {0}", FolderPath), NotificationSeverity.Success, "close"));
            FolderChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Closes the folder
    /// </summary>
    public void CloseFolder()
    {
        FolderPath = "No Folder Opened";
        NotificationSent?.Invoke(this, new NotificationSentEventArgs(_("Folder closed."), NotificationSeverity.Warning));
        FolderChanged?.Invoke(this, EventArgs.Empty);
    }
}
