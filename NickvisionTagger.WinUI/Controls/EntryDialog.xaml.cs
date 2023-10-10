using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A dialog for showing a message with an entry row
/// </summary>
public sealed partial class EntryDialog : ContentDialog
{
    /// <summary>
    /// A function used to check the validity of the entry
    /// </summary>
    public Func<string, bool>? Validator { get; set; }

    /// <summary>
    /// Constructs an EntryDialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="entryTitle">The title of the entry of the dialog</param>
    /// <param name="closeText">The text of the close button</param>
    /// <param name="primaryText">The text of the primary button</param>
    public EntryDialog(string title, string message, string entryTitle, string closeText, string primaryText)
    {
        InitializeComponent();
        //Localize Strings
        Title = title;
        PrimaryButtonText = primaryText;
        CloseButtonText = closeText;
        if (string.IsNullOrEmpty(message))
        {
            LblMessage.Visibility = Visibility.Collapsed;
        }
        else
        {
            LblMessage.Text = message;
        }
        CardEntry.Header = entryTitle;
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
            return string.IsNullOrEmpty(TxtEntry.Text) ? "NULL" : TxtEntry.Text;
        }
        return "";
    }

    /// <summary>
    /// Occurs when the TxtEntry's text is changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">TextChangedEventArgs</param>
    private void TxtEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Validator != null)
        {
            IsPrimaryButtonEnabled = Validator(TxtEntry.Text);
        }
    }
}
