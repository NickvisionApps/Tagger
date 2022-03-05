using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;

namespace NickvisionTagger.Views;

public class SettingsPageView : UserControl
{
    public SettingsPageView()
    {
        AvaloniaXamlLoader.Load(this);
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            FontFamily = new FontFamily("Cantarell");
        }
    }
}
