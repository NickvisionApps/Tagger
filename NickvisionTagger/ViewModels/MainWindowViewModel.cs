using Avalonia;
using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.Models;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Commands;
using Nickvision.Avalonia.MVVM.Messaging;
using Nickvision.Avalonia.MVVM.Services;
using Nickvision.Avalonia.Update;
using NickvisionTagger.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NickvisionTagger.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ICloseable _mainWindowCloseable;
    private readonly ServiceCollection _serviceCollection;
    private readonly HttpClient _httpClient;
    private readonly HomePageViewModel _homePageViewModel;
    private NavigationViewItem? _selectedNavItem;
    private object? _selectedPage;

    public DelegateAsyncCommand<object?> OpenedCommand { get; init; }
    public DelegateAsyncCommand<object?> CheckForUpdatesCommand { get; init; }

    public bool TitleBorderVisible => Environment.OSVersion.Platform == PlatformID.Win32NT;
    public Thickness? FrameMargin => Environment.OSVersion.Platform == PlatformID.Win32NT ? new Thickness(0, 30, 0, 0) : null;
    public Thickness? InfoBarMargin => Environment.OSVersion.Platform == PlatformID.Win32NT ? new Thickness(40, 30, 40, 0) : new Thickness(40, 0, 40, 0);

    public MainWindowViewModel(ICloseable mainWindowCloseable, ServiceCollection serviceCollection)
    {
        Title = "Nickvision Tagger";
        _mainWindowCloseable = mainWindowCloseable;
        _serviceCollection = serviceCollection;
        _httpClient = new HttpClient();
        _homePageViewModel = new HomePageViewModel(_serviceCollection);
        OpenedCommand = new DelegateAsyncCommand<object?>(Opened);
        CheckForUpdatesCommand = new DelegateAsyncCommand<object?>(CheckForUpdates);
    }

    public NavigationViewItem? SelectedNavItem
    {
        get => _selectedNavItem;

        set
        {
            SetProperty(ref _selectedNavItem, value);
            if (SelectedNavItem?.Tag is string tag)
            {
                if (tag == "Home")
                {
                    Messenger.Default.Send("HomeSettingsUpdate", null);
                    SelectedPage = _homePageViewModel.GetUserControlView();
                }
                else if (tag == "Settings")
                {
                    SelectedPage = new SettingsPageViewModel(_serviceCollection).GetUserControlView();
                }
            }
            else
            {
                SelectedPage = null;
            }
        }
    }

    public object? SelectedPage
    {
        get => _selectedPage;

        set => SetProperty(ref _selectedPage, value);
    }

    private async Task Opened(object? parameter)
    {
        var config = await Configuration.LoadAsync();
        _serviceCollection.GetService<IThemeService>()?.ChangeTheme(config.Theme);
        _serviceCollection.GetService<IThemeService>()?.ChangeAccentColor(config.AccentColor);
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            _serviceCollection.GetService<IThemeService>()?.ForceWin32WindowToTheme();
        }
        ATL.Settings.UseFileNameWhenNoTitle = false;
        _homePageViewModel.OpenedCommand.Execute(null);
    }

    private async Task CheckForUpdates(object? parameter)
    {
        var updater = new Updater(_httpClient, new Uri("https://raw.githubusercontent.com/nlogozzo/NickvisionTagger/main/UpdateConfig.json"), new Version("2022.3.0"));
        await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Checking for updates...", async () => await updater.CheckForUpdatesAsync())!;
        if (updater.UpdateAvailable)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var result = await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
                {
                    Title = "Update Available",
                    Message = $"===V{updater.LatestVersion} Changelog===\n{updater.Changelog}\n\nNickvision Tagger will automatically download and install the update, please save all work before continuing. Are you ready to update?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    DefaultButton = ContentDialogButton.Close
                })!;
                if (result == ContentDialogResult.Primary)
                {
                    var updateSuccess = false;
                    await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Downloading and installing the update...", async () => updateSuccess = await updater.WindowsUpdateAsync(_mainWindowCloseable))!;
                    if (!updateSuccess)
                    {
                        _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("Error", "An unknown error occurred when trying to download and install the update.", InfoBarSeverity.Error);
                    }
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var result = await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
                {
                    Title = "Update Available",
                    Message = $"===V{updater.LatestVersion} Changelog===\n{updater.Changelog}\n\nNickvision Tagger will automatically download the updated application to your downloads directory. If the app is currently running from your downloads directory, please move it before updating. Are you ready to update?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    DefaultButton = ContentDialogButton.Close
                })!;
                if (result == ContentDialogResult.Primary)
                {
                    var updateSuccess = false;
                    await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Downloading the update...", async () => updateSuccess = await updater.LinuxUpdateAsync("NickvisionTagger"))!;
                    if (updateSuccess)
                    {
                        _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("Update Completed", "The update has been downloaded to your downloads directory. We recommend moving the exe out of your downloads directory and running it somewhere else.", InfoBarSeverity.Success);
                    }
                    else
                    {
                        _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("Error", "An unknown error occurred when trying to download and install the update.", InfoBarSeverity.Error);
                    }
                }
            }
            else
            {
                await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
                {
                    Title = "Update Available",
                    Message = $"===V{updater.LatestVersion} Changelog===\n{updater.Changelog}\n\nPlease visit the GitHub repo to download the latest release.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                })!;
            }
        }
        else
        {
            _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("No Update Available", "There is no update at this time. Please try again later.", InfoBarSeverity.Error);
        }
    }
}
