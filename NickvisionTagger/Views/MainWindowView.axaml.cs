using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Services;
using NickvisionTagger.ViewModels;
using System;

namespace NickvisionTagger.Views;

public class MainWindowView : Window, ICloseable
{
    public MainWindowView()
    {
        AvaloniaXamlLoader.Load(this);
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            FontFamily = new FontFamily("Cantarell");
        }
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddService(new ThemeService(this));
        serviceCollection.AddService(new IOService(this));
        serviceCollection.AddService(new ContentDialogService());
        serviceCollection.AddService(new ProgressDialogService());
        serviceCollection.AddService(new InfoBarService(this.FindControl<InfoBar>("InfoBar")));
        DataContext = new MainWindowViewModel(this, serviceCollection);
    }

    private void BtnRemoveTags_OpenFlyout(object sender, RoutedEventArgs e)
    {
        var btnRemoveTags = this.FindControl<CommandBarButton>("BtnRemoveTags");
        if (btnRemoveTags != null)
        {
            btnRemoveTags.Flyout.ShowAt(btnRemoveTags);
        }
    }

    private void BtnRemoveTags_CloseFlyout(object sender, RoutedEventArgs e)
    {
        var btnRemoveTags = this.FindControl<CommandBarButton>("BtnRemoveTags");
        if (btnRemoveTags != null)
        {
            btnRemoveTags.Flyout.Hide();
        }
    }

    private void BtnFilenameToTag_OpenFlyout(object sender, RoutedEventArgs e)
    {
        var btnFilenameToTag = this.FindControl<CommandBarButton>("BtnFilenameToTag");
        if (btnFilenameToTag != null)
        {
            btnFilenameToTag.Flyout.ShowAt(btnFilenameToTag);
        }
    }

    private void BtnFilenameToTag_CloseFlyout(object sender, RoutedEventArgs e)
    {
        var btnFilenameToTag = this.FindControl<CommandBarButton>("BtnFilenameToTag");
        if(btnFilenameToTag != null)
        {
            btnFilenameToTag.Flyout.Hide();
        }
    }

    private void BtnTagToFilename_OpenFlyout(object sender, RoutedEventArgs e)
    {
        var btnTagToFilename = this.FindControl<CommandBarButton>("BtnTagToFilename");
        if (btnTagToFilename != null)
        {
            btnTagToFilename.Flyout.ShowAt(btnTagToFilename);
        }
    }

    private void BtnTagToFilename_CloseFlyout(object sender, RoutedEventArgs e)
    {
        var btnTagToFilename = this.FindControl<CommandBarButton>("BtnTagToFilename");
        if (btnTagToFilename != null)
        {
            btnTagToFilename.Flyout.Hide();
        }
    }
}
