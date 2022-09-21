#include "comboboxdialog.hpp"

using namespace NickvisionTagger::UI::Controls;

ComboBoxDialog::ComboBoxDialog(GtkWindow* parent, const std::string& title, const std::string& description, const std::string& rowTitle, const std::vector<std::string>& choices) : m_choices{ choices }, m_selectedChoice{ "" }, m_gobj{ adw_message_dialog_new(parent, title.c_str(), description.c_str()) }
{
    //Dialog Settings
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", "Cancel", "ok", "OK", nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    //Choices
    m_rowChoices = adw_combo_row_new();
    gtk_widget_set_size_request(m_rowChoices, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowChoices), rowTitle.c_str());
    GtkStringList* lstChoices = gtk_string_list_new(nullptr);
    for(const std::string& s : m_choices)
    {
        gtk_string_list_append(lstChoices, s.c_str());
    }
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowChoices), G_LIST_MODEL(lstChoices));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowChoices);
    g_object_unref(lstChoices);
    //Layout
    adw_message_dialog_set_extra_child(ADW_MESSAGE_DIALOG(m_gobj), m_preferencesGroup);
}

ComboBoxDialog::~ComboBoxDialog()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

std::string ComboBoxDialog::getSelectedChoice() const
{
    unsigned int selectedRow = adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowChoices));
    return m_choices[selectedRow];
}

GtkWidget* ComboBoxDialog::gobj()
{
    return m_gobj;
}

void ComboBoxDialog::show()
{
    gtk_widget_show(m_gobj);
}
