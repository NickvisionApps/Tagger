#include "comboboxdialog.hpp"

using namespace NickvisionTagger::UI::Controls;

ComboBoxDialog::ComboBoxDialog(GtkWindow* parent, const std::string& title, const std::string& description, const std::string& rowTitle, const std::vector<std::string>& choices) : m_choices{ choices }, m_selectedChoice{ "" }, m_gobj{ adw_window_new() }
{
    //Window Settings
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), parent);
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 420, 210);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    gtk_window_set_resizable(GTK_WINDOW(m_gobj), false);
    gtk_window_set_deletable(GTK_WINDOW(m_gobj), false);
    gtk_window_set_destroy_with_parent(GTK_WINDOW(m_gobj), false);
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    //Header Bar
    m_headerBar = adw_header_bar_new();
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), adw_window_title_new(nullptr, nullptr));
    //Cancel Button
    m_btnCancel = gtk_button_new();
    gtk_button_set_label(GTK_BUTTON(m_btnCancel), "Cancel");
    g_signal_connect(m_btnCancel, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton*, gpointer* data) { reinterpret_cast<ComboBoxDialog*>(data)->onCancel(); }), this);
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnCancel);
    //OK Button
    m_btnOK = gtk_button_new();
    gtk_button_set_label(GTK_BUTTON(m_btnOK), "OK");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnOK), "suggested-action");
    g_signal_connect(m_btnOK, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton*, gpointer* data) { reinterpret_cast<ComboBoxDialog*>(data)->onOK(); }), this);
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnOK);
    //Choices
    m_adwPreferencesPage = adw_preferences_page_new();
    m_adwPreferencesGroup = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_adwPreferencesGroup), title.c_str());
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_adwPreferencesGroup), description.c_str());
    m_rowChoices = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowChoices), rowTitle.c_str());
    GtkStringList* lstChoices = gtk_string_list_new(nullptr);
    for(const std::string& s : m_choices)
    {
        gtk_string_list_append(lstChoices, s.c_str());
    }
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowChoices), G_LIST_MODEL(lstChoices));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwPreferencesGroup), m_rowChoices);
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_adwPreferencesPage), ADW_PREFERENCES_GROUP(m_adwPreferencesGroup));
    g_object_unref(lstChoices);
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_adwPreferencesPage);
    adw_window_set_content(ADW_WINDOW(m_gobj), m_mainBox);
}

ComboBoxDialog::~ComboBoxDialog()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

const std::string& ComboBoxDialog::getSelectedChoice() const
{
    return m_selectedChoice;
}

GtkWidget* ComboBoxDialog::gobj()
{
    return m_gobj;
}

void ComboBoxDialog::show()
{
    gtk_widget_show(m_gobj);
}

void ComboBoxDialog::onCancel()
{
    gtk_widget_hide(m_gobj);
}

void ComboBoxDialog::onOK()
{
    unsigned int selectedRow = adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowChoices));
    m_selectedChoice = m_choices[selectedRow];
    gtk_widget_hide(m_gobj);
}
