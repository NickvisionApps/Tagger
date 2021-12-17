using Nickvision.Avalonia.Models;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Commands;
using Nickvision.Avalonia.MVVM.Services;
using NickvisionTagger.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NickvisionTagger.ViewModels
{
    public class SettingsDialogViewModel : ViewModelBase
    {
        private ServiceCollection _serviceCollection;
        private Configuration _configuration;
        private bool _isLightTheme;
        private bool _isDarkTheme;
        private bool _isSystemTheme;
        private AccentColor _accentColor;
        private bool _includeSubfolders;
        private bool _rememberLastOpenedFolder;

        public bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;
        public ObservableCollection<AccentColor> AccentColors { get; init; }
        public DelegateCommand<object> OpenedCommand { get; init; }
        public DelegateAsyncCommand<object> ClosedCommand { get; init; }

        public SettingsDialogViewModel(ServiceCollection serviceCollection)
        {
            Title = "Settings";
            _serviceCollection = serviceCollection;
            _configuration = Configuration.Load();
            AccentColors = new ObservableCollection<AccentColor>();
            OpenedCommand = new DelegateCommand<object>(Opened);
            ClosedCommand = new DelegateAsyncCommand<object>(Closed);
        }

        private void Opened(object parameter)
        {
            if (IsWindows)
            {
                AccentColors.Add(AccentColor.System);
            }
            AccentColors.Add(AccentColor.Red);
            AccentColors.Add(AccentColor.Orange);
            AccentColors.Add(AccentColor.Yellow);
            AccentColors.Add(AccentColor.Green);
            AccentColors.Add(AccentColor.Blue);
            AccentColors.Add(AccentColor.Purple);
            AccentColors.Add(AccentColor.Pink);
            AccentColors.Add(AccentColor.White);
            AccentColors.Add(AccentColor.Brown);
            AccentColors.Add(AccentColor.Black);
            if (_configuration.Theme == Theme.Light)
            {
                IsLightTheme = true;
            }
            else if (_configuration.Theme == Theme.Dark)
            {
                IsDarkTheme = true;
            }
            else
            {
                IsSystemTheme = true;
            }
            AccentColor = _configuration.AccentColor;
            IncludeSubfolders = _configuration.IncludeSubfolders;
            RememberLastOpenedFolder = _configuration.RememberLastOpenedFolder;
        }

        private async Task Closed(object parameter)
        {
            if (IsLightTheme)
            {
                _configuration.Theme = Theme.Light;
            }
            else if (IsDarkTheme)
            {
                _configuration.Theme= Theme.Dark;
            }
            else
            {
                _configuration.Theme = Theme.System;
            }
            _configuration.AccentColor = AccentColor;
            _configuration.IncludeSubfolders = IncludeSubfolders;
            _configuration.RememberLastOpenedFolder = RememberLastOpenedFolder;
            await _configuration.SaveAsync();
        }

        public bool IsLightTheme
        {
            get => _isLightTheme;

            set
            {
                SetProperty(ref _isLightTheme, value);
                if (value)
                {
                    _serviceCollection.GetService<IThemeService>().ChangeTheme(Theme.Light);
                    IsDarkTheme = false;
                    IsSystemTheme = false;
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
                    _serviceCollection.GetService<IThemeService>().ChangeTheme(Theme.Dark);
                    IsLightTheme = false;
                    IsSystemTheme = false;
                }
            }
        }

        public bool IsSystemTheme
        {
            get => _isSystemTheme;

            set
            {
                SetProperty(ref _isSystemTheme, value);
                if (value)
                {
                    _serviceCollection.GetService<IThemeService>().ChangeTheme(Theme.System);
                    IsLightTheme = false;
                    IsDarkTheme = false;
                }
            }
        }

        public AccentColor AccentColor
        {
            get => _accentColor;

            set
            {
                SetProperty(ref _accentColor, value);
                _serviceCollection.GetService<IThemeService>().ChangeAccentColor(value);
            }
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;

            set => SetProperty(ref _includeSubfolders, value);
        }

        public bool RememberLastOpenedFolder
        {
            get => _rememberLastOpenedFolder;

            set => SetProperty(ref _rememberLastOpenedFolder, value);
        }
    }
}
