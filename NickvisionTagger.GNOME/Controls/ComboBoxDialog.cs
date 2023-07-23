using System;
using System.Linq;
using static NickvisionTagger.Shared.Helpers.Gettext;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog for showing a message with choices and support for a custom entry
/// </summary>
public partial class ComboBoxDialog
{
    private readonly string[] _choices;
    private readonly bool _supportCustom;
    private readonly Adw.MessageDialog _dialog;
    private readonly Adw.PreferencesGroup _group;
    private readonly Adw.ComboRow _choicesRow;
    private readonly Adw.EntryRow _customRow;
    
    /// <summary>
    /// The response of the dialog
    /// </summary>
    public string Response { get; private set; }
    
    /// <summary>
    /// Whether or not the dialog is visible
    /// </summary>
    public bool Visible => _dialog.GetVisible();
    
    /// <summary>
    /// Constructs a ComboBoxDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="iconName">The name of the icon of the dialog</param>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="choicesTitle">The title of the choices of the dialog</param>
    /// <param name="choices">The choices of the dialog</param>
    /// <param name="cancelText">The text of the cancel button</param>
    /// <param name="suggestedText">The text of the suggested button</param>
    public ComboBoxDialog(Gtk.Window parentWindow, string iconName, string title, string message, string choicesTitle, string[] choices, bool supportCustom, string cancelText, string suggestedText)
    {
        Response = "";
        _choices = choices;
        _supportCustom = supportCustom;
        if(_supportCustom)
        {
            _choices = _choices.Append(_("Custom")).ToArray();
        }
        _dialog = Adw.MessageDialog.New(parentWindow, title, message);
        _dialog.SetHideOnClose(true);
        _dialog.SetIconName(iconName);
        _group = Adw.PreferencesGroup.New();
        _choicesRow = Adw.ComboRow.New();
        _choicesRow.SetSizeRequest(300, -1);
        _choicesRow.SetTitle(choicesTitle);
        _choicesRow.SetModel(Gtk.StringList.New(_choices));
        _group.Add(_choicesRow);
        _customRow = Adw.EntryRow.New();
        _customRow.SetTitle(_("Custom"));
        _customRow.SetVisible(false);
        if(_supportCustom)
        {
            _group.Add(_customRow);
            _choicesRow.OnNotify += (sender, e) =>
            {
                if(e.Pspec.GetName() == "selected-item")
                {
                    _customRow.SetVisible((int)_choicesRow.GetSelected() == _choices.Length - 1);
                }
            };
        }
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
        if(response == "suggested")
        {
            if(_supportCustom && (int)_choicesRow.GetSelected() == _choices.Length - 1)
            {
                Response = _customRow.GetText();
            }
            else
            {
                Response = _choices[(int)_choicesRow.GetSelected()];
            }
        }
        else
        {
            Response = "";
        }
    }
}