using CommunityToolkit.WinUI.Notifications;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Nickvision.Aura.Events;
using Nickvision.Aura.Taskbar;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.Shared.Models;
using NickvisionTagger.WinUI.Controls;
using NickvisionTagger.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Vanara.Extensions.Reflection;
using Vanara.PInvoke;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using WinRT.Interop;
using static Nickvision.Aura.Localization.Gettext;

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
    private List<MusicFileRow> _musicFileRows;
    private AlbumArtType _currentAlbumArtType;

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
        _musicFileRows = new List<MusicFileRow>();
        _currentAlbumArtType = AlbumArtType.Front;
        //Register Events
        AppWindow.Closing += Window_Closing;
        _controller.NotificationSent += (sender, e) => DispatcherQueue.TryEnqueue(() => NotificationSent(sender, e));
        _controller.ShellNotificationSent += ShellNotificationSent;
        _controller.LoadingStateUpdated += (sender, e) => DispatcherQueue.TryEnqueue(() =>
        {
            ViewStack.CurrentPageName = "Loading";
            LblLoading.Text = e;
            ProgLoading.Visibility = Visibility.Collapsed;
            LblLoadingProgress.Visibility = Visibility.Collapsed;
            MainMenu.IsEnabled = false;
        });
        _controller.LoadingProgressUpdated += (sender, e) => DispatcherQueue.TryEnqueue(() =>
        {
            ProgLoading.Visibility = Visibility.Visible;
            LblLoadingProgress.Visibility = Visibility.Visible;
            ProgLoading.Minimum = 0;
            ProgLoading.Maximum = e.MaxValue;
            ProgLoading.Value = e.Value;
            LblLoadingProgress.Text = e.Message;
        });
        _controller.MusicLibraryUpdated += (sender, e) => DispatcherQueue.TryEnqueue(MusicLibraryUpdated);
        _controller.MusicFileSaveStatesChanged += (sender, e) => DispatcherQueue.TryEnqueue(() => MusicFileSaveStatesChanged(e));
        _controller.SelectedMusicFilesPropertiesChanged += (sender, e) => DispatcherQueue.TryEnqueue(SelectedMusicFilesPropertiesChanged);
        _controller.FingerprintCalculated += (sender, e) => DispatcherQueue.TryEnqueue(UpdateFingerprint);
        _controller.CorruptedFilesFound += (sender, e) => DispatcherQueue.TryEnqueue(CorruptedFilesFound);
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
        User32.ShowWindow(_hwnd, ShowWindowCommand.SW_SHOWMAXIMIZED);
        //Localize Strings
        MenuFile.Title = _("File");
        MenuOpenFolder.Text = _("Open Folder");
        MenuOpenPlaylist.Text = _("Open Playlist");
        MenuReloadLibrary.Text = _("Reload Library");
        MenuCloseLibrary.Text = _("Close Library");
        MenuExit.Text = _("Exit");
        MenuEdit.Title = _("Edit");
        MenuSortFilesBy.Text = _("Sort Files By");
        MenuSortFilesFileName.Text = _("File Name");
        MenuSortFilesFilePath.Text = _("File Path");
        MenuSortFilesTitle.Text = _("Title");
        MenuSortFilesArtist.Text = _("Artist");
        MenuSortFilesAlbum.Text = _("Album");
        MenuSortFilesYear.Text = _("Year");
        MenuSortFilesTrack.Text = _("Track");
        MenuSortFilesGenre.Text = _("Genre");
        MenuSettings.Text = _("Settings");
        MenuPlaylist.Title = _("Playlist");
        MenuCreatePlaylist.Text = _("Create Playlist");
        MenuAddToPlaylist.Text = _("Add to Playlist");
        MenuRemoveFromPlaylist.Text = _("Remove from Playlist");
        MenuTag.Title = _("Tag");
        MenuSaveTag.Text = _("Save Tag");
        MenuDiscardChanges.Text = _("Discard Unapplied Changes");
        MenuDeleteTag.Text = _("Delete Tag");
        MenuManageLyrics.Text = _("Manage Lyrics");
        MenuAlbumArt.Text = _("Album Art");
        MenuAlbumArtFront.Text = _("Front");
        MenuAlbumArtFrontInsert.Text = _("Insert");
        MenuAlbumArtFrontRemove.Text = _("Remove");
        MenuAlbumArtFrontExport.Text = _("Export");
        MenuAlbumArtFrontInfo.Text = _("Info");
        MenuAlbumArtBack.Text = _("Back");
        MenuAlbumArtBackInsert.Text = _("Insert");
        MenuAlbumArtBackRemove.Text = _("Remove");
        MenuAlbumArtBackExport.Text = _("Export");
        MenuAlbumArtBackInfo.Text = _("Info");
        MenuSwitchAlbumArt.Text = _("Switch to Back Cover");
        MenuCustomProperties.Text = _("Custom Properties");
        MenuCustomPropertiesAdd.Text = _("Add");
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
        StatusPageHome.Title = _("Tag Your Music");
        StatusPageHome.Description = _("Open a library with music to get started");
        HomeOpenFolderButtonLabel.Text = _("Open Folder");
        HomeOpenPlaylistButtonLabel.Text = _("Open Playlist");
        LblHomeHelp.Text = _("Tagger's documentation and support channels are accessible via the Help menu.");
        StatusPageNoFiles.Title = _("No Music Files Found");
        StatusPageNoFiles.Description = _("Try a different library");
        SearchMusicFiles.PlaceholderText = _("Type ! for advanced search");
        ToolTipService.SetToolTip(BtnAdvancedSearchInfo, _("Advanced Search Info"));
        StatusPageNoSelected.Title = _("No Selected Music Files");
        StatusPageNoSelected.Description = _("Select some files");
        CmdBtnSaveTag.Label = _("Save Tag");
        ToolTipService.SetToolTip(CmdBtnSaveTag, _("Save Tag (Ctrl+S)"));
        ToolTipService.SetToolTip(CmdBtnDeleteTag, _("Delete Tag (Shift+Delete)"));
        ToolTipService.SetToolTip(CmdBtnDiscardChanges, _("Discard Unapplied Changed (Ctrl+Z)"));
        CmdBtnManageLyrics.Label = _("Lyrics");
        ToolTipService.SetToolTip(CmdBtnManageLyrics, _("Manage Lyrics (Ctrl+L)"));
        ToolTipService.SetToolTip(CmdBtnFilenameToTag, _("File Name to Tag (Ctrl+F)"));
        ToolTipService.SetToolTip(CmdBtnTagToFilename, _("Tag to File Name (Ctrl+T)"));
        ToolTipService.SetToolTip(CmdBtnDownloadMusicBrainz, _("Download MusicBrainz Metadata (Ctrl+M)"));
        CmdBtnExtrasPane.Label = _("Hide Extras Pane");
        CardFilename.Header = _("File Name");
        TxtFilename.PlaceholderText = _("Enter file name here");
        LblMainProperties.Text = _("Main Properties");
        CardTitle.Header = _("Title");
        TxtTitle.PlaceholderText = _("Enter title here");
        CardArtist.Header = _("Artist");
        TxtArtist.PlaceholderText = _("Enter artist here");
        CardAlbum.Header = _("Album");
        TxtAlbum.PlaceholderText = _("Enter album here");
        CardAlbumArtist.Header = _("Album Artist");
        TxtAlbumArtist.PlaceholderText = _("Enter album artist here");
        CardGenre.Header = _("Genre");
        TxtGenre.PlaceholderText = _("Enter genre here");
        CardYear.Header = _("Year");
        TxtYear.PlaceholderText = _("Enter year here");
        CardTrack.Header = _("Track");
        TxtTrack.PlaceholderText = _("Enter track here");
        TxtTrackTotal.PlaceholderText = _("Enter track total here");
        LblAdditionProperties.Text = _("Additional Properties");
        CardComment.Header = _("Comment");
        TxtComment.PlaceholderText = _("Enter comment here");
        CardBPM.Header = _("Beats Per Minute");
        TxtBPM.PlaceholderText = _("Enter beats per minute here");
        CardComposer.Header = _("Composer");
        TxtComposer.PlaceholderText = _("Enter composer here");
        CardDescription.Header = _("Description");
        TxtDescription.PlaceholderText = _("Enter description here");
        CardDisc.Header = _("Disc");
        TxtDiscNumber.PlaceholderText = _("Enter disc number here");
        TxtDiscTotal.PlaceholderText = _("Enter disc total here");
        CardPublisher.Header = _("Publisher");
        TxtPublisher.PlaceholderText = _("Enter publisher here");
        CardPublishingDate.Header = _("Publishing Date");
        DatePublishingDate.PlaceholderText = _("Pick a date");
        ToolTipService.SetToolTip(BtnClearPublishingDate, _("Clear Publishing Date"));
        LblCustomProperties.Text = _("Custom Properties");
        LblBtnAddCustomProperty.Text = _("Add");
        ToolTipService.SetToolTip(BtnAddCustomProperty, _("Add New Property"));
        LblCustomPropertiesWarning.Text = _("Custom properties can only be edited for individual files.");
        LblFileProperties.Text = _("File Properties");
        CardFingerprint.Header = _("Fingerprint");
        LblFingerprint.Text = _("Calculating...");
        ToolTipService.SetToolTip(BtnCopyFingerprint, _("Copy Fingerprint To Clipboard"));
        LblBtnAlbumArtSwitch.Text = _("Switch to Back Cover");
        LblBtnAlbumArtInsert.Text = _("Insert");
        LblBtnAlbumArtRemove.Text = _("Remove");
        LblBtnAlbumArtExport.Text = _("Export");
        LblBtnAlbumArtInfo.Text = _("Info");
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
        if (!_isOpened)
        {
            ViewStack.CurrentPageName = "Startup";
            var accent = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            _controller.TaskbarItem = TaskbarItem.ConnectWindows(_hwnd, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(accent.Color.A, accent.Color.R, accent.Color.G, accent.Color.B)), MainGrid.ActualTheme == ElementTheme.Dark ? System.Drawing.Brushes.Black : System.Drawing.Brushes.White);
            await _controller.StartupAsync();
            _controller.NetworkMonitor!.StateChanged += (sender, state) => DispatcherQueue.TryEnqueue(() =>
            {
                MenuDownloadMusicBrainz.IsEnabled = state;
                MenuDownloadLyrics.IsEnabled = state;
                MenuSubmitToAcoustId.IsEnabled = state;
            });
            MainMenu.IsEnabled = true;
            //Update sort file by menu
            MenuSortFilesFileName.IsChecked = _controller.SortFilesBy == SortBy.Filename;
            MenuSortFilesFilePath.IsChecked = _controller.SortFilesBy == SortBy.Path;
            MenuSortFilesTitle.IsChecked = _controller.SortFilesBy == SortBy.Title;
            MenuSortFilesArtist.IsChecked = _controller.SortFilesBy == SortBy.Artist;
            MenuSortFilesAlbum.IsChecked = _controller.SortFilesBy == SortBy.Album;
            MenuSortFilesYear.IsChecked = _controller.SortFilesBy == SortBy.Year;
            MenuSortFilesTrack.IsChecked = _controller.SortFilesBy == SortBy.Track;
            MenuSortFilesGenre.IsChecked = _controller.SortFilesBy == SortBy.Genre;
            ViewStack.CurrentPageName = "Home";
            if (!_controller.ExtrasPane)
            {
                ExtrasPaneToggle(this, new RoutedEventArgs());
            }
            _isOpened = true;
        }
    }

    /// <summary>
    /// Occurs when the window is closing
    /// </summary>
    /// <param name="sender">AppWindow</param>
    /// <param name="e">AppWindowClosingEventArgs</param>
    private async void Window_Closing(AppWindow sender, AppWindowClosingEventArgs e)
    {
        if (!_controller.CanClose)
        {
            e.Cancel = true;
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(false);
                Close();
            }
            if (res == ContentDialogResult.Secondary)
            {
                _controller.ForceAllowClose();
                Close();
            }
        }
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
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var first = (await e.DataView.GetStorageItemsAsync()).FirstOrDefault();
            if (first != null && MusicLibrary.GetIsValidLibraryPath(first.Path))
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
        if (e.Action == "update")
        {
            _notificationButtonClickEvent = WindowsUpdate;
            BtnInfoBar.Content = _("Update");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "reload")
        {
            _notificationButtonClickEvent = ReloadLibrary;
            BtnInfoBar.Content = _("Reload");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "unsupported")
        {
            _notificationButtonClickEvent = async (_, _) =>
            {
                InfoBar.IsOpen = false;
                await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("unsupported")));
            };
            BtnInfoBar.Content = _("Help");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "format")
        {
            _notificationButtonClickEvent = async (_, _) =>
            {
                InfoBar.IsOpen = false;
                await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("format-strings")));
            };
            BtnInfoBar.Content = _("Help");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "web")
        {
            _notificationButtonClickEvent = async (_, _) =>
            {
                InfoBar.IsOpen = false;
                await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("web-services")));
            };
            BtnInfoBar.Content = _("Help");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "musicbrainz" && !string.IsNullOrEmpty(e.ActionParam))
        {
            _notificationButtonClickEvent = async (_, _) =>
            {
                InfoBar.IsOpen = false;
                var dialog = new ContentDialog()
                {
                    Title = _("Failed MusicBrainz Lookup"),
                    Content = new ScrollViewer()
                    {
                        Content = e.ActionParam
                    },
                    CloseButtonText = _("OK"),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = MainGrid.XamlRoot
                };
                await dialog.ShowAsync();
            };
            BtnInfoBar.Content = _("Info");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else if (e.Action == "open-playlist" && File.Exists(e.ActionParam))
        {
            _notificationButtonClickEvent = async (_, _) =>
            {
                InfoBar.IsOpen = false;
                await _controller.OpenLibraryAsync(e.ActionParam);
            };
            BtnInfoBar.Content = _("Open");
            BtnInfoBar.Visibility = Visibility.Visible;
            BtnInfoBar.Click += _notificationButtonClickEvent;
        }
        else
        {
            BtnInfoBar.Visibility = Visibility.Collapsed;
        }
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
        folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            await _controller.OpenLibraryAsync(folder.Path);
        }
    }

    /// <summary>
    /// Occurs when the open playlist menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void OpenPlaylist(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        InitializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        foreach (var format in Enum.GetValues<PlaylistFormat>())
        {
            filePicker.FileTypeFilter.Add(format.GetDotExtension());
        }
        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            await _controller.OpenLibraryAsync(file.Path);
        }
    }

    /// <summary>
    /// Occurs when the reload library menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ReloadLibrary(object sender, RoutedEventArgs e)
    {
        InfoBar.IsOpen = false;
        if (!_controller.CanClose)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(false);
            }
            if (res != ContentDialogResult.None)
            {
                await _controller.ReloadLibraryAsync();
            }
        }
        else
        {
            await _controller.ReloadLibraryAsync();
        }
    }

    /// <summary>
    /// Occurs when the close library menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void CloseLibrary(object sender, RoutedEventArgs e)
    {
        if (!_controller.CanClose)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(false);
            }
            if (res != ContentDialogResult.None)
            {
                _controller.CloseLibrary();
            }
        }
        else
        {
            _controller.CloseLibrary();
        }
    }

    /// <summary>
    /// Occurs when the exit menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Exit(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Occurs when a sort files by menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void SortFilesBy(object sender, RoutedEventArgs e)
    {
        var sortBy = SortBy.Filename;
        if (ReferenceEquals(sender, MenuSortFilesFileName))
        {
            sortBy = SortBy.Filename;
        }
        else if (ReferenceEquals(sender, MenuSortFilesFilePath))
        {
            sortBy = SortBy.Path;
        }
        else if (ReferenceEquals(sender, MenuSortFilesTitle))
        {
            sortBy = SortBy.Title;
        }
        else if (ReferenceEquals(sender, MenuSortFilesArtist))
        {
            sortBy = SortBy.Artist;
        }
        else if (ReferenceEquals(sender, MenuSortFilesAlbum))
        {
            sortBy = SortBy.Album;
        }
        else if (ReferenceEquals(sender, MenuSortFilesYear))
        {
            sortBy = SortBy.Year;
        }
        else if (ReferenceEquals(sender, MenuSortFilesTrack))
        {
            sortBy = SortBy.Track;
        }
        else if (ReferenceEquals(sender, MenuSortFilesGenre))
        {
            sortBy = SortBy.Genre;
        }
        _controller.SortFilesBy = sortBy;
        _controller.SaveConfiguration();
        //Update sort file by menu
        MenuSortFilesFileName.IsChecked = _controller.SortFilesBy == SortBy.Filename;
        MenuSortFilesFilePath.IsChecked = _controller.SortFilesBy == SortBy.Path;
        MenuSortFilesTitle.IsChecked = _controller.SortFilesBy == SortBy.Title;
        MenuSortFilesArtist.IsChecked = _controller.SortFilesBy == SortBy.Artist;
        MenuSortFilesAlbum.IsChecked = _controller.SortFilesBy == SortBy.Album;
        MenuSortFilesYear.IsChecked = _controller.SortFilesBy == SortBy.Year;
        MenuSortFilesTrack.IsChecked = _controller.SortFilesBy == SortBy.Track;
        MenuSortFilesGenre.IsChecked = _controller.SortFilesBy == SortBy.Genre;
    }

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
        if (!_controller.CanClose)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(true);
            }
            if (res == ContentDialogResult.Secondary)
            {
                await _controller.DiscardSelectedUnappliedChangesAsync();
            }
            if (res != ContentDialogResult.None)
            {
                await settingsDialog.ShowAsync();
            }
        }
        else
        {
            await settingsDialog.ShowAsync();
        }
        //Update sort file by menu
        MenuSortFilesFileName.IsChecked = _controller.SortFilesBy == SortBy.Filename;
        MenuSortFilesFilePath.IsChecked = _controller.SortFilesBy == SortBy.Path;
        MenuSortFilesTitle.IsChecked = _controller.SortFilesBy == SortBy.Title;
        MenuSortFilesArtist.IsChecked = _controller.SortFilesBy == SortBy.Artist;
        MenuSortFilesAlbum.IsChecked = _controller.SortFilesBy == SortBy.Album;
        MenuSortFilesYear.IsChecked = _controller.SortFilesBy == SortBy.Year;
        MenuSortFilesTrack.IsChecked = _controller.SortFilesBy == SortBy.Track;
        MenuSortFilesGenre.IsChecked = _controller.SortFilesBy == SortBy.Genre;
    }

    /// <summary>
    /// Occurs when the Extras Pane toggle menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ExtrasPaneToggle(object sender, RoutedEventArgs e)
    {
        if (ExtrasPane.Visibility == Visibility.Visible)
        {
            DetailsSeparator.Visibility = Visibility.Collapsed;
            ExtrasPane.Visibility = Visibility.Collapsed;
            CmdBtnExtrasPane.Label = _("Show Extras Pane");
            CmdBtnExtrasPane.Icon = new SymbolIcon(Symbol.OpenPane);
            _controller.ExtrasPane = false;
        }
        else
        {
            DetailsSeparator.Visibility = Visibility.Visible;
            ExtrasPane.Visibility = Visibility.Visible;
            CmdBtnExtrasPane.Label = _("Hide Extras Pane");
            CmdBtnExtrasPane.Icon = new SymbolIcon(Symbol.ClosePane);
            _controller.ExtrasPane = true;
        }
        _controller.SaveConfiguration();
    }

    /// <summary>
    /// Occurs when the create playlist menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void CreatePlaylist(object sender, RoutedEventArgs e)
    {
        var createPlaylistDialog = new CreatePlaylistDialog(InitializeWithWindow)
        {
            XamlRoot = MainGrid.XamlRoot
        };
        if (!_controller.CanClose)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(true);
            }
            if (res == ContentDialogResult.Secondary)
            {
                await _controller.DiscardSelectedUnappliedChangesAsync();
            }
            if (res != ContentDialogResult.None)
            {
                var po = await createPlaylistDialog.ShowAsync();
                if (po != null)
                {
                    _controller.CreatePlaylist(po);
                }
            }
        }
        else
        {
            var po = await createPlaylistDialog.ShowAsync();
            if (po != null)
            {
                _controller.CreatePlaylist(po);
            }
        }
    }

    /// <summary>
    /// Occurs when the add to playlist menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AddToPlaylist(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        InitializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        foreach (var ext in MusicLibrary.SupportedExtensions)
        {
            filePicker.FileTypeFilter.Add(ext);
        }
        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            var relativeDialog = new ContentDialog()
            {
                Title = _("Use Relative Paths?"),
                Content = _("Would you like to save the added file to the playlist using it's relative path?\nIf not, the full path will be used instead."),
                PrimaryButtonText = _("Yes"),
                CloseButtonText = _("No"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            if (!_controller.CanClose)
            {
                var dialog = new ContentDialog()
                {
                    Title = _("Apply Changes?"),
                    Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                    PrimaryButtonText = _("Apply"),
                    SecondaryButtonText = _("Discard"),
                    CloseButtonText = _("Cancel"),
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = MainGrid.XamlRoot
                };
                var res = await dialog.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    await _controller.SaveAllTagsAsync(false);
                }
                if (res == ContentDialogResult.Secondary)
                {
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (res != ContentDialogResult.None)
                {
                    res = await relativeDialog.ShowAsync();
                    await _controller.AddFileToPlaylistAsync(file.Path, res == ContentDialogResult.Primary);
                }
            }
            else
            {
                var res = await relativeDialog.ShowAsync();
                await _controller.AddFileToPlaylistAsync(file.Path, res == ContentDialogResult.Primary);
            }
        }
    }

    /// <summary>
    /// Occurs when the remove to playlist menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void RemoveFromPlaylist(object sender, RoutedEventArgs e)
    {
        if (_controller.SelectedMusicFiles.Count == 0)
        {
            NotificationSent(sender, new NotificationSentEventArgs(_("No files selected for removal."), NotificationSeverity.Error));
        }
        else
        {
            var dialog = new ContentDialog()
            {
                Title = _("Remove Files?"),
                Content = _("The selected files will not be deleted from disk but will be removed from this playlist."),
                PrimaryButtonText = _("Yes"),
                CloseButtonText = _("No"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                if (!_controller.CanClose)
                {
                    dialog = new ContentDialog()
                    {
                        Title = _("Apply Changes?"),
                        Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                        PrimaryButtonText = _("Apply"),
                        SecondaryButtonText = _("Discard"),
                        CloseButtonText = _("Cancel"),
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = MainGrid.XamlRoot
                    };
                    res = await dialog.ShowAsync();
                    if (res == ContentDialogResult.Primary)
                    {
                        await _controller.SaveAllTagsAsync(false);
                    }
                    if (res == ContentDialogResult.Secondary)
                    {
                        await _controller.DiscardSelectedUnappliedChangesAsync();
                    }
                    if (res != ContentDialogResult.None)
                    {
                        await _controller.RemoveSelectedFilesFromPlaylistAsync();
                    }
                }
                else
                {
                    await _controller.RemoveSelectedFilesFromPlaylistAsync();
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the save tag menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void SaveTag(object sender, RoutedEventArgs e) => await _controller.SaveSelectedTagsAsync();

    /// <summary>
    /// Occurs when the discard unapplied changes menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void DiscardUnappliedChanges(object sender, RoutedEventArgs e) => await _controller.DiscardSelectedUnappliedChangesAsync();

    /// <summary>
    /// Occurs when the delete tag menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void DeleteTag(object sender, RoutedEventArgs e) => await _controller.DeleteSelectedTagsAsync();

    /// <summary>
    /// Occurs when the manage lyrics menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ManageLyrics(object sender, RoutedEventArgs e)
    {
        var controller = _controller.CreateLyricsDialogController();
        var lyricsDialog = new LyricsDialog(controller, InitializeWithWindow)
        {
            XamlRoot = MainGrid.XamlRoot
        };
        var res = await lyricsDialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            _controller.UpdateLyrics(controller.Lyrics);
        }
    }

    /// <summary>
    /// Occurs when the insert album art menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void InsertAlbumArt(object sender, RoutedEventArgs e)
    {
        var type = _currentAlbumArtType;
        if (ReferenceEquals(sender, MenuAlbumArtFrontInsert))
        {
            type = AlbumArtType.Front;
        }
        if (ReferenceEquals(sender, MenuAlbumArtBackInsert))
        {
            type = AlbumArtType.Back;
        }
        var filePicker = new FileOpenPicker();
        InitializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        filePicker.FileTypeFilter.Add(".jpg");
        filePicker.FileTypeFilter.Add(".jpeg");
        filePicker.FileTypeFilter.Add(".png");
        filePicker.FileTypeFilter.Add(".bmp");
        filePicker.FileTypeFilter.Add(".webp");
        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            await _controller.InsertSelectedAlbumArtAsync(file.Path, type);
        }
    }

    /// <summary>
    /// Occurs when the remove album art menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void RemoveAlbumArt(object sender, RoutedEventArgs e)
    {
        var type = _currentAlbumArtType;
        if (ReferenceEquals(sender, MenuAlbumArtFrontInsert))
        {
            type = AlbumArtType.Front;
        }
        if (ReferenceEquals(sender, MenuAlbumArtBackInsert))
        {
            type = AlbumArtType.Back;
        }
        await _controller.RemoveSelectedAlbumArtAsync(type);
    }

    /// <summary>
    /// Occurs when the export album art menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ExportAlbumArt(object sender, RoutedEventArgs e)
    {
        var type = _currentAlbumArtType;
        if (ReferenceEquals(sender, MenuAlbumArtFrontInsert))
        {
            type = AlbumArtType.Front;
        }
        if (ReferenceEquals(sender, MenuAlbumArtBackInsert))
        {
            type = AlbumArtType.Back;
        }
        var albumArt = type == AlbumArtType.Front ? _controller.SelectedPropertyMap.FrontAlbumArt : _controller.SelectedPropertyMap.BackAlbumArt;
        if (albumArt != "hasArt")
        {
            return;
        }
        var ext = _controller.GetFirstAlbumArt(type).MimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/tiff" => ".tiff",
            "image/tiff-fx" => ".tiff",
            _ => ".jpg"
        };
        var filePicker = new FileSavePicker();
        InitializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        filePicker.FileTypeChoices.Add(_("All files"), new List<string>() { ext, ext.ToUpper() });
        filePicker.DefaultFileExtension = ext;
        var file = await filePicker.PickSaveFileAsync();
        if (file != null)
        {
            _controller.ExportSelectedAlbumArt(file.Path, type);
        }
    }

    /// <summary>
    /// Occurs when the album art info menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AlbumArtInfo(object sender, RoutedEventArgs e)
    {
        var type = _currentAlbumArtType;
        if (ReferenceEquals(sender, MenuAlbumArtFrontInsert))
        {
            type = AlbumArtType.Front;
        }
        if (ReferenceEquals(sender, MenuAlbumArtBackInsert))
        {
            type = AlbumArtType.Back;
        }
        var dialog = new AlbumArtInfoDialog(_controller.GetFirstAlbumArt(type))
        {
            XamlRoot = MainGrid.XamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the add custom property menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AddCustomProperty(object sender, RoutedEventArgs e)
    {
        var entryDialog = new EntryDialog(_("New Custom Property"), "", _("Property Name"), _("Cancel"), _("Add"))
        {
            XamlRoot = MainGrid.XamlRoot
        };
        var res = await entryDialog.ShowAsync();
        if (!string.IsNullOrEmpty(res) && res != "NULL")
        {
            _controller.AddCustomProperty(res);
        }
    }

    /// <summary>
    /// Occurs when the filename to tag menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void FilenameToTag(object sender, RoutedEventArgs e)
    {
        var dialog = new ComboBoxDialog(_("File Name to Tag"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _controller.PreviousFTTFormatString, _("Cancel"), _("Convert"))
        {
            XamlRoot = MainGrid.XamlRoot
        };
        var res = await dialog.ShowAsync();
        if (!string.IsNullOrEmpty(res))
        {
            await _controller.FilenameToTagAsync(res);
        }
    }

    /// <summary>
    /// Occurs when the tag to filename menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void TagToFilename(object sender, RoutedEventArgs e)
    {
        var dialog = new ComboBoxDialog(_("Tag to File Name"), _("Please select a format string."), _("Format String"), _controller.FormatStrings, true, _controller.PreviousTTFFormatString, _("Cancel"), _("Convert"))
        {
            XamlRoot = MainGrid.XamlRoot
        };
        var res = await dialog.ShowAsync();
        if (!string.IsNullOrEmpty(res))
        {
            await _controller.TagToFilenameAsync(res);
        }
    }

    /// <summary>
    /// Occurs when the download musicbrainz metadata menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void DownloadMusicBrainzMetadata(object sender, RoutedEventArgs e) => await _controller.DownloadMusicBrainzMetadataAsync();

    /// <summary>
    /// Occurs when the download lyrics menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void DownloadLyrics(object sender, RoutedEventArgs e)
    {
        if (!_controller.CanClose)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Apply Changes?"),
                Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                PrimaryButtonText = _("Apply"),
                SecondaryButtonText = _("Discard"),
                CloseButtonText = _("Cancel"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainGrid.XamlRoot
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await _controller.SaveAllTagsAsync(false);
            }
            if (res == ContentDialogResult.Secondary)
            {
                await _controller.DiscardSelectedUnappliedChangesAsync();
            }
            if (res != ContentDialogResult.None)
            {
                await _controller.DownloadLyricsAsync();
            }
        }
        else
        {
            await _controller.DownloadLyricsAsync();
        }
    }

    /// <summary>
    /// Occurs when the submit to acoust id menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void SubmitToAcoustId(object sender, RoutedEventArgs e)
    {
        if (_controller.SelectedMusicFiles.Count > 1)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Too Many Files Selected"),
                Content = _("Only one file can be submitted to AcoustID at a time. Please select only one file and try again."),
                CloseButtonText = _("OK"),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = MainGrid.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }
        var entryDialog = new EntryDialog(_("Submit to AcoustId"), _("AcoustId can associate a song's fingerprint with a MusicBrainz Recording Id for easy identification.\n\nIf you have a MusicBrainz Recording Id for this song, please provide it below.\n\nIf none is provided, Tagger will submit your tag's metadata in association with the fingerprint instead."), _("MusicBrainz Recording Id"), _("Cancel"), _("Submit"))
        {
            XamlRoot = MainGrid.XamlRoot
        };
        var mbid = await entryDialog.ShowAsync();
        if (!string.IsNullOrEmpty(mbid))
        {
            if (!_controller.CanClose)
            {
                var dialog = new ContentDialog()
                {
                    Title = _("Apply Changes?"),
                    Content = _("Some music files still have changes waiting to be applied. What would you like to do?"),
                    PrimaryButtonText = _("Apply"),
                    SecondaryButtonText = _("Discard"),
                    CloseButtonText = _("Cancel"),
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = MainGrid.XamlRoot
                };
                var res = await dialog.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    await _controller.SaveAllTagsAsync(false);
                }
                if (res == ContentDialogResult.Secondary)
                {
                    await _controller.DiscardSelectedUnappliedChangesAsync();
                }
                if (res != ContentDialogResult.None)
                {
                    await _controller.SubmitToAcoustIdAsync(mbid == "NULL" ? null : mbid);
                }
            }
            else
            {
                await _controller.SubmitToAcoustIdAsync(mbid == "NULL" ? null : mbid);
            }
        }
    }

    /// <summary>
    /// Occurs when the switch album art menu item is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void SwitchAlbumArt(object sender, RoutedEventArgs e)
    {
        _currentAlbumArtType = _currentAlbumArtType == AlbumArtType.Front ? AlbumArtType.Back : AlbumArtType.Front;
        MenuSwitchAlbumArt.Text = _currentAlbumArtType == AlbumArtType.Front ? _("Switch to Back Cover") : _("Switch to Front Cover");
        LblBtnAlbumArtSwitch.Text = _currentAlbumArtType == AlbumArtType.Front ? _("Switch to Back Cover") : _("Switch to Front Cover");
        SelectedMusicFilesPropertiesChanged();
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
        MainMenu.IsEnabled = false;
        InfoBar.IsOpen = false;
        var page = ViewStack.CurrentPageName;
        ViewStack.CurrentPageName = "Startup";
        if (!(await _controller.WindowsUpdateAsync()))
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
    /// Occurs when the advanced search info button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AdvancedSearchInfo(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri(DocumentationHelpers.GetHelpURL("search")));

    /// <summary>
    /// Occurs when the music library is updated
    /// </summary>
    private void MusicLibraryUpdated()
    {
        ListMusicFilesCVS.Source = null;
        _musicFileRows.Clear();
        MainMenu.IsEnabled = true;
        if (!string.IsNullOrEmpty(_controller.MusicLibraryName))
        {
            var data = new Dictionary<string, TitledList>();
            foreach (var musicFile in _controller.MusicFiles)
            {
                var row = new MusicFileRow(musicFile);
                var header = _controller.GetHeaderForMusicFile(musicFile) ?? "";
                if (!data.ContainsKey(header))
                {
                    data[header] = new TitledList(header);
                }
                data[header].Add(row);
                _musicFileRows.Add(row);
            }
            ListMusicFilesCVS.Source = data.Values.ToList();
            MenuReloadLibrary.IsEnabled = true;
            MenuCloseLibrary.IsEnabled = true;
            MenuCreatePlaylist.IsEnabled = _controller.MusicLibraryType == MusicLibraryType.Folder;
            MenuAddToPlaylist.IsEnabled = _controller.MusicLibraryType == MusicLibraryType.Playlist;
            MenuRemoveFromPlaylist.IsEnabled = _controller.MusicLibraryType == MusicLibraryType.Playlist;
            ViewStack.CurrentPageName = "Library";
            FilesViewStack.CurrentPageName = _controller.MusicFiles.Count > 0 ? "Files" : "NoFiles";
            SelectedViewStack.CurrentPageName = "NoSelected";
            StatusBar.Visibility = Visibility.Visible;
            StatusIcon.Glyph = _controller.MusicLibraryType == MusicLibraryType.Folder ? "\uE8B7" : "\uE142";
            ToolTipService.SetToolTip(StatusIcon, _controller.MusicLibraryType == MusicLibraryType.Folder ? _("Folder Mode") : _("Playlist Mode"));
            StatusLabelLeft.Text = _controller.MusicLibraryName;
            StatusLabelRight.Text = _controller.MusicFiles.Count > 0 ? _("{0} of {1} selected", _controller.SelectedMusicFiles.Count, _controller.MusicFiles.Count) : "";
        }
        else
        {
            MenuReloadLibrary.IsEnabled = false;
            MenuCloseLibrary.IsEnabled = false;
            MenuCreatePlaylist.IsEnabled = false;
            MenuAddToPlaylist.IsEnabled = false;
            MenuRemoveFromPlaylist.IsEnabled = false;
            MenuTag.IsEnabled = false;
            ViewStack.CurrentPageName = "Home";
            StatusBar.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Occurs when a music file's save state is changed
    /// </summary>
    /// <param name="pending">Whether or not there are unsaved changes</param>
    private void MusicFileSaveStatesChanged(bool pending)
    {
        MainMenu.IsEnabled = true;
        ViewStack.CurrentPageName = "Library";
        MenuSaveTag.IsEnabled = pending;
        CmdBtnSaveTag.IsEnabled = pending;
        var i = 0;
        foreach (var saved in _controller.MusicFileSaveStates)
        {
            _musicFileRows[i].ShowUnsaveIcon = !saved;
            i++;
        }

    }

    /// <summary>
    /// Occurs when the selected music files' properties are changed
    /// </summary>
    private void SelectedMusicFilesPropertiesChanged()
    {
        //Update Properties
        SelectedViewStack.CurrentPageName = _controller.SelectedMusicFiles.Count > 0 ? "Selected" : "NoSelected";
        MenuSaveTag.IsEnabled = _controller.SelectedMusicFiles.Count > 0 && _controller.SelectedHasUnsavedChanges;
        CmdBtnSaveTag.IsEnabled = _controller.SelectedMusicFiles.Count > 0 && _controller.SelectedHasUnsavedChanges;
        MenuTag.IsEnabled = _controller.SelectedMusicFiles.Count > 0;
        MenuManageLyrics.IsEnabled = _controller.SelectedMusicFiles.Count == 1;
        CmdBtnManageLyrics.IsEnabled = _controller.SelectedMusicFiles.Count == 1;
        MenuCustomProperties.IsEnabled = _controller.SelectedMusicFiles.Count == 1;
        CustomPropertiesHeader.Visibility = _controller.SelectedMusicFiles.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
        ListCustomProperties.Visibility = _controller.SelectedMusicFiles.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
        LblCustomPropertiesWarning.Visibility = _controller.SelectedMusicFiles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        StatusLabelRight.Text = _controller.MusicFiles.Count > 0 ? _("{0} of {1} selected", _controller.SelectedMusicFiles.Count, _controller.MusicFiles.Count) : "";
        TxtFilename.IsReadOnly = _controller.SelectedMusicFiles.Count > 1;
        if (_controller.SelectedMusicFiles.Count == 0)
        {
            SearchMusicFiles.Text = "";
        }
        TxtFilename.Text = _controller.SelectedPropertyMap.Filename;
        TxtTitle.Text = _controller.SelectedPropertyMap.Title;
        TxtArtist.Text = _controller.SelectedPropertyMap.Artist;
        TxtAlbum.Text = _controller.SelectedPropertyMap.Album;
        TxtYear.Text = _controller.SelectedPropertyMap.Year;
        TxtTrack.Text = _controller.SelectedPropertyMap.Track;
        TxtTrackTotal.Text = _controller.SelectedPropertyMap.TrackTotal;
        TxtAlbumArtist.Text = _controller.SelectedPropertyMap.AlbumArtist;
        TxtGenre.Text = _controller.SelectedPropertyMap.Genre;
        TxtComment.Text = _controller.SelectedPropertyMap.Comment;
        TxtBPM.Text = _controller.SelectedPropertyMap.BeatsPerMinute;
        TxtComposer.Text = _controller.SelectedPropertyMap.Composer;
        TxtDescription.Text = _controller.SelectedPropertyMap.Description;
        TxtDiscNumber.Text = _controller.SelectedPropertyMap.DiscNumber;
        TxtDiscTotal.Text = _controller.SelectedPropertyMap.DiscTotal;
        TxtPublisher.Text = _controller.SelectedPropertyMap.Publisher;
        DatePublishingDate.Date = string.IsNullOrEmpty(_controller.SelectedPropertyMap.PublishingDate) ? null : (_controller.SelectedPropertyMap.PublishingDate == _("<keep>") ? null : DateTimeOffset.Parse(_controller.SelectedPropertyMap.PublishingDate));
        DatePublishingDate.PlaceholderText = _controller.SelectedPropertyMap.PublishingDate == _("<keep>") ? _("<keep>") : _("Pick a date");
        LblDurationFileSize.Text = $"{_controller.SelectedPropertyMap.Duration} • {_controller.SelectedPropertyMap.FileSize}";
        LblFingerprint.Text = _controller.SelectedPropertyMap.Fingerprint;
        var albumArt = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedPropertyMap.FrontAlbumArt : _controller.SelectedPropertyMap.BackAlbumArt;
        if (albumArt == "hasArt")
        {
            ArtViewStack.CurrentPageName = "Image";
            var art = _currentAlbumArtType == AlbumArtType.Front ? _controller.SelectedMusicFiles.First().Value.FrontAlbumArt : _controller.SelectedMusicFiles.First().Value.BackAlbumArt;
            if (art.IsEmpty)
            {
                ImgAlbumArt.Source = null;
            }
            else
            {
                using var ms = new InMemoryRandomAccessStream();
                using var writer = new DataWriter(ms.GetOutputStreamAt(0));
                writer.WriteBytes(art.Image);
                writer.StoreAsync().GetResults();
                var image = new BitmapImage();
                image.SetSource(ms);
                ImgAlbumArt.Source = image;
            }
        }
        else if (albumArt == "keepArt")
        {
            ArtViewStack.CurrentPageName = "KeepImage";
        }
        else
        {
            ArtViewStack.CurrentPageName = "NoImage";
        }
        ToolTipService.SetToolTip(ArtViewStack, _currentAlbumArtType == AlbumArtType.Front ? _("Front") : _("Back"));
        MenuAlbumArtFrontRemove.IsEnabled = _controller.SelectedPropertyMap.FrontAlbumArt == "hasArt" || _controller.SelectedPropertyMap.FrontAlbumArt == "keepArt";
        MenuAlbumArtBackRemove.IsEnabled = _controller.SelectedPropertyMap.BackAlbumArt == "hasArt" || _controller.SelectedPropertyMap.BackAlbumArt == "keepArt";
        BtnAlbumArtRemove.IsEnabled = ArtViewStack.CurrentPageName != "NoImage";
        MenuAlbumArtFrontExport.IsEnabled = _controller.SelectedPropertyMap.FrontAlbumArt == "hasArt";
        MenuAlbumArtBackExport.IsEnabled = _controller.SelectedPropertyMap.BackAlbumArt == "hasArt";
        BtnAlbumArtExport.IsEnabled = ArtViewStack.CurrentPageName == "Image";
        MenuAlbumArtFrontInfo.IsEnabled = _controller.SelectedPropertyMap.FrontAlbumArt == "hasArt";
        MenuAlbumArtBackInfo.IsEnabled = _controller.SelectedPropertyMap.BackAlbumArt == "hasArt";
        BtnAlbumArtInfo.IsEnabled = ArtViewStack.CurrentPageName == "Image";
        //Update Custom Properties
        ListCustomProperties.Children.Clear();
        if (_controller.SelectedMusicFiles.Count == 1)
        {
            foreach (var pair in _controller.SelectedPropertyMap.CustomProperties)
            {
                var row = new CustomPropertyRow(pair);
                row.TextChanged += TagPropertyChanged;
                row.RemoveClicked += (sender, e) => _controller.RemoveCustomProperty(pair.Key);
                ListCustomProperties.Children.Add(row);
            }
        }
        //Update Rows
        foreach (var pair in _controller.SelectedMusicFiles)
        {
            _musicFileRows[pair.Key].Update(pair.Value);
        }
    }

    /// <summary>
    /// Occurs when a tag property's row text is changed
    /// </summary>
    private void TagPropertyChanged(object sender, TextChangedEventArgs e)
    {
        UIElement? box = null;
        if (sender is TextBox || sender is AutoSuggestBox || sender is CalendarDatePicker)
        {
            box = (UIElement)sender;
        }
        if (_controller.SelectedMusicFiles.Count > 0 && box != null && (box.FocusState == FocusState.Keyboard || box.FocusState == FocusState.Pointer || box is AutoSuggestBox || (box is CalendarDatePicker && box.FocusState == FocusState.Unfocused)))
        {
            //Update Tags
            var propMap = new PropertyMap()
            {
                Filename = TxtFilename.Text,
                Title = TxtTitle.Text,
                Artist = TxtArtist.Text,
                Album = TxtAlbum.Text,
                Year = TxtYear.Text,
                Track = TxtTrack.Text,
                TrackTotal = TxtTrackTotal.Text,
                AlbumArtist = TxtAlbumArtist.Text,
                Genre = TxtGenre.Text,
                Comment = TxtComment.Text,
                BeatsPerMinute = TxtBPM.Text,
                Composer = TxtComposer.Text,
                Description = TxtDescription.Text,
                DiscNumber = TxtDiscNumber.Text,
                DiscTotal = TxtDiscTotal.Text,
                Publisher = TxtPublisher.Text,
                PublishingDate = DatePublishingDate.PlaceholderText == _("<keep>") ? _("<keep>") : (DatePublishingDate.Date?.Date.ToShortDateString() ?? ""),
            };
            if (_controller.SelectedMusicFiles.Count == 1)
            {
                foreach (var row in ListCustomProperties.Children.Select(x => (CustomPropertyRow)x))
                {
                    propMap.CustomProperties.Add(row.Key, row.Value);
                }
            }
            _controller.UpdateTags(propMap, false);
            //Update Rows
            foreach (var pair in _controller.SelectedMusicFiles)
            {
                _musicFileRows[pair.Key].Update(pair.Value);
            }
        }
    }

    /// <summary>
    /// Occurs when fingerprint is ready to be shown
    /// </summary>
    private void UpdateFingerprint()
    {
        if (!string.IsNullOrEmpty(_controller.SelectedPropertyMap.Fingerprint) && _controller.SelectedPropertyMap.Fingerprint != _("Calculating..."))
        {
            LblFingerprint.Text = _controller.SelectedPropertyMap.Fingerprint;
            BtnCopyFingerprint.IsEnabled = true;
        }
    }

    /// <summary>
    /// Occurs when there are corrupted music files found in a music library
    /// </summary>
    private async void CorruptedFilesFound()
    {
        var dialog = new CorruptedFilesDialog(_controller.MusicLibraryName, _controller.CorruptedFiles)
        {
            XamlRoot = MainGrid.XamlRoot
        };
        await dialog.ShowAsync();
    }

    /// <summary>
    /// Occurs when the ListMusicFiles's selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void ListMusicFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedIndexes = ListMusicFiles.SelectedItems.Select(x => _musicFileRows.IndexOf((MusicFileRow)x)).ToList();
        if (_currentAlbumArtType != AlbumArtType.Front)
        {
            _currentAlbumArtType = _currentAlbumArtType == AlbumArtType.Front ? AlbumArtType.Back : AlbumArtType.Front;
            MenuSwitchAlbumArt.Text = _currentAlbumArtType == AlbumArtType.Front ? _("Switch to Back Cover") : _("Switch to Front Cover");
            LblBtnAlbumArtSwitch.Text = _currentAlbumArtType == AlbumArtType.Front ? _("Switch to Back Cover") : _("Switch to Front Cover");
        }
        _controller.UpdateSelectedMusicFiles(selectedIndexes);
        if (string.IsNullOrEmpty(LblFingerprint.Text))
        {
            BtnCopyFingerprint.IsEnabled = false;
        }
    }

    /// <summary>
    /// Occurs when the SearchMusicFiles's text is changed
    /// </summary>
    /// <param name="sender">AutoSuggestBox</param>
    /// <param name="args">AutoSuggestBoxTextChangedEventArgs</param>
    private void SearchMusicFiles_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var search = SearchMusicFiles.Text.ToLower();
        if (!string.IsNullOrEmpty(search) && search[0] == '!')
        {
            BtnAdvancedSearchInfo.Visibility = Visibility.Visible;
            var result = _controller.AdvancedSearch(search);
            if (!result.Success)
            {
                SearchMusicFiles.Background = new AcrylicBrush()
                {
                    TintOpacity = 0.5,
                    TintColor = MainGrid.ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 224, 27, 36) : Color.FromArgb(255, 192, 28, 40)
                };
                foreach (var row in _musicFileRows)
                {
                    ListMusicFiles.ContainerFromItem(row).SetPropertyValue("Visibility", Visibility.Visible);
                }
            }
            else
            {
                SearchMusicFiles.Background = new AcrylicBrush()
                {
                    TintOpacity = 0.5,
                    TintColor = MainGrid.ActualTheme == ElementTheme.Light ? Color.FromArgb(255, 46, 194, 126) : Color.FromArgb(255, 38, 162, 105)
                };
                foreach (var row in _musicFileRows)
                {
                    if (result.LowerFilenames!.Count == 0)
                    {
                        ListMusicFiles.ContainerFromItem(row).SetPropertyValue("Visibility", Visibility.Collapsed);
                    }
                    var rowFilename = row.Subtitle;
                    if (string.IsNullOrEmpty(rowFilename))
                    {
                        rowFilename = row.Title;
                    }
                    rowFilename = rowFilename.ToLower();
                    ListMusicFiles.ContainerFromItem(row).SetPropertyValue("Visibility", result.LowerFilenames.Contains(rowFilename) ? Visibility.Visible : Visibility.Collapsed);
                }
            }
        }
        else
        {
            BtnAdvancedSearchInfo.Visibility = Visibility.Collapsed;
            SearchMusicFiles.Background = new SolidColorBrush(Colors.Transparent);
            foreach (var row in _musicFileRows)
            {
                var rowFilename = row.Subtitle;
                if (string.IsNullOrEmpty(rowFilename))
                {
                    rowFilename = row.Title;
                }
                rowFilename = rowFilename.ToLower();
                ListMusicFiles.ContainerFromItem(row).SetPropertyValue("Visibility", string.IsNullOrEmpty(search) || rowFilename.Contains(search) ? Visibility.Visible : Visibility.Collapsed);
            }
        }
    }

    /// <summary>
    /// Occurs when the PropertiesPane's size changes
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void PropertiesPane_SizeChanged(object sender, SizeChangedEventArgs e) => (PropertiesPane.Content as StackPanel).Margin = new Thickness(0, 0, PropertiesPane.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the TxtGenre's text is changed
    /// </summary>
    /// <param name="sender">AutoSuggestBox</param>
    /// <param name="args">AutoSuggestBoxTextChangedEventArgs</param>
    private void TxtGenre_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        TxtGenre.ItemsSource = null;
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            TxtGenre.ItemsSource = _controller.GetGenreSuggestions(TxtGenre.Text);
        }
        if (args.Reason != AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
        {
            TagPropertyChanged(sender, null);
        }
    }

    /// <summary>
    /// Occurs when the DatePublishingDate's date is changed
    /// </summary>
    /// <param name="sender">CalendarDatePicker</param>
    /// <param name="args">CalendarDatePickerDateChangedEventArgs</param>
    private void DatePublishingDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        if (args.NewDate != null)
        {
            TagPropertyChanged(sender, null);
        }
    }

    /// <summary>
    /// Occurs when the clear publishing date button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ClearPublishingDate(object sender, RoutedEventArgs e)
    {
        DatePublishingDate.Date = null;
        TagPropertyChanged(DatePublishingDate, null);
    }

    /// <summary>
    /// Occurs when the CopyFingerprintButton is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void CopyFingerprintToClipboard(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(LblFingerprint.Text);
        Clipboard.SetContent(package);
        NotificationSent(sender, new NotificationSentEventArgs(_("Fingerprint was copied to clipboard."), NotificationSeverity.Success));
    }
}
