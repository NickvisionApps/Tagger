using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.Extensions;
using Nickvision.Avalonia.Models;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Commands;
using Nickvision.Avalonia.MVVM.Services;
using Nickvision.Avalonia.Update;
using NickvisionTagger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NickvisionTagger.ViewModels
{
    public enum FontSize
    {
        Big,
        Regular,
        Small
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private ICloseable _mainWindowCloseable;
        private readonly ServiceCollection _serviceCollection;
        private bool _canClose;
        private readonly HttpClient _httpClient;
        private string? _path;
        private FontSize _selectedFontSize;

        public ObservableCollection<FontSize> FontSizes { get; init; }
        public DelegateAsyncCommand<object?> OpenedCommand { get; init; }
        public DelegateAsyncCommand<CancelEventArgs?> ClosingCommand { get; init; }
        public DelegateAsyncCommand<object?> NewFileCommand { get; init; }
        public DelegateAsyncCommand<object?> OpenFileCommand { get; init; }
        public DelegateCommand<object?> CloseFileCommand { get; init; }
        public DelegateCommand<object?> ExitCommand { get; init; }
        public DelegateAsyncCommand<object?> SettingsCommand { get; init; }
        public DelegateAsyncCommand<object?> CheckForUpdatesCommand { get; init; }
        public DelegateCommand<object?> GitHubRepoCommand { get; init; }
        public DelegateCommand<object?> ReportABugCommand { get; init; }
        public DelegateAsyncCommand<object?> ChangelogCommand { get; init; }
        public DelegateAsyncCommand<object?> AboutCommand { get; init; }

        public MainWindowViewModel(ICloseable mainWindowCloseable, ServiceCollection serviceCollection)
        {
            Title = "Nickvision Tagger";
            _mainWindowCloseable = mainWindowCloseable;
            _serviceCollection = serviceCollection;
            _canClose = false;
            _httpClient = new HttpClient();
            FontSizes = EnumExtensions.GetObservableCollection<FontSize>();
            OpenedCommand = new DelegateAsyncCommand<object?>(Opened);
            ClosingCommand = new DelegateAsyncCommand<CancelEventArgs?>(Closing);
            NewFileCommand = new DelegateAsyncCommand<object?>(NewFile);
            OpenFileCommand = new DelegateAsyncCommand<object?>(OpenFile);
            CloseFileCommand = new DelegateCommand<object?>(CloseFile, () => Path != "No File Opened");
            ExitCommand = new DelegateCommand<object?>(Exit);
            SettingsCommand = new DelegateAsyncCommand<object?>(Settings);
            CheckForUpdatesCommand = new DelegateAsyncCommand<object?>(CheckForUpdates);
            GitHubRepoCommand = new DelegateCommand<object?>(GitHubRepo);
            ReportABugCommand = new DelegateCommand<object?>(ReportAbug);
            ChangelogCommand = new DelegateAsyncCommand<object?>(Changelog);
            AboutCommand = new DelegateAsyncCommand<object?>(About);
            Path = "No File Opened";
            SelectedFontSize = FontSize.Regular;
        }

        public string? Path
        {
            get => _path;

            set
            {
                SetProperty(ref _path, value);
                CloseFileCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("FileContents");
            }
        }

        public FontSize SelectedFontSize
        {
            get => _selectedFontSize;

            set
            {
                SetProperty(ref _selectedFontSize, value);
                OnPropertyChanged("DocFontSize");
            }
        }

        public int DocFontSize
        {
            get
            {
                return SelectedFontSize switch
                {
                    FontSize.Big => 20,
                    FontSize.Regular => 14,
                    FontSize.Small => 9,
                    _ => 14
                };
            }
        }

        public string FileContents
        {
            get
            {
                try
                {
                    return File.ReadAllText(Path!);
                }
                catch
                {
                    return "";
                }
            }
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
        }

        private async Task Closing(CancelEventArgs? args)
        {
            if (!_canClose)
            {
                if (args != null)
                {
                    args.Cancel = true;
                }
                var result = await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
                {
                    Title = "Goodbye",
                    Message = "Are you sure you want to exit?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes",
                    DefaultButton = ContentDialogButton.Close
                })!;
                if (result == ContentDialogResult.Primary)
                {
                    _canClose = true;
                    _mainWindowCloseable.Close();
                }
            }
            else
            {
                var config = await Configuration.LoadAsync();
                await config.SaveAsync();
            }
        }

        private async Task NewFile(object? parameter)
        {
            var fileFilters = new List<FileDialogFilter>();
            fileFilters.Add(new FileDialogFilter()
            {
                Name = "Text File",
                Extensions = new List<string>() { "txt" }
            });
            var result = await _serviceCollection.GetService<IIOService>()?.ShowSaveFileDialogAsync("Save File", fileFilters, "txt")!;
            if (result != null)
            {
                await File.WriteAllTextAsync(result, "");
                Path = result;
            }
        }

        private async Task OpenFile(object? parameter)
        {
            var fileFilters = new List<FileDialogFilter>();
            fileFilters.Add(new FileDialogFilter()
            {
                Name = "Text File",
                Extensions = new List<string>() { "txt" }
            });
            var result = await _serviceCollection.GetService<IIOService>()?.ShowOpenFileDialogAsync("Open File", fileFilters)!;
            if (result != null)
            {
                Path = result[0];
            }
        }

        private void CloseFile(object? parameter) => Path = "No File Opened";

        private void Exit(object? parameter) => _mainWindowCloseable.Close();

        private async Task Settings(object? parameter) => await _serviceCollection.GetService<IContentDialogService>()?.ShowCustomAsync(new SettingsDialogViewModel(_serviceCollection))!;

        private async Task CheckForUpdates(object? parameter)
        {
            var updater = new Updater(_httpClient, new Uri("https://raw.githubusercontent.com/nlogozzo/NickvisionTagger/main/UpdateConfig.json"), new Version("2022.2.0"));
            await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Checking for updates...", async () => await updater.CheckForUpdatesAsync())!;
            if (updater.UpdateAvailable)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var result = await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
                    {
                        Title = "Update Available",
                        Message = $"===V{updater.LatestVersion} Changelog===\n{updater.Changelog}\n\nNickvisionApp will automatically download and install the update, please save all work before continuing. Are you ready to update?",
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
                        Message = $"===V{updater.LatestVersion} Changelog===\n{updater.Changelog}\n\nNickvisionApp will automatically download the updated application to your downloads directory. If the app is currently running from your downloads directory, please move it before updating. Are you ready to update?",
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

        private void GitHubRepo(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger").OpenInBrowser();

        private void ReportAbug(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger/issues/new").OpenInBrowser();

        private async Task Changelog(object? parameter)
        {
            await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
            {
                Title = "What's New?",
                Message = "- Rewrote application in C# and Avalonia",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            })!;
        }

        private async Task About(object? parameter)
        {
            await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
            {
                Title = "About",
                Message = "Nickvision Tagger Version 2022.2.0\nAn easy-to-use music tag (metadata) editor.\n\nBuilt with C# and Avalonia\n(C) Nickvision 2021-2022",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            })!;
        }
    }
}