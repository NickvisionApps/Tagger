using System;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog for showing a message with an entry row
/// </summary>
public partial class EntryDialog
{
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.PreferencesGroup _group;
    private readonly Adw.EntryRow _entryRow;

    /// <summary>
    /// The response of the dialog
    /// </summary>
    public string Response { get; private set; }

    /// <summary>
    /// A validator of the entry
    /// </summary>
    public Func<string, bool>? Validator { get; set; }

    /// <summary>
    /// Whether or not the dialog is visible
    /// </summary>
    public bool Visible => _dialog.GetVisible();

    /// <summary>
    /// Constructs a EntryDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="iconName">The name of the icon of the dialog</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="entryTitle">The title of the entry of the dialog</param>
    /// <param name="cancelText">The text of the cancel button</param>
    /// <param name="suggestedText">The text of the suggested button</param>
    public EntryDialog(Gtk.Window parentWindow, string iconName, string title, string message, string entryTitle, string cancelText, string suggestedText)
    {
        Response = "";
        _dialog = Adw.MessageDialog.New(parentWindow, title, message);
        _dialog.SetHideOnClose(true);
        _dialog.SetIconName(iconName);
        _group = Adw.PreferencesGroup.New();
        _entryRow = Adw.EntryRow.New();
        _entryRow.SetSizeRequest(300, -1);
        _entryRow.SetTitle(entryTitle);
        _entryRow.SetActivatesDefault(true);
        _entryRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text" && Validator != null)
            {
                _dialog.SetResponseEnabled("suggested", Validator(_entryRow.GetText()));
            }
        };
        _group.Add(_entryRow);
        _dialog.SetExtraChild(_group);
        _dialog.AddResponse("cancel", cancelText);
        _dialog.AddResponse("suggested", suggestedText);
        _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        _dialog.SetDefaultResponse("suggested");
        _dialog.SetCloseResponse("cancel");
        _dialog.OnResponse += (sender, e) => SetResponse(e.Response);
    }

    public event GObject.SignalHandler<Adw.MessageDialog, Adw.MessageDialog.ResponseSignalArgs> OnResponse
    {
        add
        {
            _dialog.OnResponse += value;
        }
        remove
        {
            _dialog.OnResponse -= value;
        }
    }

    /// <summary>
    /// Presents the dialog
    /// </summary>
    public void Present() => _dialog.Present();

    /// <summary>
    /// Destroys the dialog
    /// </summary>
    public void Destroy() => _dialog.Destroy();

    /// <summary>
    /// Sets the response of the dialog as a MessageDialogResponse
    /// </summary>
    /// <param name="response">The string response of the dialog</param>
    private void SetResponse(string response)
    {
        Response = response switch
        {
            "suggested" => string.IsNullOrEmpty(_entryRow.GetText()) ? "NULL" : _entryRow.GetText(),
            _ => ""
        };
    }
}