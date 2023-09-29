using CommunityToolkit.WinUI.Notifications;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Nickvision.Aura.Taskbar;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using NickvisionTagger.WinUI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.WinUI.Views;

/// <summary>
/// The MainWindow
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly MainWindowController _controller;
    private readonly IntPtr _hwnd;
    private bool _isOpened;
    private bool _isActived;
    private RoutedEventHandler? _notificationButtonClickEvent;

    private enum Monitor_DPI_Type : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
        MDT_Default = MDT_Effective_DPI
    }

    [DllImport("Shcore.dll", SetLastError = true)]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

    public MainWindow(MainWindowController controller)
    {
        InitializeComponent();
        _controller = controller;
        _hwnd = WindowNative.GetWindowHandle(this);
        _isOpened = false;
        _isActived = true;
        //Register Events
        AppWindow.Closing += Window_Closing;
        _controller.NotificationSent += NotificationSent;
        _controller.ShellNotificationSent += ShellNotificationSent;
        _controller.LoadingStateUpdated += (sender, e) => DispatcherQueue.TryEnqueue(() => SetLoadingState(e));
        _controller.LoadingProgressUpdated += (sender, e) => DispatcherQueue.TryEnqueue(() => UpdateLoadingProgress(e));
        //Set TitleBar
        TitleBarTitle.Text = _controller.AppInfo.ShortName;
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        TitlePreview.Text = _controller.AppInfo.IsDevVersion ? _("PREVIEW") : "";
        AppWindow.Title = TitleBarTitle.Text;
        AppWindow.SetIcon(@"Resources\org.nickvision.tagger.ico");
        TitleBar.Loaded += (sender, e) => SetDragRegionForCustomTitleBar();
        TitleBar.SizeChanged += (sender, e) => SetDragRegionForCustomTitleBar();
        //Window Sizing
        AppWindow.Resize(new SizeInt32(900, 700));
        //Home
        HomeBanner.Background = new AcrylicBrush()
        {
            TintOpacity = 0.9,
            TintColor = MainGrid.ActualTheme == ElementTheme.Light ? ColorHelper.FromArgb(255, 225, 120, 0) : ColorHelper.FromArgb(255, 230, 97, 0)
        };
        //Localize Strings
        MenuFile.Title = _("File");
        MenuOpenFolder.Text = _("Open Folder");
        MenuOpenPlaylist.Text = _("Open Playlist");
        MenuReloadLibrary.Text = _("Reload Library");
        MenuExit.Text = _("Exit");
        MenuEdit.Title = _("Edit");
        MenuSettings.Text = _("Settings");
        MenuPlaylist.Title = _("Playlist");
        MenuCreatePlaylist.Text = _("Create Playlist");
        MenuAddToPlaylist.Text = _("Add to Playlist");
        MenuRemoveFromPlaylist.Text = _("Remove from Playlist");
        MenuTag.Title = _("Tag");
        MenuSaveTag.Text = _("Save Tag");
        MenuDiscardChanges.Text = _("Discard Unapplied Changed");
        MenuDeleteTag.Text = _("Delete Tag");
        MenuManageLyrics.Text = _("Manage Lyrics");
        MenuAlbumArt.Text = _("Album Art");
        MenuAlbumArtFront.Text = _("Front");
        MenuAlbumArtFrontInsert.Text = _("Insert");
        MenuAlbumArtFrontRemove.Text = _("Remove");
        MenuAlbumArtFrontExport.Text = _("Export");
        MenuAlbumArtBack.Text = _("Back");
        MenuAlbumArtBackInsert.Text = _("Insert");
        MenuAlbumArtBackRemove.Text = _("Remove");
        MenuAlbumArtBackExport.Text = _("Export");
        MenuConvert.Text = _("Convert");
        MenuFilenameToTag.Text = _("File Name to Tag");
        MenuTagToFilename.Text = _("Tag to File Name");
        MenuWebServices.Text = _("Web Services");
        MenuDownloadMusicBrainz.Text = _("Download MusicBrainz Metadata");
        MenuDownloadLyrics.Text = _("Download Lyrics");
        MenuSubmitToAcoustId.Text = _("Submit to AcoustId");
        MenuHelp.Title = _("Help");
        MenuCheckForUpdates.Text = _("Check for Updates");
        MenuDocumentation.Text = _("Documentation");
        MenuGitHubRepo.Text = _("GitHub Repo");
        MenuReportABug.Text = _("Report a Bug");
        MenuDiscussions.Text = _("Discussions");
        MenuAbout.Text = _("About {0}", _controller.AppInfo.ShortName);
        StatusLabel.Text = _("Ready");
        HomeBannerTitle.Text = _controller.Greeting;
        HomeBannerDescription.Text = _controller.AppInfo.Description;
        HomeGettingStartedTitle.Text = _("Getting Started");
        HomeGettingStartedDescription.Text = _("Open a library with music to get started.");
        HomeOpenFolderButtonLabel.Text = _("Open Folder");
        HomeOpenPlaylistButtonLabel.Text = _("Open Playlist");
        HomeGettingHelpTitle.Text = _("Getting Help");
        HomeGettingHelpDescription.Text = _("Tagger includes online documentation to guide users through its more complicated features.");
        HomeDocumentationButtonLabel.Text = _("Documentation");
        HomeReportABugButtonLabel.Text = _("Report a Bug");
        HomeDiscussionsButtonLabel.Text = _("Discussions");
    }

    /// <summary>
    /// Calls InitializeWithWindow.Initialize on the target object with the MainWindow's hwnd
    /// </summary>
    /// <param name="target">The target object to initialize</param>
    public void InitializeWithWindow(object target) => WinRT.Interop.InitializeWithWindow.Initialize(target, _hwnd);

    /// <summary>
    /// Occurs when the window is activated
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">WindowActivatedEventArgs</param>
    private void Window_Activated(object sender, WindowActivatedEventArgs e)
    {
        _isActived = e.WindowActivationState != WindowActivationState.Deactivated;
        //Update TitleBar
        TitleBarTitle.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        AppWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
    }

    /// <summary>
    /// Occurs when the window is loaded
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if(!_isOpened)
        {
            ViewStack.CurrentPageName = "Startup";
            var accent = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            _controller.TaskbarItem = TaskbarItem.ConnectWindows(_hwnd, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(accent.Color.A, accent.Color.R, accent.Color.G, accent.Color.B)), MainGrid.ActualTheme == ElementTheme.Dark ? System.Drawing.Brushes.Black : System.Drawing.Brushes.White);
            await _controller.StartupAsync();
            _controller.NetworkMonitor!.StateChanged += (sender, state) =>
            {
                MenuDownloadMusicBrainz.IsEnabled = state;
                MenuDownloadLyrics.IsEnabled = state;
                MenuSubmitToAcoustId.IsEnabled = state;
            };
            MainMenu.IsEnabled = true;
            ViewStack.CurrentPageName = "Home";
            _isOpened = true;
        }
    }

    /// <summary>
    /// Occurs when the window is closing
    /// </summary>
    /// <param name="sender">AppWindow</param>
    /// <param name="e">AppWindowClosingEventArgs</param>
    private void Window_Closing(AppWindow sender, AppWindowClosingEventArgs e)
    {
        _controller.Dispose();
    }

    /// <summary>
    /// Occurs when the window's theme is changed
    /// </summary>
    /// <param name="sender">FrameworkElement</param>
    /// <param name="e">object</param>
    private void Window_ActualThemeChanged(FrameworkElement sender, object e)
    {
        //Update TitleBar
        TitleBarTitle.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        MenuFile.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        MenuEdit.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        MenuHelp.Foreground = (SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"];
        AppWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Application.Current.Resources[_isActived ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled"]).Color;
        HomeBanner.Background = HomeBanner.Background = new AcrylicBrush()
        {
            TintOpacity = 0.9,
            TintColor = MainGrid.ActualTheme == ElementTheme.Light ? ColorHelper.FromArgb(255, 225, 120, 0) : ColorHelper.FromArgb(255, 230, 97, 0)
        };
    }

    /// <summary>
    /// Sets the drag region for the TitleBar
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void SetDragRegionForCustomTitleBar()
    {
        var hMonitor = Win32Interop.GetMonitorFromDisplayId(DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd), DisplayAreaFallback.Primary).DisplayId);
        var result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }
        var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        var scaleAdjustment = scaleFactorPercent / 100.0;
        RightPaddingColumn.Width = new GridLength(AppWindow.TitleBar.RightInset / scaleAdjustment);
        LeftPaddingColumn.Width = new GridLength(AppWindow.TitleBar.LeftInset / scaleAdjustment);
        var dragRectsList = new List<RectInt32>();
        RectInt32 dragRectL;
        dragRectL.X = (int)((LeftPaddingColumn.ActualWidth) * scaleAdjustment);
        dragRectL.Y = 0;
        dragRectL.Height = (int)(TitleBar.ActualHeight * scaleAdjustment);
        dragRectL.Width = (int)((IconColumn.ActualWidth
                                + TitleColumn.ActualWidth
                                + LeftDragColumn.ActualWidth) * scaleAdjustment);
        dragRectsList.Add(dragRectL);
        RectInt32 dragRectR;
        dragRectR.X = (int)((LeftPaddingColumn.ActualWidth
                            + IconColumn.ActualWidth
                            + TitleBarTitle.ActualWidth
                            + LeftDragColumn.ActualWidth
                            + MainMenu.ActualWidth) * scaleAdjustment);
        dragRectR.Y = 0;
        dragRectR.Height = (int)(TitleBar.ActualHeight * scaleAdjustment);
        dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
        dragRectsList.Add(dragRectR);
        RectInt32[] dragRects = dragRectsList.ToArray();
        AppWindow.TitleBar.SetDragRectangles(dragRects);
    }

    /// <summary>
    /// Occurs when something is dropped into the window
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DragEventArgs</param>
    private async void OnDrop(object sender, DragEventArgs e)
    {
        if(e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var first = (await e.DataView.GetStorageItemsAsync()).FirstOrDefault();
            if(first != null && MusicLibrary.GetIsValidLibraryPath(first.Path))
            {
                await _controller.OpenLibraryAsync(first.Path);
            }
        }
    }

    /// <summary>
    /// Occurs when something is dragged over into the window
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">DragEventArgs</param>
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy | DataPackageOperation.Link;
        e.DragUIOverride.Caption = _("Drop here to open library");
        e.DragUIOverride.IsGlyphVisible = true;
        e.DragUIOverride.IsContentVisible = true;
        e.DragUIOverride.IsCaptionVisible = true;
    }

    /// <summary>
    /// Occurs when a notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">NotificationSentEventArgs</param>
    private void NotificationSent(object? sender, NotificationSentEventArgs e)
    {
        //InfoBar
        InfoBar.Message = e.Message;
        InfoBar.Severity = e.Severity switch
        {
            NotificationSeverity.Informational => InfoBarSeverity.Informational,
            NotificationSeverity.Success => InfoBarSeverity.Success,
            NotificationSeverity.Warning => InfoBarSeverity.Warning,
            NotificationSeverity.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
        if (_notificationButtonClickEvent != null)
        {
            BtnInfoBar.Click -= _notificationButtonClickEvent;
        }
        if(e.Action == "update")
        {
            _notificationButtonClickEvent = WindowsUpdate;
            BtnInfoBar.Content = _("Update");
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        BtnInfoBar.Visibility = !string.IsNullOrEmpty(e.Action) ? Visibility.Visible : Visibility.Collapsed;
        InfoBar.IsOpen = true;
    }

    /// <summary>
    /// Occurs when a shell notification is sent from the controller
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">ShellNotificationSentEventArgs</param>
    private void ShellNotificationSent(object? sender, ShellNotificationSentEventArgs e) => new ToastContentBuilder().AddText(e.Title).AddText(e.Message).Show();

    /// <summary>
    /// Occurs when the open folder menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void OpenFolder(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker();
        InitializeWithWindow(folderPicker);
        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            SetLoadingState(_("Loading music files from folder..."));
            await _controller.OpenLibraryAsync(folder.Path);
        }
    }

    /// <summary>
    /// Occurs when the exit menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Exit(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Occurs when the settings menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Settings(object sender, RoutedEventArgs e)
    {
        var settingsDialog = new SettingsDialog(_controller.CreatePreferencesViewController())
        {
            XamlRoot = MainGrid.XamlRoot
        };
        await settingsDialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the check for updates menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void CheckForUpdates(object sender, RoutedEventArgs e) => await _controller.CheckForUpdatesAsync();

    /// <summary>
    /// Occurs when the windows update button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void WindowsUpdate(object sender, RoutedEventArgs e)
    {
        InfoBar.IsOpen = false;
        var page = ViewStack.CurrentPageName;
        ViewStack.CurrentPageName = "Startup";
        if(!(await _controller.WindowsUpdateAsync()))
        {
            ViewStack.CurrentPageName = page;
        }
    }

    /// <summary>
    /// Occurs when the documentation menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Documentation(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("index")));

    /// <summary>
    /// Occurs when the github repo menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void GitHubRepo(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.SourceRepo);

    /// <summary>
    /// Occurs when the report a bug menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ReportABug(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.IssueTracker);

    /// <summary>
    /// Occurs when the discussions menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void Discussions(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(_controller.AppInfo.SupportUrl);

    /// <summary>
    /// Occurs when the about menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void About(object sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog(_controller.AppInfo)
        {
            XamlRoot = MainGrid.XamlRoot
        };
        await aboutDialog.ShowAsync();
    }

    /// <summary>
    /// Sets the app into a loading state
    /// </summary>
    /// <param name="message">The message to show on the loading screen</param>
    private void SetLoadingState(string message)
    {
        ViewStack.CurrentPageName = "Loading";
        LblLoading.Text = message;
        ProgLoading.Visibility = Visibility.Collapsed;
        LblLoadingProgress.Visibility = Visibility.Collapsed;
        MainMenu.IsEnabled = false;
    }

    /// <summary>
    /// Updates the progress of the loading state
    /// </summary>
    /// <param name="e">(int Value, int MaxValue, string Message)</param>
    private void UpdateLoadingProgress((int Value, int MaxValue, string Message) e)
    {
        ProgLoading.Visibility = Visibility.Visible;
        LblLoadingProgress.Visibility = Visibility.Visible;
        ProgLoading.Minimum = 0;
        ProgLoading.Maximum = e.MaxValue;
        ProgLoading.Value = e.Value;
        LblLoadingProgress.Text = e.Message;
    }
}
