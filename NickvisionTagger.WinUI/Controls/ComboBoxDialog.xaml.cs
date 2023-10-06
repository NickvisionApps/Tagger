using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A dialog for showing a message with choices and support for a custom entry
/// </summary>
public sealed partial class ComboBoxDialog : ContentDialog
{
    private readonly string[] _choices;
    private readonly bool _supportCustom;

    /// <summary>
    /// Constructs a ComboBoxDialog
    /// </summary>
    public ComboBoxDialog(string title, string message, string choicesTitle, string[] choices, bool supportCustom, string closeText, string primaryText)
    {
        InitializeComponent();
        _choices = choices;
        _supportCustom = supportCustom;
        if (_supportCustom)
        {
            _choices = _choices.Append(_("Custom")).ToArray();
        }
        //Localize Strings
        Title = title;
        PrimaryButtonText = primaryText;
        CloseButtonText = closeText;
        LblMessage.Text = message;
        CardChoices.Header = choicesTitle;
        CmbChoices.ItemsSource = _choices;
        CardCustom.Header = _("Custom");
        TxtCustom.PlaceholderText = _("Enter custom choice here");
        CmbChoices.SelectedIndex = 0;
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    /// <returns>string</returns>
    public new async Task<string> ShowAsync()
    {
        var result = await base.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (_supportCustom && CmbChoices.SelectedIndex == _choices.Length - 1)
            {
                return TxtCustom.Text;
            }
            return _choices[CmbChoices.SelectedIndex];
        }
        return "";
    }

    /// <summary>
    /// Occurs when the ScrollViewer's size is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SizeChangedEventArgs</param>
    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => StackPanel.Margin = new Thickness(0, 0, ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 14 : 0, 0);

    /// <summary>
    /// Occurs when the CmbChoice's selection changes
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SelectionChangedEventArgs</param>
    private void CmbChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_supportCustom)
        {
            CardCustom.Visibility = CmbChoices.SelectedIndex == _choices.Length - 1 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
