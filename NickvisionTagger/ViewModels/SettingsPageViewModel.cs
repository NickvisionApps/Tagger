using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.Extensions;
using Nickvision.Avalonia.Models;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Commands;
using Nickvision.Avalonia.MVVM.Services;
using NickvisionTagger.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NickvisionTagger.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    private readonly ServiceCollection _serviceCollection;
    private readonly Configuration _configuration;
    private bool _isLightTheme;
    private bool _isDarkTheme;

    public ObservableCollection<AccentColor> AccentColors { get; init; }
    public DelegateCommand<object?> GitHubRepoCommand { get; init; }
    public DelegateCommand<object?> ReportABugCommand { get; init; }
    public DelegateAsyncCommand<object?> ChangelogCommand { get; init; }

    public SettingsPageViewModel(ServiceCollection serviceCollection)
    {
        Title = "Settings";
        _serviceCollection = serviceCollection;
        _configuration = Configuration.Load();
        AccentColors = EnumExtensions.GetObservableCollection<AccentColor>();
        GitHubRepoCommand = new DelegateCommand<object?>(GitHubRepo);
        ReportABugCommand = new DelegateCommand<object?>(ReportAbug);
        ChangelogCommand = new DelegateAsyncCommand<object?>(Changelog);
        if (_configuration.Theme == Theme.Light)
        {
            IsLightTheme = true;
        }
        else
        {
            IsDarkTheme = true;
        }
    }

    public bool IsLightTheme
    {
        get => _isLightTheme;

        set
        {
            SetProperty(ref _isLightTheme, value);
            if (value)
            {
                _configuration.Theme = Theme.Light;
                IsDarkTheme = false;
                _serviceCollection.GetService<IThemeService>()?.ChangeTheme(Theme.Light);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    _serviceCollection.GetService<IThemeService>()?.ForceWin32WindowToTheme();
                }
                _configuration.Save();
            }
        }
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;

        set
        {
            SetProperty(ref _isDarkTheme, value);
            if (value)
            {
                _configuration.Theme = Theme.Dark;
                IsLightTheme = false;
                _serviceCollection.GetService<IThemeService>()?.ChangeTheme(Theme.Dark);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    _serviceCollection.GetService<IThemeService>()?.ForceWin32WindowToTheme();
                }
                _configuration.Save();
            }
        }
    }

    public AccentColor SelectedAccentColor
    {
        get => _configuration.AccentColor;

        set
        {
            _configuration.AccentColor = value;
            _serviceCollection.GetService<IThemeService>()?.ChangeAccentColor(value);
            _configuration.Save();
            OnPropertyChanged();
        }
    }

    public bool IncludeSubfolders
    {
        get => _configuration.IncludeSubfolders;

        set
        {
            _configuration.IncludeSubfolders = value;
            _configuration.Save();
            OnPropertyChanged();
        }
    }

    public bool RememberLastOpenedFolder
    {
        get => _configuration.RememberLastOpenedFolder;

        set
        {
            _configuration.RememberLastOpenedFolder = value;
            _configuration.Save();
            OnPropertyChanged();
        }
    }

    private void GitHubRepo(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger").OpenInBrowser();

    private void ReportAbug(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger/issues/new").OpenInBrowser();

    private async Task Changelog(object? parameter)
    {
        await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
        {
            Title = "What's New?",
            Message = "- Major new design\n- Added file counter to DataGrid\n- Refreshed icon",
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        })!;
    }
}
