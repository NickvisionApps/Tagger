#include "preferencesdialog.h"

using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;
using namespace NickvisionTagger::UI::Views;

PreferencesDialog::PreferencesDialog(GtkWidget* parent, Configuration& configuration) : Widget{"/org/nickvision/tagger/ui/views/preferencesdialog.xml", "adw_preferencesDialog"}, m_configuration{configuration}
{
    //==Dialog==//
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), GTK_WINDOW(parent));
    //==Signals==//
    g_signal_connect(gtk_builder_get_object(m_builder, "gtk_btnCancel"), "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton* button, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->cancel(); }), this);
    g_signal_connect(gtk_builder_get_object(m_builder, "gtk_btnSave"), "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton* button, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->save(); }), this);
    g_signal_connect(gtk_builder_get_object(m_builder, "adw_rowIncludeSubfolders"), "activated", G_CALLBACK((void (*)(AdwActionRow*, gpointer*))[](AdwActionRow* row, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->onRowIncludeSubfoldersActivate(); }), this);
    g_signal_connect(gtk_builder_get_object(m_builder, "adw_rowRememberLastOpenedFolder"), "activated", G_CALLBACK((void (*)(AdwActionRow*, gpointer*))[](AdwActionRow* row, gpointer* data) { reinterpret_cast<PreferencesDialog*>(data)->onRowRememberLastOpenedFolderActivate(); }), this);
    //==Load Config==//
    if(m_configuration.getTheme() == Theme::System)
    {
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "adw_rowTheme")), 0);
    }
    else if(m_configuration.getTheme() == Theme::Light)
    {
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "adw_rowTheme")), 1);
    }
    else if(m_configuration.getTheme() == Theme::Dark)
    {
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "adw_rowTheme")), 2);
    }
    gtk_switch_set_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchIncludeSubfolders")), m_configuration.getIncludeSubfolders());
    gtk_switch_set_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchRememberLastOpenedFolder")), m_configuration.getRememberLastOpenedFolder());
}

PreferencesDialog::~PreferencesDialog()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

void PreferencesDialog::cancel()
{
    gtk_widget_hide(m_gobj);
}

void PreferencesDialog::save()
{
    m_configuration.setTheme(static_cast<Theme>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "adw_rowTheme")))));
    m_configuration.setIncludeSubfolders(gtk_switch_get_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchIncludeSubfolders"))));
    m_configuration.setRememberLastOpenedFolder(gtk_switch_get_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchRememberLastOpenedFolder"))));
    m_configuration.save();
    gtk_widget_hide(m_gobj);
}

void PreferencesDialog::onRowIncludeSubfoldersActivate()
{
    gtk_switch_set_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchIncludeSubfolders")), !gtk_switch_get_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchIncludeSubfolders"))));
}
void PreferencesDialog::onRowRememberLastOpenedFolderActivate()
{
    gtk_switch_set_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchRememberLastOpenedFolder")), !gtk_switch_get_active(GTK_SWITCH(gtk_builder_get_object(m_builder, "gtk_switchRememberLastOpenedFolder"))));
}
