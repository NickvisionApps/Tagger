using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Models;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A row for a MusicFile
/// </summary>
public sealed partial class MusicFileRow : UserControl
{
    /// <summary>
    /// Constructs a MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public MusicFileRow(MusicFile musicFile)
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(musicFile.Title))
        {
            Title = $"{(musicFile.Track != 0 ? $"{musicFile.Track:D2} - " : "")}{musicFile.Title}";
            Subtitle = musicFile.Filename;
        }
        else
        {
            Title = musicFile.Filename;
            Subtitle = "";
            TxtSubtitle.Visibility = Visibility.Collapsed;
        }
        ShowUnsaveIcon = false;
    }

    /// <summary>
    /// Whether or not to show the icon for unsaved changes
    /// </summary>
    public bool ShowUnsaveIcon
    {
        get => UnsaveIcon.Visibility == Visibility.Visible;

        set => UnsaveIcon.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// The title of the row
    /// </summary>
    public string Title
    {
        get => TxtTitle.Text;

        set => TxtTitle.Text = value;
    }

    /// <summary>
    /// The subtitle of the row
    /// </summary>
    public string Subtitle
    {
        get => TxtSubtitle.Text;

        set
        {
            TxtSubtitle.Text = value;
            TxtSubtitle.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
