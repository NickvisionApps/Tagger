using Avalonia;
using System;

namespace NickvisionTagger
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().With(new Win32PlatformOptions()
        {
            UseWindowsUIComposition = true
        });
    }
}