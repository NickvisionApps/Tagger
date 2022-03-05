using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;

namespace NickvisionTagger.Views;

public class HomePageView : UserControl
{
    public HomePageView()
    {
        AvaloniaXamlLoader.Load(this);
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            FontFamily = new FontFamily("Cantarell");
        }
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
        if (btnFilenameToTag != null)
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
