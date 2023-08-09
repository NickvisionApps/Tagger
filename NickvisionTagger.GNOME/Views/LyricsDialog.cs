using NickvisionTagger.GNOME.Helpers;

namespace NickvisionTagger.GNOME.Views;

/// <summary>
/// A dialog for managing lyrics of a music file
/// </summary>
public partial class LyricsDialog : Adw.Window
{
    /// <summary>
    /// Constructs a LyricsDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    private LyricsDialog(Gtk.Builder builder, Gtk.Window parent, string iconName) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
    }

    /// <summary>
    /// Constructs a LyricsDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    public LyricsDialog(Gtk.Window parent, string iconName) : this(Builder.FromFile("lyrics_dialog.ui"), parent, iconName)
    {
        
    }
}