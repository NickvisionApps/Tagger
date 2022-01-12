#include "settingsdialog.h"

namespace NickvisionTagger::Views
{
    SettingsDialog::SettingsDialog(Gtk::Window& parent) : Gtk::Dialog("Settings", parent, true, true)
    {
        //==Settings==//
        set_default_size(600, 500);
        set_resizable(false);
        set_hide_on_close(true);
        //==General Section==//
        m_lblGeneral.set_markup("<b>General</b>");
        m_lblGeneral.set_halign(Gtk::Align::START);
        m_lblGeneral.set_margin_start(20);
        m_lblGeneral.set_margin_top(20);
        m_listGeneral.set_selection_mode(Gtk::SelectionMode::NONE);
        m_listGeneral.set_margin_top(6);
        m_listGeneral.set_margin_start(20);
        m_listGeneral.set_margin_end(20);
        m_chkIncludeSubfolders.set_label("Include Subfolders");
        m_chkIncludeSubfolders.set_tooltip_text("If checked, Tagger will scan for files in the subfolders of the opened folder.");
        m_chkRememberLastOpenedFolder.set_label("Remember Last Opened Folder");
        m_chkRememberLastOpenedFolder.set_tooltip_text("If checked, Tagger will remember the last opened music folder and automatically open it again when the application starts again.");
        m_listGeneral.append(m_chkIncludeSubfolders);
        m_listGeneral.append(m_chkRememberLastOpenedFolder);
        //==Layout==//
        m_mainBox.set_orientation(Gtk::Orientation::VERTICAL);
        m_mainBox.append(m_lblGeneral);
        m_mainBox.append(m_listGeneral);
        m_scroll.set_child(m_mainBox);
        set_child(m_scroll);
        //==Load Configuration==//
        m_chkIncludeSubfolders.set_active(m_configuration.includeSubfolders());
        m_chkRememberLastOpenedFolder.set_active(m_configuration.rememberLastOpenedFolder());
    }

    SettingsDialog::~SettingsDialog()
    {
        m_configuration.setIncludeSubfolders(m_chkIncludeSubfolders.get_active());
        m_configuration.setRememberLastOpenedFolder(m_chkRememberLastOpenedFolder.get_active());
        m_configuration.save();
    }
}
