using NickvisionTagger.GNOME.Helpers;

namespace NickvisionTagger.GNOME.Controls;

public partial class ScrollingMessageDialog : Adw.Window
{
    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.Label _messageLabel;

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="title">The title for the window</param>
    /// <param name="message">The message for the window</param>
    private ScrollingMessageDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string title, string message) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _title.SetTitle(title);
        _messageLabel.SetLabel(message);
    }

    /// <summary>
    /// Constructs a CorruptedFilesDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="title">The title for the window</param>
    /// <param name="message">The message for the window</param>
    public ScrollingMessageDialog(Gtk.Window parent, string iconName, string title, string message) : this(Builder.FromFile("scrolling_message_dialog.ui"), parent, iconName, title, message)
    {
    }
}