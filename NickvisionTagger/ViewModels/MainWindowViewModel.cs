using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.Extensions;
using Nickvision.Avalonia.Models;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Commands;
using Nickvision.Avalonia.MVVM.Services;
using Nickvision.Avalonia.Update;
using NickvisionTagger.Extensions;
using NickvisionTagger.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace NickvisionTagger.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ICloseable _mainWindowCloseable;
        private readonly ServiceCollection _serviceCollection;
        private readonly HttpClient _httpClient;
        private bool _tagFilenameEnabled;
        private string _tagFilename;
        private string _tagTitle;
        private string _tagArtist;
        private string _tagAlbum;
        private string _tagYear;
        private string _tagTrack;
        private string _tagAlbumArtist;
        private string _tagComposer;
        private string _tagGenre;
        private string _tagComment;
        private string _tagDuration;
        private string _tagFileSize;

        public MusicFolder MusicFolder { get; init; }
        public ObservableCollection<MusicFile> SelectedMusicFiles { get; init; }
        public DelegateAsyncCommand<object?> OpenedCommand { get; init; }
        public DelegateAsyncCommand<object?> OpenMusicFolderCommand { get; init; }
        public DelegateAsyncCommand<object?> RefreshMusicFolderCommand { get; init; }
        public DelegateAsyncCommand<object?> CloseMusicFolderCommand { get; set; }
        public DelegateAsyncCommand<object?> SaveTagsCommand { get; init; }
        public DelegateCommand<object?> ExitCommand { get; init; }
        public DelegateAsyncCommand<object?> RemoveTagsCommand { get; init; }
        public DelegateAsyncCommand<object?> SettingsCommand { get; init; }
        public DelegateAsyncCommand<object?> FilenameToTagCommand { get; init; }
        public DelegateAsyncCommand<object?> TagToFilenameCommand { get; init; }
        public DelegateAsyncCommand<object?> CheckForUpdatesCommand { get; init; }
        public DelegateCommand<object?> GitHubRepoCommand { get; init; }
        public DelegateCommand<object?> ReportABugCommand { get; init; }
        public DelegateAsyncCommand<object?> ChangelogCommand { get; init; }
        public DelegateAsyncCommand<object?> AboutCommand { get; init; }
        public DelegateCommand<IList?> DataMusicFiles_SelectionChangedCommand { get; init; }

        public MainWindowViewModel(ICloseable mainWindowCloseable, ServiceCollection serviceCollection)
        {
            Title = "Nickvision Tagger";
            _mainWindowCloseable = mainWindowCloseable;
            _serviceCollection = serviceCollection;
            _httpClient = new HttpClient();
            _tagFilenameEnabled = true;
            _tagFilename = "";
            _tagTitle = "";
            _tagArtist = "";
            _tagAlbum = "";
            _tagYear = "";
            _tagTrack = "";
            _tagAlbumArtist = "";
            _tagComposer = "";
            _tagGenre = "";
            _tagComment = "";
            _tagDuration = "";
            _tagFileSize = "";
            MusicFolder = new MusicFolder();
            SelectedMusicFiles = new ObservableCollection<MusicFile>();
            OpenedCommand = new DelegateAsyncCommand<object?>(Opened);
            OpenMusicFolderCommand = new DelegateAsyncCommand<object?>(OpenMusicFolder);
            RefreshMusicFolderCommand = new DelegateAsyncCommand<object?>(RefreshMusicFolder, () => MusicFolder.Path != "No Folder Open");
            CloseMusicFolderCommand = new DelegateAsyncCommand<object?>(CloseMusicFolder, () => MusicFolder.Path != "No Folder Open");
            SaveTagsCommand = new DelegateAsyncCommand<object?>(SaveTags, () => SelectedMusicFiles.Count > 0);
            ExitCommand = new DelegateCommand<object?>(Exit);
            RemoveTagsCommand = new DelegateAsyncCommand<object?>(RemoveTags, () => SelectedMusicFiles.Count > 0);
            SettingsCommand = new DelegateAsyncCommand<object?>(Settings);
            FilenameToTagCommand = new DelegateAsyncCommand<object?>(FilenameToTag, () => SelectedMusicFiles.Count > 0);
            TagToFilenameCommand = new DelegateAsyncCommand<object?>(TagToFilename, () => SelectedMusicFiles.Count > 0);
            CheckForUpdatesCommand = new DelegateAsyncCommand<object?>(CheckForUpdates);
            GitHubRepoCommand = new DelegateCommand<object?>(GitHubRepo);
            ReportABugCommand = new DelegateCommand<object?>(ReportAbug);
            ChangelogCommand = new DelegateAsyncCommand<object?>(Changelog);
            AboutCommand = new DelegateAsyncCommand<object?>(About);
            DataMusicFiles_SelectionChangedCommand = new DelegateCommand<IList?>(DataMusicFiles_SelectionChanged);
        }

        public bool TagFilenameEnabled
        {
            get => _tagFilenameEnabled;

            set => SetProperty(ref _tagFilenameEnabled, value);
        }

        public string TagFilename
        {
            get => _tagFilename;

            set => SetProperty(ref _tagFilename, value);
        }

        public string TagTitle
        {
            get => _tagTitle;

            set => SetProperty(ref _tagTitle, value);
        }

        public string TagArtist
        {
            get => _tagArtist;

            set => SetProperty(ref _tagArtist, value);
        }

        public string TagAlbum
        {
            get => _tagAlbum;

            set => SetProperty(ref _tagAlbum, value);
        }

        public string TagYear
        {
            get => _tagYear;

            set => SetProperty(ref _tagYear, value);
        }

        public string TagTrack
        {
            get => _tagTrack;

            set => SetProperty(ref _tagTrack, value);
        }

        public string TagAlbumArtist
        {
            get => _tagAlbumArtist;

            set => SetProperty(ref _tagAlbumArtist, value);
        }

        public string TagComposer
        {
            get => _tagComposer;

            set => SetProperty(ref _tagComposer, value);
        }

        public string TagGenre
        {
            get => _tagGenre;

            set => SetProperty(ref _tagGenre, value);
        }

        public string TagComment
        {
            get => _tagComment;

            set => SetProperty(ref _tagComment, value);
        }

        public string TagDuration
        {
            get => _tagDuration;

            set => SetProperty(ref _tagDuration, value);
        }

        public string TagFileSize
        {
            get => _tagFileSize;

            set => SetProperty(ref _tagFileSize, value);
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
            if (config.RememberLastOpenedFolder)
            {
                MusicFolder.Path = config.LastOpenedFolder;
                RefreshMusicFolderCommand.RaiseCanExecuteChanged();
                CloseMusicFolderCommand.RaiseCanExecuteChanged();
                await RefreshMusicFolder(null);
            }
        }

        private async Task OpenMusicFolder(object? parameter)
        {
            var result = await _serviceCollection.GetService<IIOService>()?.ShowOpenFolderDialogAsync("Select Music Folder")!;
            if (result != null)
            {
                MusicFolder.Path = result;
                RefreshMusicFolderCommand.RaiseCanExecuteChanged();
                CloseMusicFolderCommand.RaiseCanExecuteChanged();
                var config = await Configuration.LoadAsync();
                if (config.RememberLastOpenedFolder)
                {
                    config.LastOpenedFolder = MusicFolder.Path;
                    await config.SaveAsync();
                }
                await RefreshMusicFolder(null);
            }
        }

        private async Task RefreshMusicFolder(object? parameter) => await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Loading music files...", async () => await MusicFolder.ReloadFilesAsync())!;

        private async Task CloseMusicFolder(object? parameter)
        {
            MusicFolder.CloseFolder();
            RefreshMusicFolderCommand.RaiseCanExecuteChanged();
            CloseMusicFolderCommand.RaiseCanExecuteChanged();
            var config = await Configuration.LoadAsync();
            if (config.RememberLastOpenedFolder)
            {
                config.LastOpenedFolder = MusicFolder.Path;
                await config.SaveAsync();
            }
        }

        private async Task SaveTags(object? parameter)
        {
            await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Saving tags...", async () =>
            {
                await Task.Run(() =>
                {
                    foreach (var musicFile in SelectedMusicFiles)
                    {
                        if (TagFilename != musicFile.Filename && TagFilename != "<keep>")
                        {
                            musicFile.Filename = TagFilename;
                        }
                        if (TagTitle != "<keep>")
                        {
                            musicFile.Title = TagTitle;
                        }
                        if (TagArtist != "<keep>")
                        {
                            musicFile.Artist = TagArtist;
                        }
                        if (TagAlbum != "<keep>")
                        {
                            musicFile.Album = TagAlbum;
                        }
                        if (TagYear != "<keep>")
                        {
                            musicFile.Year = string.IsNullOrEmpty(TagYear) ? null : int.Parse(TagYear);
                        }
                        if (TagTrack != "<keep>")
                        {
                            musicFile.Track = string.IsNullOrEmpty(TagTrack) ? null : int.Parse(TagYear);
                        }
                        if (TagAlbumArtist != "<keep>")
                        {
                            musicFile.AlbumArtist = TagAlbumArtist;
                        }
                        if (TagComposer != "<keep>")
                        {
                            musicFile.Composer = TagComposer;
                        }
                        if (TagGenre != "<keep>")
                        {
                            musicFile.Genre = TagGenre;
                        }
                        if (TagComment != "<keep>")
                        {
                            musicFile.Comment = TagComment;
                        }
                        musicFile.SaveTag();
                    }
                });
            })!;
            await RefreshMusicFolder(null);
        }

        private void Exit(object? parameter) => _mainWindowCloseable.Close();

        private async Task RemoveTags(object? parameter)
        {
            await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Removing tags...", async () =>
            {
                await Task.Run(() =>
                {
                    foreach (var musicFile in SelectedMusicFiles)
                    {
                        musicFile.RemoveTag();
                    }
                });
            })!;
            await RefreshMusicFolder(null);
        }

        private async Task Settings(object? parameter) => await _serviceCollection.GetService<IContentDialogService>()?.ShowCustomAsync(new SettingsDialogViewModel(_serviceCollection))!;

        private async Task FilenameToTag(object? parameter)
        {
            var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%title%" };
            var result = await _serviceCollection.GetService<IComboBoxDialogService>()?.ShowAsync("Filename to Tag", "Please select format string", formatStrings)!;
            if (result != null)
            {
                int success = 0;
                await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Converting filenames to tags...", async () =>
                {
                    await Task.Run(() =>
                    {
                        foreach (var musicFile in SelectedMusicFiles)
                        {
                            try
                            {
                                musicFile.FilenameToTag(result);
                                success++;
                            }
                            catch { }
                        }
                    });
                })!;
                _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("Conversion Complete", $"Converted {success} out of {SelectedMusicFiles.Count} filenames to tags.", InfoBarSeverity.Success);
                await RefreshMusicFolder(null);
            }
        }

        private async Task TagToFilename(object? parameter)
        {
            var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%title%" };
            var result = await _serviceCollection.GetService<IComboBoxDialogService>()?.ShowAsync("Tag to Fijlename", "Please select format string", formatStrings)!;
            if (result != null)
            {
                int success = 0;
                await _serviceCollection.GetService<IProgressDialogService>()?.ShowAsync("Converting tags to filenames...", async () =>
                {
                    await Task.Run(() =>
                    {
                        foreach (var musicFile in SelectedMusicFiles)
                        {
                            try
                            {
                                musicFile.TagToFilename(result);
                                success++;
                            }
                            catch { }
                        }
                    });
                })!;
                _serviceCollection.GetService<IInfoBarService>()?.ShowCloseableNotification("Conversion Complete", $"Converted {success} out of {SelectedMusicFiles.Count} tags to filenames.", InfoBarSeverity.Success);
                await RefreshMusicFolder(null);
            }
        }

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

        private void GitHubRepo(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger").OpenInBrowser();

        private void ReportAbug(object? parameter) => new Uri("https://github.com/nlogozzo/NickvisionTagger/issues/new").OpenInBrowser();

        private async Task Changelog(object? parameter)
        {
            await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
            {
                Title = "What's New?",
                Message = "- Rewrote application in C# and Avalonia\n\nNew in Alpha 2:\n- Added icon",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            })!;
        }

        private async Task About(object? parameter)
        {
            await _serviceCollection.GetService<IContentDialogService>()?.ShowMessageAsync(new ContentDialogMessageInfo()
            {
                Title = "About",
                Message = "Nickvision Tagger Version 2022.2.0-alpha2\nAn easy-to-use music tag (metadata) editor.\n\nBuilt with C# and Avalonia\n(C) Nickvision 2021-2022",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            })!;
        }

        private void DataMusicFiles_SelectionChanged(IList? selectedItems)
        {
            SelectedMusicFiles.Clear();
            if(selectedItems != null)
            {
                foreach (var item in selectedItems)
                {
                    SelectedMusicFiles.Add(item as MusicFile);
                }
            }
            SaveTagsCommand.RaiseCanExecuteChanged();
            RemoveTagsCommand.RaiseCanExecuteChanged();
            FilenameToTagCommand.RaiseCanExecuteChanged();
            TagToFilenameCommand.RaiseCanExecuteChanged();
            TagFilenameEnabled = true;
            if (SelectedMusicFiles.Count == 0)
            {
                TagFilename = "";
                TagTitle = "";
                TagArtist = "";
                TagAlbum = "";
                TagYear = "";
                TagTrack = "";
                TagAlbumArtist = "";
                TagComposer = "";
                TagGenre = "";
                TagComment = "";
                TagDuration = "";
                TagFileSize = "";
            }
            else if (SelectedMusicFiles.Count == 1)
            {
                var musicFile = SelectedMusicFiles[0];
                TagFilename = musicFile.Filename;
                TagTitle = musicFile.Title;
                TagArtist = musicFile.Artist;
                TagAlbum = musicFile.Album;
                TagYear = musicFile.Year == null ? "" : musicFile.Year.ToString();
                TagTrack = musicFile.Track == null ? "" : musicFile.Track.ToString();
                TagAlbumArtist = musicFile.AlbumArtist;
                TagAlbumArtist = musicFile.Composer;
                TagGenre = musicFile.Genre;
                TagComment = musicFile.Comment;
                TagDuration = musicFile.DurationAsString;
                TagFileSize = musicFile.FileSizeAsString;
            }
            else
            {
                var firstMusicFile = SelectedMusicFiles[0];
                TagFilenameEnabled = false;
                TagFilename = "<keep>";
                var haveSameTitle = true;
                var haveSameArtist = true;
                var haveSameAlbum = true;
                var haveSameYear = true;
                var haveSameTrack = true;
                var haveSameAlbumArtist = true;
                var haveSameComposer = true;
                var haveSameGenre = true;
                var haveSameComment = true;
                var totalDuration = 0;
                long totalFileSize = 0;
                foreach (var musicFile in SelectedMusicFiles)
                {
                    if (firstMusicFile.Title != musicFile.Title)
                    {
                        haveSameTitle = false;
                    }
                    if (firstMusicFile.Artist != musicFile.Artist)
                    {
                        haveSameArtist = false;
                    }
                    if (firstMusicFile.Album != musicFile.Album)
                    {
                        haveSameAlbum = false;
                    }
                    if (firstMusicFile.Year != musicFile.Year)
                    {
                        haveSameYear = false;
                    }
                    if (firstMusicFile.Track != musicFile.Track)
                    {
                        haveSameTrack = false;
                    }
                    if (firstMusicFile.AlbumArtist != musicFile.AlbumArtist)
                    {
                        haveSameAlbumArtist = false;
                    }
                    if (firstMusicFile.Composer != musicFile.Composer)
                    {
                        haveSameComposer = false;
                    }
                    if (firstMusicFile.Genre != musicFile.Genre)
                    {
                        haveSameGenre = false;
                    }
                    if (firstMusicFile.Comment != musicFile.Comment)
                    {
                        haveSameComment = false;
                    }
                    totalDuration += musicFile.Duration;
                    totalFileSize += musicFile.FileSize;
                }
                TagTitle = haveSameTitle ? firstMusicFile.Title : "<keep>";
                TagArtist = haveSameArtist ? firstMusicFile.Artist : "<keep>";
                TagAlbum = haveSameAlbum ? firstMusicFile.Album : "<keep>";
                TagYear = haveSameYear ? (firstMusicFile.Year == null ? "" : firstMusicFile.Year.ToString()) : "<keep>";
                TagTrack = haveSameTrack ? (firstMusicFile.Track == null ? "" : firstMusicFile.Track.ToString()) : "<keep>";
                TagAlbumArtist = haveSameAlbumArtist ? firstMusicFile.AlbumArtist : "<keep>";
                TagGenre = haveSameGenre ? firstMusicFile.Genre : "<keep>";
                TagComment = haveSameComment ? firstMusicFile.Comment : "<keep>";
                TagDuration = totalDuration.DurationToString();
                TagFileSize = totalFileSize.FileSizeToString();
            }
        }
    }
}