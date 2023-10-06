using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using System;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A row for a synchronized lyric
/// </summary>
public sealed partial class SyncLyricRow : UserControl
{
    /// <summary>
    /// Occurs when a lyric's text is applied
    /// </summary>
    public event EventHandler<string>? LyricApplied;
    /// <summary>
    /// Occurs when a lyric is to be removed
    /// </summary>
    public event EventHandler<EventArgs>? LyricRemoved;

    /// <summary>
    /// Constructs a SyncLyricRow
    /// </summary>
    /// <param name="e">SynchronizedLyricsEventArgs</param>
    public SyncLyricRow(SynchronizedLyricsEventArgs e)
    {
        InitializeComponent();
        Card.Header = e.Timestamp.MillisecondsToTimecode();
        TxtLyric.Text = e.Lyric;
    }

    /// <summary>
    /// Occurs when the TxtLyric's text is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtLyric_TextChanged(object sender, TextChangedEventArgs e) => BtnApply.IsEnabled = !string.IsNullOrEmpty(TxtLyric.Text);

    /// <summary>
    /// Occurs when the apply button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Apply(object sender, RoutedEventArgs e) => LyricApplied?.Invoke(this, TxtLyric.Text);

    /// <summary>
    /// Occurs when the remove button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Remove(object sender, RoutedEventArgs e) => LyricRemoved?.Invoke(this, EventArgs.Empty);
}
