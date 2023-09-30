using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.WinUI.Controls;

/// <summary>
/// A row for a custom property
/// </summary>
public sealed partial class CustomPropertyRow : UserControl
{
    /// <summary>
    /// Constructs a CustomPropertyRow
    /// </summary>
    /// <param name="musicFile">KeyValuePair of property</param>
    public CustomPropertyRow(KeyValuePair<string, string> custom)
    {
        InitializeComponent();
        TxtProp.Header = custom.Key;
        TxtProp.Text = custom.Value;
        ToolTipService.SetToolTip(BtnRemove, _("Remove Custom Property"));
    }

    /// <summary>
    /// Occurs when the property's text value changes
    /// </summary>
    public event TextChangedEventHandler TextChanged
    {
        add => TxtProp.TextChanged += value;

        remove => TxtProp.TextChanged -= value;
    }

    /// <summary>
    /// Occurs when the remove property button is clicked
    /// </summary>
    public event RoutedEventHandler RemoveClicked
    {
        add => BtnRemove.Click += value;

        remove => BtnRemove.Click -= value;
    }
}
