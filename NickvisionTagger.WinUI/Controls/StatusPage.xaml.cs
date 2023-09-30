using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A control for displaying a simple page with a title, description, and icon
/// </summary>
public sealed partial class StatusPage : UserControl, INotifyPropertyChanged
{
    public static DependencyProperty GlyphProperty { get; } = DependencyProperty.Register("Glyph", typeof(string), typeof(StatusPage), new PropertyMetadata("", (sender, e) => (sender as StatusPage)?.NotifyPropertyChanged(nameof(Glyph))));
    public static DependencyProperty UseAppIconProperty { get; } = DependencyProperty.Register("UseAppIcon", typeof(bool), typeof(StatusPage), new PropertyMetadata("", (sender, e) => (sender as StatusPage)?.NotifyPropertyChanged(nameof(UseAppIcon))));
    public static DependencyProperty TitleProperty { get; } = DependencyProperty.Register("Title", typeof(string), typeof(StatusPage), new PropertyMetadata("", (sender, e) => (sender as StatusPage)?.NotifyPropertyChanged(nameof(Title))));
    public static DependencyProperty DescriptionProperty { get; } = DependencyProperty.Register("Description", typeof(string), typeof(StatusPage), new PropertyMetadata("", (sender, e) => (sender as StatusPage)?.NotifyPropertyChanged(nameof(Description))));
    public static DependencyProperty ChildProperty { get; } = DependencyProperty.Register("Child", typeof(UIElement), typeof(StatusPage), new PropertyMetadata(null, (sender, e) => (sender as StatusPage)?.NotifyPropertyChanged(nameof(Child))));

    public event PropertyChangedEventHandler? PropertyChanged;

    public StatusPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The glyph code for the FontIcon
    /// </summary>
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);

        set
        {
            SetValue(GlyphProperty, value);
            if(!string.IsNullOrEmpty(value))
            {
                GlyphIcon.Visibility = Visibility.Visible;
                AppIcon.Visibility = Visibility.Collapsed;
            }
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Whether or not to use the app icon instead of a glyph
    /// </summary>
    public bool UseAppIcon
    {
        get => (bool)GetValue(UseAppIconProperty);

        set
        {
            SetValue(UseAppIconProperty, value);
            if(value)
            {
                GlyphIcon.Visibility = Visibility.Collapsed;
                AppIcon.Visibility = Visibility.Visible;
            }
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The title of the status
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);

        set
        {
            SetValue(TitleProperty, value);
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The message of the status
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);

        set
        {
            SetValue(DescriptionProperty, value);
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// The extra child of the page
    /// </summary>
    public UIElement? Child
    {
        get => (UIElement?)GetValue(ChildProperty);

        set
        {
            SetValue(ChildProperty, value);
            NotifyPropertyChanged();
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
