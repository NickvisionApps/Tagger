using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Models;
using System;
using System.Threading.Tasks;
using Windows.System;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.WinUI.Views;

/// <summary>
/// A dialog for managing application settings
/// </summary>
public sealed partial class SettingsDialog : ContentDialog
{
    private readonly PreferencesViewController _controller;

    /// <summary>
    /// Constructs a SettingsDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    public SettingsDialog(PreferencesViewController controller)
    {
        InitializeComponent();
        _controller = controller;
        //Localize Strings
        Title = _("Settings");
        PrimaryButtonText = _("Apply");
        CloseButtonText = _("Cancel");
        LblUserInterface.Text = _("User Interface");
        CardTheme.Header = _("Theme");
        CardTheme.Description = _("An application restart is required to change the theme.");
        CmbTheme.Items.Add(_p("Theme", "Light"));
        CmbTheme.Items.Add(_p("Theme", "Dark"));
        CmbTheme.Items.Add(_p("Theme", "System"));
        CardAutomaticallyCheckForUpdates.Header = _("Automatically Check for Updates");
        TglAutomaticallyCheckForUpdates.OnContent = _("On");
        TglAutomaticallyCheckForUpdates.OffContent = _("Off");
        CardRememberLastOpenedLibrary.Header = ("Remember Last Opened Library");
        TglRememberLastOpenedLibrary.OnContent = _("On");
        TglRememberLastOpenedLibrary.OffContent = _("Off");
        LblMusicLibrary.Text = _("Music Library");
        CardIncludeSubfolders.Header = ("Include Subfolders");
        TglIncludeSubfolders.OnContent = _("On");
        TglIncludeSubfolders.OffContent = _("Off");
        CardSortFiles.Header = _("Sort Files By");
        CmbSortFiles.Items.Add(_("File Name"));
        CmbSortFiles.Items.Add(_("File Path"));
        CmbSortFiles.Items.Add(_("Title"));
        CmbSortFiles.Items.Add(_("Artist"));
        CmbSortFiles.Items.Add(_("Album"));
        CmbSortFiles.Items.Add(_("Year"));
        CmbSortFiles.Items.Add(_("Track"));
        CmbSortFiles.Items.Add(_("Genre"));
        LblMusicFile.Text = _("Music File");
        CardPreserveModificationTimestamp.Header = _("Preserve Modification Timestamp");
        TglPreserveModificationTimestamp.OnContent = _("On");
        TglPreserveModificationTimestamp.OffContent = _("Off");
        LblWebServices.Text = _("Web Services");
        CardOverwriteTagMusicBrainz.Header = _("Overwrite Tag With MusicBrainz");
        CardOverwriteTagMusicBrainz.Description = _("Enable to overwrite existing tag metadata with data found from MusicBrainz. If disabled, only blank tag properties will be filled.");
        TglOverwriteTagMusicBrainz.OnContent = _("On");
        TglOverwriteTagMusicBrainz.OffContent = _("Off");
        CardOverwriteAlbumArtMusicBrainz.Header = _("Overwrite Album Art With MusicBrainz");
        CardOverwriteAlbumArtMusicBrainz.Description = _("Enable to overwrite existing album art with art found from MusicBrainz.");
        TglOverwriteAlbumArtMusicBrainz.OnContent = _("On");
        TglOverwriteAlbumArtMusicBrainz.OffContent = _("Off");
        CardOverwriteLyricsFromWebService.Header = _("Overwrite Lyrics From Web Service");
        CardOverwriteLyricsFromWebService.Description = _("Enable to overwrite existing lyrics with that found from downloading lyrics from the web. If disabled, only blank lyrics will be filled.");
        TglOverwriteLyricsFromWebService.OnContent = _("On");
        TglOverwriteLyricsFromWebService.OffContent = _("Off");
        CardAcoustIdKey.Header = _("AcoustId User API Key");
        TxtAcoustIdKey.PlaceholderText = _("Enter key here");
        ToolTipService.SetToolTip(BtnAcoustIdKey, _("Get New API Key"));
        //Load Config
        CmbTheme.SelectedIndex = (int)_controller.Theme;
        TglAutomaticallyCheckForUpdates.IsOn = _controller.AutomaticallyCheckForUpdates;
        TglRememberLastOpenedLibrary.IsOn = _controller.RememberLastOpenedFolder;
        TglIncludeSubfolders.IsOn = _controller.IncludeSubfolders;
        CmbSortFiles.SelectedIndex = (int)_controller.SortFilesBy;
        TglPreserveModificationTimestamp.IsOn = _controller.PreserveModificationTimestamp;
        TglOverwriteTagMusicBrainz.IsOn = _controller.OverwriteTagWithMusicBrainz;
        TglOverwriteAlbumArtMusicBrainz.IsOn = _controller.OverwriteAlbumArtWithMusicBrainz;
        TglOverwriteLyricsFromWebService.IsOn = _controller.OverwriteLyricsWithWebService;
        TxtAcoustIdKey.Text = _controller.AcoustIdUserAPIKey;
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    /// <returns>ContentDialogResult</returns>
    public new async Task<ContentDialogResult> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var needsRestart = false;
            if (_controller.Theme != (Theme)CmbTheme.SelectedIndex)
            {
                _controller.Theme = (Theme)CmbTheme.SelectedIndex;
                needsRestart = true;
            }
            _controller.AutomaticallyCheckForUpdates = TglAutomaticallyCheckForUpdates.IsOn;
            _controller.RememberLastOpenedFolder = TglRememberLastOpenedLibrary.IsOn;
            _controller.IncludeSubfolders = TglIncludeSubfolders.IsOn;
            _controller.SortFilesBy = (SortBy)CmbSortFiles.SelectedIndex;
            _controller.PreserveModificationTimestamp = TglPreserveModificationTimestamp.IsOn;
            _controller.OverwriteTagWithMusicBrainz = TglOverwriteTagMusicBrainz.IsOn;
            _controller.OverwriteAlbumArtWithMusicBrainz = TglOverwriteAlbumArtMusicBrainz.IsOn;
            _controller.OverwriteLyricsWithWebService = TglOverwriteLyricsFromWebService.IsOn;
            _controller.AcoustIdUserAPIKey = TxtAcoustIdKey.Text;
            _controller.SaveConfiguration();
            if (needsRestart)
            {
                var restartDialog = new ContentDialog()
                {
                    Title = _("Restart To Apply Theme?"),
                    Content = _("Would you like to restart {0} to apply the new theme?\nAny unsaved work will be lost.", _controller.AppInfo.ShortName),
                    PrimaryButtonText = _("Yes"),
                    CloseButtonText = _("No"),
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };
                var resultRestart = await restartDialog.ShowAsync();
                if (resultRestart == ContentDialogResult.Primary)
                {
                    AppInstance.Restart("Apply new theme");
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the new acoust id key button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void NewAcoustIdKey(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri(_controller.AcoustIdUserAPIKeyLink));
}
