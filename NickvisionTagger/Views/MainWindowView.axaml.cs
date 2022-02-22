using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Nickvision.Avalonia.MVVM;
using Nickvision.Avalonia.MVVM.Services;
using NickvisionTagger.ViewModels;
using System;

namespace NickvisionTagger.Views
{
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
    }
}