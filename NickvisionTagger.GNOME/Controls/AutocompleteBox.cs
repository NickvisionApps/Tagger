using NickvisionTagger.GNOME.Helpers;
using System;
using System.Collections.Generic;

namespace NickvisionTagger.GNOME.Controls;

/// <summary>
/// A dialog for showing autocomplete suggestions
/// </summary>
public class AutocompleteBox : Gtk.Box
{
    private readonly Adw.EntryRow _parent;
    private readonly List<Gtk.Widget> _rows;
    private readonly Gtk.EventControllerKey _parentKeyController;
    private bool _canHide;
    
    [Gtk.Connect] private readonly Adw.PreferencesGroup _group;
    
    /// <summary>
    /// Occurs when a suggestion is clicked
    /// </summary>
    /// <remarks>The string argument is the text of the clicked suggestion</remarks>
    public event EventHandler<string>? SuggestionAccepted;
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    private AutocompleteBox(Gtk.Builder builder, Adw.EntryRow parent) : base(builder.GetPointer("_root"), false)
    {
        _parent = parent;
        _rows = new List<Gtk.Widget>();
        _canHide = true;
        //Build UI
        builder.Connect(this);
        _parentKeyController = Gtk.EventControllerKey.New();
        _parentKeyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
        _parentKeyController.OnKeyPressed += (sender, e) =>
        {
            if(e.Keyval == 65293 || e.Keyval == 65421) //enter | keypad enter
            {
                if(GetVisible())
                {
                    AcceptSuggestion(0);
                    return true;
                }
            }
            if(e.Keyval == 65364) //down arrow
            {
                if(GetVisible())
                {
                    _canHide = false;
                    GrabFocus();
                    return true;
                }
            }
            return false;
        };
        _parent.AddController(_parentKeyController);
        _parent.OnStateFlagsChanged += (sender, e) =>
        {
            if(!_canHide)
            {
                _canHide = true;
            }
            else if(e.Flags.HasFlag(Gtk.StateFlags.FocusWithin) && !_parent.GetStateFlags().HasFlag(Gtk.StateFlags.FocusWithin))
            {
                SetVisible(false);
            }
        };
    }
    
    /// <summary>
    /// Constructs an AutocompleteDialog
    /// </summary>
    public AutocompleteBox(Adw.EntryRow parent) : this(Builder.FromFile("autocomplete_box.ui"), parent)
    {
    }
    
    /// <summary>
    /// Grabs focus for the box
    /// </summary>
    public new void GrabFocus() => _rows[0].GrabFocus();

    /// <summary>
    /// Updates the list of suggestions
    /// </summary>
    /// <param name="suggestions">A list of suggestions</param>
    public void UpdateSuggestions(List<string> suggestions)
    {
        foreach(var row in _rows)
        {
            _group.Remove(row);
        }
        _rows.Clear();
        foreach(var suggestion in suggestions)
        {
            var row = Adw.ActionRow.New();
            row.SetTitle(suggestion);
            row.SetActivatable(true);
            row.OnActivated += (sender, e) =>
            {
                SuggestionAccepted?.Invoke(this, suggestion);
                SetVisible(false);
            };
            var keyController = Gtk.EventControllerKey.New();
            keyController.SetPropagationPhase(Gtk.PropagationPhase.Capture);
            keyController.OnKeyPressed += (sender, e) =>
            {
                if(e.Keyval == 65293 || e.Keyval == 65421) //enter | keypad enter
                {
                    row.Activate();
                    return true;
                }
                return false;
            };
            row.AddController(keyController);
            _rows.Add(row);
            _group.Add(row);
        }
    }

    /// <summary>
    /// Accepts a suggestion
    /// </summary>
    /// <param name="index">The index of the suggestion to accept</param>
    public void AcceptSuggestion(int index) => _rows[index].Activate();
}