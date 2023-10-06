using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using NickvisionTagger.Shared.Models;
using System;
using System.Linq;
using Windows.Storage.Streams;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A row for a MusicFile
/// </summary>
public sealed partial class MusicFileRow : UserControl
{
    private byte[] _art;

    /// <summary>
    /// Constructs a MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public MusicFileRow(MusicFile musicFile)
    {
        InitializeComponent();
        _art = Array.Empty<byte>();
        ArtViewStack.CurrentPageName = "NoArt";
        ShowUnsaveIcon = false;
        Update(musicFile);
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

    /// <summary>
    /// The album art of the row
    /// </summary>
    public byte[] Art
    {
        get => _art;

        set
        {
            if (_art.SequenceEqual(value))
            {
                return;
            }
            _art = value;
            if (_art.Length > 0)
            {
                using var ms = new InMemoryRandomAccessStream();
                using var writter = new DataWriter(ms.GetOutputStreamAt(0));
                writter.WriteBytes(_art);
                writter.StoreAsync().GetResults();
                var image = new BitmapImage();
                image.SetSource(ms);
                ArtViewStack.CurrentPageName = "Art";
                ImgArt.Source = image;
            }
            else
            {
                ArtViewStack.CurrentPageName = "NoArt";
                ImgArt.Source = null;
            }
        }
    }

    /// <summary>
    /// Updates the MusicFileRow
    /// </summary>
    /// <param name="musicFile">MusicFile</param>
    public void Update(MusicFile musicFile)
    {
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
        Art = musicFile.FrontAlbumArt;
    }
}
