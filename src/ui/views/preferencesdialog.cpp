#include "preferencesdialog.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::UI::Views;

PreferencesDialog::PreferencesDialog(GtkWindow* parent, const PreferencesDialogController& controller) : m_controller{ controller }, m_gobj{ adw_window_new() }
{
    //Window Settings
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), parent);
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 800, 600);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    gtk_window_set_deletable(GTK_WINDOW(m_gobj), false);
    gtk_window_set_destroy_with_parent(GTK_WINDOW(m_gobj), false);
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    //Header Bar
    m_headerBar = adw_header_bar_new();
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), adw_window_title_new("Preferences", nullptr));
    //Cancel Button
    m_btnCancel = gtk_button_new();
    gtk_button_set_label(GTK_BUTTON(m_btnCancel), "Cancel");
    g_signal_connect(m_btnCancel, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton*, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->onCancel(); }), this);
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnCancel);
    //Save Button
    m_btnSave = gtk_button_new();
    gtk_button_set_label(GTK_BUTTON(m_btnSave), "Save");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnSave), "suggested-action");
    g_signal_connect(m_btnSave, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton*, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->onSave(); }), this);
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnSave);
    //User Interface Group
    m_grpUserInterface = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpUserInterface), "User Interface");
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpUserInterface), "Customize the application's user interface.");
    //Theme Row
    m_rowTheme = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTheme), "Theme");
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowTheme), G_LIST_MODEL(gtk_string_list_new(new const char*[4]{ "System", "Light", "Dark", nullptr })));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpUserInterface), m_rowTheme);
    //Application Group
    m_grpApplication = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpApplication), "Application");
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpApplication), "Customize application settings.");
    //Is First Time Open Row
    m_rowIsFirstTimeOpen = adw_action_row_new();
    m_switchIsFirstTimeOpen = gtk_switch_new();
    gtk_widget_set_valign(m_switchIsFirstTimeOpen, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowIsFirstTimeOpen), "Is First Time Open");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowIsFirstTimeOpen), m_switchIsFirstTimeOpen);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowIsFirstTimeOpen), m_switchIsFirstTimeOpen);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpApplication), m_rowIsFirstTimeOpen);
    //Tagger Group
    m_grpTagger = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpTagger), "Tagger");
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpTagger), "Customize tagger settings.");
    //Include Subfolders Row
    m_rowIncludeSubfolders = adw_action_row_new();
    m_switchIncludeSubfolders = gtk_switch_new();
    gtk_widget_set_valign(m_switchIncludeSubfolders, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowIncludeSubfolders), "Include Subfolders");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowIncludeSubfolders), m_switchIncludeSubfolders);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowIncludeSubfolders), m_switchIncludeSubfolders);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpTagger), m_rowIncludeSubfolders);
    //Remember Last Opened Folder Row
    m_rowRememberLastOpenedFolder = adw_action_row_new();
    m_switchRememberLastOpenedFolder = gtk_switch_new();
    gtk_widget_set_valign(m_switchRememberLastOpenedFolder, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowRememberLastOpenedFolder), "Remember Last Opened Folder");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowRememberLastOpenedFolder), m_switchRememberLastOpenedFolder);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowRememberLastOpenedFolder), m_switchRememberLastOpenedFolder);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpTagger), m_rowRememberLastOpenedFolder);
    //Page
    m_page = adw_preferences_page_new();
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpUserInterface));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpApplication));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpTagger));
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_page);
    adw_window_set_content(ADW_WINDOW(m_gobj), m_mainBox);
    //Load Configuration
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowTheme), m_controller.getThemeAsInt());
    gtk_switch_set_active(GTK_SWITCH(m_switchIsFirstTimeOpen), m_controller.getIsFirstTimeOpen());
    gtk_switch_set_active(GTK_SWITCH(m_switchIncludeSubfolders), m_controller.getIncludeSubfolders());
    gtk_switch_set_active(GTK_SWITCH(m_switchRememberLastOpenedFolder), m_controller.getRememberLastOpenedFolder());
}

PreferencesDialog::~PreferencesDialog()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

GtkWidget* PreferencesDialog::gobj()
{
    return m_gobj;
}

void PreferencesDialog::show()
{
    gtk_widget_show(m_gobj);
}

void PreferencesDialog::onCancel()
{
    gtk_widget_hide(m_gobj);
}

void PreferencesDialog::onSave()
{
    m_controller.setTheme(adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowTheme)));
    m_controller.setIsFirstTimeOpen(gtk_switch_get_active(GTK_SWITCH(m_switchIsFirstTimeOpen)));
    m_controller.setIncludeSubfolders(gtk_switch_get_active(GTK_SWITCH(m_switchIncludeSubfolders)));
    m_controller.setRememberLastOpenedFolder(gtk_switch_get_active(GTK_SWITCH(m_switchRememberLastOpenedFolder)));
    m_controller.saveConfiguration();
    if(m_controller.getThemeAsInt() == 0)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_PREFER_LIGHT);
    }
    else if(m_controller.getThemeAsInt() == 1)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
    }
    else if(m_controller.getThemeAsInt() == 2)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
    }
    gtk_widget_hide(m_gobj);
}
