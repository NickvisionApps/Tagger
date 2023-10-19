using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Models;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.WinUI.Controls;

public sealed partial class AlbumArtInfoDialog : ContentDialog
{
    public AlbumArtInfoDialog(AlbumArt art)
    {
        InitializeComponent();
        //Localize Strings
        Title = art.Type == AlbumArtType.Front ? _("Front") : _("Back");
        CloseButtonText = _("Close");
        CardMimeType.Header = _("Mime Type");
        CardWidth.Header = _("Width");
        CardHeight.Header = _("Height");
        //Load
        LblMimeType.Text = art.MimeType;
        LblWidth.Text = _("{0} pixels", art.Width);
        LblHeight.Text = _("{0} pixels", art.Height);
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);
}
