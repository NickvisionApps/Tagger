#include "preferencesdialog.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::UI::Views;

PreferencesDialog::PreferencesDialog(GtkWindow* parent, const PreferencesDialogController& controller) : m_controller{ controller }, m_gobj{ adw_window_new() }
{
    //Window Settings
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), parent);
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 800, 600);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    gtk_window_set_destroy_with_parent(GTK_WINDOW(m_gobj), false);
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    //Header Bar
    m_headerBar = adw_header_bar_new();
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), adw_window_title_new(_("Preferences"), nullptr));
    //User Interface Group
    m_grpUserInterface = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpUserInterface), _("User Interface"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpUserInterface), _("Customize the application's user interface."));
    //Theme Row
    m_rowTheme = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTheme), _("Theme"));
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowTheme), G_LIST_MODEL(gtk_string_list_new(new const char*[4]{ _("System"), _("Light"), _("Dark"), nullptr })));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpUserInterface), m_rowTheme);
    g_signal_connect(m_rowTheme, "notify::selected-item", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
    //Music Folder Group
    m_grpMusicFolder = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpMusicFolder), _("Music Folder"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpMusicFolder), _("Customize music folder settings."));
    //Include Subfolders Row
    m_rowIncludeSubfolders = adw_action_row_new();
    m_switchIncludeSubfolders = gtk_switch_new();
    gtk_widget_set_valign(m_switchIncludeSubfolders, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowIncludeSubfolders), _("Include Subfolders"));
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_rowIncludeSubfolders), _("If checked, subfolders will be included when scanning for music files in a music folder."));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowIncludeSubfolders), m_switchIncludeSubfolders);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowIncludeSubfolders), m_switchIncludeSubfolders);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpMusicFolder), m_rowIncludeSubfolders);
    //Remember Last Opened Folder Row
    m_rowRememberLastOpenedFolder = adw_action_row_new();
    m_switchRememberLastOpenedFolder = gtk_switch_new();
    gtk_widget_set_valign(m_switchRememberLastOpenedFolder, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowRememberLastOpenedFolder), _("Remember Last Opened Folder"));
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_rowRememberLastOpenedFolder), _("If checked, the last opened music folder will be remembered and opened again when Tagger starts."));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowRememberLastOpenedFolder), m_switchRememberLastOpenedFolder);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowRememberLastOpenedFolder), m_switchRememberLastOpenedFolder);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpMusicFolder), m_rowRememberLastOpenedFolder);
    //Music File Group
    m_grpMusicFile = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpMusicFile), _("Music File"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpMusicFile), _("Customize music file settings."));
    //Preserve Modification Time Stamp Row
    m_rowPreserveModificationTimeStamp = adw_action_row_new();
    m_switchPreserveModificationTimeStamp = gtk_switch_new();
    gtk_widget_set_valign(m_switchPreserveModificationTimeStamp, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowPreserveModificationTimeStamp), _("Preserve Modification Time Stamp"));
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_rowPreserveModificationTimeStamp), _("If checked, a music file's modification time stamp will not be updated when the tag is edited."));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowPreserveModificationTimeStamp), m_switchPreserveModificationTimeStamp);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowPreserveModificationTimeStamp), m_switchPreserveModificationTimeStamp);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpMusicFile), m_rowPreserveModificationTimeStamp);
    //Overwrite Tag With MusicBrainz
    m_rowOverwriteTagWithMusicBrainz = adw_action_row_new();
    m_switchOverwriteTagWithMusicBrainz = gtk_switch_new();
    gtk_widget_set_valign(m_switchOverwriteTagWithMusicBrainz, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowOverwriteTagWithMusicBrainz), _("Overwrite Tag With MusicBrainz"));
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_rowOverwriteTagWithMusicBrainz), _("If checked, a tag's properties will be overwritten with the metadata downloaded from MusicBrainz. Else, Tagger will fill-in only empty tag properties with MusicBrainz metadata."));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowOverwriteTagWithMusicBrainz), m_switchOverwriteTagWithMusicBrainz);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowOverwriteTagWithMusicBrainz), m_switchOverwriteTagWithMusicBrainz);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpMusicFile), m_rowOverwriteTagWithMusicBrainz);
    //Fingerprinting Group
    m_grpFingerprinting = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpFingerprinting), _("Fingerprinting"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpFingerprinting), _("Customize fingerprinting settings."));
    //AcoustId User API Key
    m_btnGetAcoustIdUserAPIKey = gtk_button_new();
    gtk_widget_set_valign(m_btnGetAcoustIdUserAPIKey, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnGetAcoustIdUserAPIKey, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnGetAcoustIdUserAPIKey), "window-new-symbolic");
    gtk_widget_set_tooltip_text(m_btnGetAcoustIdUserAPIKey, _("Get New API Key"));
    g_signal_connect(m_btnGetAcoustIdUserAPIKey, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<PreferencesDialog*>(data)->onGetAcoustIdUserAPIKeyClicked(); }), this);
    m_rowAcoustIdUserAPIKey = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAcoustIdUserAPIKey), _("AcoustId User API Key"));
    adw_entry_row_add_suffix(ADW_ENTRY_ROW(m_rowAcoustIdUserAPIKey), m_btnGetAcoustIdUserAPIKey);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpFingerprinting), m_rowAcoustIdUserAPIKey);
    //Page
    m_page = adw_preferences_page_new();
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpUserInterface));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpMusicFolder));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpMusicFile));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpFingerprinting));
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_page);
    adw_window_set_content(ADW_WINDOW(m_gobj), m_mainBox);
    //Load Configuration
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowTheme), m_controller.getThemeAsInt());
    gtk_switch_set_active(GTK_SWITCH(m_switchIncludeSubfolders), m_controller.getIncludeSubfolders());
    gtk_switch_set_active(GTK_SWITCH(m_switchRememberLastOpenedFolder), m_controller.getRememberLastOpenedFolder());
    gtk_switch_set_active(GTK_SWITCH(m_switchPreserveModificationTimeStamp), m_controller.getPreserveModificationTimeStamp());
    gtk_switch_set_active(GTK_SWITCH(m_switchOverwriteTagWithMusicBrainz), m_controller.getOverwriteTagWithMusicBrainz());
    gtk_editable_set_text(GTK_EDITABLE(m_rowAcoustIdUserAPIKey), m_controller.getAcoustIdUserAPIKey().c_str());
}

GtkWidget* PreferencesDialog::gobj()
{
    return m_gobj;
}

void PreferencesDialog::run()
{
    gtk_widget_show(m_gobj);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    m_controller.setIncludeSubfolders(gtk_switch_get_active(GTK_SWITCH(m_switchIncludeSubfolders)));
    m_controller.setRememberLastOpenedFolder(gtk_switch_get_active(GTK_SWITCH(m_switchRememberLastOpenedFolder)));
    m_controller.setPreserveModificationTimeStamp(gtk_switch_get_active(GTK_SWITCH(m_switchPreserveModificationTimeStamp)));
    m_controller.setOverwriteTagWithMusicBrainz(gtk_switch_get_active(GTK_SWITCH(m_switchOverwriteTagWithMusicBrainz)));
    m_controller.setAcoustIdUserAPIKey(gtk_editable_get_text(GTK_EDITABLE(m_rowAcoustIdUserAPIKey)));
    m_controller.saveConfiguration();
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

void PreferencesDialog::onThemeChanged()
{
    m_controller.setTheme(adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowTheme)));
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
}

void PreferencesDialog::onGetAcoustIdUserAPIKeyClicked()
{
    g_app_info_launch_default_for_uri(m_controller.getAcoudtIdUserAPIKeyLink().c_str(), nullptr, nullptr);
}
