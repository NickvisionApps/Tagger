using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NickvisionTagger.Shared.Controllers;
using NickvisionTagger.Shared.Events;
using NickvisionTagger.Shared.Helpers;
using NickvisionTagger.WinUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionTagger.WinUI.Views;

/// <summary>
/// A dialog for managing lyrics of a music file
/// </summary>
public sealed partial class LyricsDialog : ContentDialog
{
    private readonly LyricsDialogController _controller;
    private readonly Action<object> _initializeWithWindow;
    private readonly Dictionary<int, SyncLyricRow> _syncRows;
    private bool _showingAnotherDialog;

    /// <summary>
    /// Constructs a LyricsDialog
    /// </summary>
    /// <param name="controller">LyricsDialogController</param>
    /// <param name="initializeWithWindow">Action</param>
    public LyricsDialog(LyricsDialogController controller, Action<object> initializeWithWindow)
    {
        InitializeComponent();
        _controller = controller;
        _initializeWithWindow = initializeWithWindow;
        _syncRows = new Dictionary<int, SyncLyricRow>();
        _showingAnotherDialog = false;
        //Localize Strings
        Title = _("Lryics");
        PrimaryButtonText = _("Apply");
        CloseButtonText = _("Cancel");
        LblConfigure.Text = _("Configure");
        CardType.Header = _("Type");
        CmbType.Items.Add(_("Unsynchronized"));
        CmbType.Items.Add(_("Synchronized"));
        CardLanguageCode.Header = _("Language Code");
        TxtLanguageCode.PlaceholderText = _("Enter language code here");
        CardDescription.Header = _("Description");
        TxtDescription.PlaceholderText = _("Enter description here");
        CardOffset.Header = _("Offset (in milliseconds)");
        TxtOffset.PlaceholderText = _("Enter offset here");
        ToolTipService.SetToolTip(BtnApplyOffset, _("Apply"));
        LblEdit.Text = _("Edit");
        LblBtnAddSyncLyric.Text = _("Add");
        ToolTipService.SetToolTip(BtnClearAllSyncLyrics, _("Clear All Lyrics"));
        ToolTipService.SetToolTip(BtnImportLRC, _("Import from LRC"));
        ToolTipService.SetToolTip(BtnExportLRC, _("Export to LRC"));
        //Register Events
        _controller.SynchronizedLyricCreated += CreateSyncRow;
        _controller.SynchronizedLyricRemoved += RemoveSyncRow;
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    /// <returns>ContentDialogResult</returns>
    public new async Task<ContentDialogResult> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (_showingAnotherDialog)
        {
            while (_showingAnotherDialog)
            {
                await Task.Delay(100);
            }
            result = await base.ShowAsync();
        }
        if (result == ContentDialogResult.Primary)
        {
            _controller.LanguageCode = TxtLanguageCode.Text;
            _controller.Description = TxtDescription.Text;
            _controller.UnsynchronizedLyrics = TxtUnsync.Text;
        }
        return result;
    }

    /// <summary>
    /// Occurs when the dialog is loaded
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        _controller.Startup();
        CmbType.SelectedIndex = (int)_controller.LyricsType;
        TxtLanguageCode.Text = _controller.LanguageCode;
        TxtDescription.Text = _controller.Description;
        if (_controller.LyricsType == LyricsType.Unsynchronized)
        {
            CardOffset.Visibility = Visibility.Collapsed;
            SyncControls.Visibility = Visibility.Collapsed;
            EditViewStack.CurrentPageName = "Unsync";
            TxtUnsync.Text = _controller.UnsynchronizedLyrics;
        }
        else
        {
            CardOffset.Visibility = Visibility.Visible;
            TxtOffset.Text = _controller.SynchronizedLyricsOffset!.Value.ToString();
            SyncControls.Visibility = Visibility.Visible;
            EditViewStack.CurrentPageName = "Sync";
        }
    }

    /// <summary>
    /// Occurs when a sync lyric is created
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SynchronizedLyricsEventArgs</param>
    private void CreateSyncRow(object? sender, SynchronizedLyricsEventArgs e)
    {
        if (!_syncRows.ContainsKey(e.Timestamp))
        {
            var row = new SyncLyricRow(e);
            row.LyricApplied += (s, ea) => _controller.SetSynchronizedLyric(e.Timestamp, ea);
            row.LyricRemoved += (s, ea) => _controller.RemoveSynchronizedLyric(e.Timestamp);
            ListSync.Children.Add(row);
            _syncRows.Add(e.Timestamp, row);
        }
    }

    /// <summary>
    /// Occurs when a sync lyric is removed
    /// </summary>
    /// <param name="sender">object?</param>
    /// <param name="e">SynchronizedLyricsEventArgs</param>
    private void RemoveSyncRow(object? sender, SynchronizedLyricsEventArgs e)
    {
        if (_syncRows.ContainsKey(e.Timestamp))
        {
            ListSync.Children.Remove(_syncRows[e.Timestamp]);
            _syncRows.Remove(e.Timestamp);
        }
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the CmbType's selection is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var newType = (LyricsType)CmbType.SelectedIndex;
        if (_controller.LyricsType != newType)
        {
            _controller.LyricsType = newType;
            ListSync.Children.Clear();
            _syncRows.Clear();
            TxtLanguageCode.Text = _controller.LanguageCode;
            TxtDescription.Text = _controller.Description;
            if (_controller.LyricsType == LyricsType.Unsynchronized)
            {
                CardOffset.Visibility = Visibility.Collapsed;
                SyncControls.Visibility = Visibility.Collapsed;
                EditViewStack.CurrentPageName = "Unsync";
                TxtUnsync.Text = _controller.UnsynchronizedLyrics;
            }
            else
            {
                CardOffset.Visibility = Visibility.Visible;
                TxtOffset.Text = _controller.SynchronizedLyricsOffset!.Value.ToString();
                SyncControls.Visibility = Visibility.Visible;
                EditViewStack.CurrentPageName = "Sync";
            }
        }
    }

    /// <summary>
    /// Occurs when TxtOffset's text is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtOffset_TextChanged(object sender, TextChangedEventArgs e) => BtnApplyOffset.IsEnabled = !string.IsNullOrEmpty(TxtOffset.Text);

    /// <summary>
    /// Occurs when the apply offset button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ApplyOffset(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtOffset.Text, out var offset))
        {
            _controller.SynchronizedLyricsOffset = offset;
        }
        else
        {
            TxtOffset.Text = _controller.SynchronizedLyricsOffset!.Value.ToString();
            TxtOffset.Select(TxtOffset.Text.Length, 0);
        }
    }

    /// <summary>
    /// Occurs when the add sync lyric button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void AddSyncLyric(object sender, RoutedEventArgs e)
    {
        var entryDialog = new EntryDialog(_("New Synchronized Lyric"), "", _("Timestamp (hh:mm:ss or mm:ss.xx)"), _("Cancel"), _("Add"))
        {
            Validator = x => x.TimecodeToMs() != -1,
            XamlRoot = XamlRoot
        };
        _showingAnotherDialog = true;
        Hide();
        var res = await entryDialog.ShowAsync();
        _showingAnotherDialog = false;
        if (!string.IsNullOrEmpty(res) && res != "NULL")
        {
            var ms = res.TimecodeToMs();
            if (ms != -1)
            {
                _controller.AddSynchronizedLyric(ms);
            }
        }
    }

    /// <summary>
    /// Occurs when the clear all sync lyrics button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private void ClearAllSyncLyrics(object sender, RoutedEventArgs e) => _controller.ClearSynchronizedLyrics();

    /// <summary>
    /// Occurs when the import sync to lrc button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ImportSyncFromLRC(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        _initializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        filePicker.FileTypeFilter.Add(".lrc");
        filePicker.FileTypeFilter.Add(".LRC");
        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            var dialog = new ContentDialog()
            {
                Title = _("Existing Lyrics"),
                Content = _("What would you like Tagger to do with lyrics found from the LRC file that conflict with existing lyrics of the same timestamp?"),
                PrimaryButtonText = _("Keep Tagger's lyrics"),
                SecondaryButtonText = _("Use LRC's lyrics"),
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            var overwrite = true;
            if (_controller.SynchronizedLyricsCount!.Value > 0)
            {
                _showingAnotherDialog = true;
                Hide();
                var res = await dialog.ShowAsync();
                _showingAnotherDialog = false;
                overwrite = res == ContentDialogResult.Secondary;
            }
            if (_controller.ImportFromLRC(file.Path, overwrite))
            {
                TxtOffset.Text = _controller.SynchronizedLyricsOffset!.Value.ToString();
            }
            else
            {
                InfoBar.Title = _("Unable to import from LRC.");
                InfoBar.Severity = InfoBarSeverity.Error;
                InfoBar.IsOpen = true;
            }
        }
    }

    /// <summary>
    /// Occurs when the export sync to lrc button is clicked
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">RoutedEventArgs</param>
    private async void ExportSyncToLRC(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileSavePicker();
        _initializeWithWindow(filePicker);
        filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
        filePicker.FileTypeChoices.Add(_("All files"), new List<string>() { ".lrc", ".LRC" });
        filePicker.DefaultFileExtension = ".lrc";
        var file = await filePicker.PickSaveFileAsync();
        if (file != null)
        {
            var path = file.Path;
            if (Path.GetExtension(path).ToLower() != ".lrc")
            {
                path += ".lrc";
            }
            if (_controller.ExportToLRC(path))
            {
                InfoBar.Title = string.Format(_("Exported successfully to: {0}"), path);
                InfoBar.Severity = InfoBarSeverity.Success;
                InfoBar.IsOpen = true;
            }
            else
            {
                InfoBar.Title = _("Unable to export to LRC.");
                InfoBar.Severity = InfoBarSeverity.Error;
                InfoBar.IsOpen = true;
            }
        }
    }
}
