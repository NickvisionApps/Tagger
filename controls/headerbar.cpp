#include "headerbar.h"

namespace NickvisionTagger::Controls
{
    HeaderBar::HeaderBar()
    {
        //==Folder==//
        m_actionFolder = Gio::SimpleActionGroup::create();
        m_actionOpenMusicFolder = m_actionFolder->add_action("openMusicFolder");
        m_actionReloadMusicfolder = m_actionFolder->add_action("reloadMusicFolder");
        m_actionCloseMusicFolder = m_actionFolder->add_action("closeMusicFolder");
        insert_action_group("folder", m_actionFolder);
        m_menuFolder = Gio::Menu::create();
        m_menuFolder->append("Open Music Folder", "folder.openMusicFolder");
        m_menuFolder->append("Reload Music Folder", "folder.reloadMusicFolder");
        m_menuFolder->append("Close Music Folder", "folder.closeMusicFolder");
        m_btnFolder.set_icon_name("folder");
        m_btnFolder.set_menu_model(m_menuFolder);
        m_btnFolder.set_tooltip_text("Folder Actions");
        //==Save Tags==//
        m_btnSaveTags.set_icon_name("document-save");
        m_btnSaveTags.set_tooltip_text("Save Tags");
        //==Remove Tags==//
        m_boxRemoveTags.set_orientation(Gtk::Orientation::VERTICAL);
        m_lblRemoveTags.set_label("Are you sure you want to remove tags from the selected files?");
        m_lblRemoveTags.set_margin(4);
        m_btnRTRemove.set_label("Remove");
        m_btnRTCancel.set_label("Cancel");
        m_btnRTCancel.signal_clicked().connect(sigc::mem_fun(m_popRemoveTags, &Gtk::Popover::popdown));
        m_boxRTBtns.set_orientation(Gtk::Orientation::HORIZONTAL);
        m_boxRTBtns.set_homogeneous(true);
        m_boxRTBtns.set_spacing(6);
        m_boxRTBtns.append(m_btnRTRemove);
        m_boxRTBtns.append(m_btnRTCancel);
        m_boxRemoveTags.append(m_lblRemoveTags);
        m_boxRemoveTags.append(m_boxRTBtns);
        m_popRemoveTags.set_child(m_boxRemoveTags);
        m_btnRemoveTags.set_icon_name("edit-delete");
        m_btnRemoveTags.set_popover(m_popRemoveTags);
        m_btnRemoveTags.set_tooltip_text("Remove Tags");
        //==Filename To Tag==//
        m_boxFilenameToTag.set_orientation(Gtk::Orientation::VERTICAL);
        m_lblFTTFormatString.set_label("Please choose a format string.");
        m_lblFTTFormatString.set_margin(4);
        m_cmbFTTFormatString.append("%artist%- %title%");
        m_cmbFTTFormatString.append("%title%- %artist%");
        m_cmbFTTFormatString.append("%title%");
        m_cmbFTTFormatString.set_active_text("%artist%- %title%");
        m_btnFTTConvert.set_label("Convert");
        m_btnFTTCancel.set_label("Cancel");
        m_btnFTTCancel.signal_clicked().connect(sigc::mem_fun(m_popFilenameToTag, &Gtk::Popover::popdown));
        m_boxFTTBtns.set_orientation(Gtk::Orientation::HORIZONTAL);
        m_boxFTTBtns.set_homogeneous(true);
        m_boxFTTBtns.set_spacing(6);
        m_boxFTTBtns.append(m_btnFTTConvert);
        m_boxFTTBtns.append(m_btnFTTCancel);
        m_boxFilenameToTag.append(m_lblFTTFormatString);
        m_boxFilenameToTag.append(m_cmbFTTFormatString);
        m_boxFilenameToTag.append(m_boxFTTBtns);
        m_popFilenameToTag.set_child(m_boxFilenameToTag);
        m_btnFilenameToTag.set_icon_name("edit-find-replace");
        m_btnFilenameToTag.set_popover(m_popFilenameToTag);
        m_btnFilenameToTag.set_tooltip_text("Filename to Tag");
        //==Tag To Filename==//
        m_boxTagToFilename.set_orientation(Gtk::Orientation::VERTICAL);
        m_lblTTFFormatString.set_label("Please choose a format string.");
        m_lblTTFFormatString.set_margin(4);
        m_cmbTTFFormatString.append("%artist%- %title%");
        m_cmbTTFFormatString.append("%title%- %artist%");
        m_cmbTTFFormatString.append("%title%");
        m_cmbTTFFormatString.set_active_text("%artist%- %title%");
        m_btnTTFConvert.set_label("Convert");
        m_btnTTFCancel.set_label("Cancel");
        m_btnTTFCancel.signal_clicked().connect(sigc::mem_fun(m_popTagToFilename, &Gtk::Popover::popdown));
        m_boxTTFBtns.set_orientation(Gtk::Orientation::HORIZONTAL);
        m_boxTTFBtns.set_homogeneous(true);
        m_boxTTFBtns.set_spacing(6);
        m_boxTTFBtns.append(m_btnTTFConvert);
        m_boxTTFBtns.append(m_btnTTFCancel);
        m_boxTagToFilename.append(m_lblTTFFormatString);
        m_boxTagToFilename.append(m_cmbTTFFormatString);
        m_boxTagToFilename.append(m_boxTTFBtns);
        m_popTagToFilename.set_child(m_boxTagToFilename);
        m_btnTagToFilename.set_icon_name("edit");
        m_btnTagToFilename.set_popover(m_popTagToFilename);
        m_btnTagToFilename.set_tooltip_text("Tag to Filename");
        //==Settings==//
        m_btnSettings.set_icon_name("settings");
        m_btnSettings.set_tooltip_text("Settings");
        //==Help==//
        m_actionHelp = Gio::SimpleActionGroup::create();
        m_actionCheckForUpdates = m_actionHelp->add_action("checkForUpdates");
        m_actionGitHubRepo = m_actionHelp->add_action("gitHubRepo");
        m_actionReportABug = m_actionHelp->add_action("reportABug");
        m_actionChangelog = m_actionHelp->add_action("changelog");
        m_actionAbout = m_actionHelp->add_action("about");
        insert_action_group("help", m_actionHelp);
        m_menuHelp = Gio::Menu::create();
        m_menuHelpUpdate = Gio::Menu::create();
        m_menuHelpUpdate->append("Check for Updates", "help.checkForUpdates");
        m_menuHelpLinks = Gio::Menu::create();
        m_menuHelpLinks->append("GitHub Repo", "help.gitHubRepo");
        m_menuHelpLinks->append("Report a Bug", "help.reportABug");
        m_menuHelpActions = Gio::Menu::create();
        m_menuHelpActions->append("Changelog", "help.changelog");
        m_menuHelpActions->append("About", "help.about");
        m_menuHelp->append_section(m_menuHelpUpdate);
        m_menuHelp->append_section(m_menuHelpLinks);
        m_menuHelp->append_section(m_menuHelpActions);
        m_btnHelp.set_direction(Gtk::ArrowType::NONE);
        m_btnHelp.set_menu_model(m_menuHelp);
        m_btnHelp.set_tooltip_text("Help");
        //==Layout==//
        pack_start(m_btnFolder);
        pack_start(m_sep1);
        pack_start(m_btnSaveTags);
        pack_start(m_btnRemoveTags);
        pack_start(m_sep2);
        pack_start(m_btnFilenameToTag);
        pack_start(m_btnTagToFilename);
        pack_end(m_btnHelp);
        pack_end(m_btnSettings);
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionOpenMusicFolder() const
    {
        return m_actionOpenMusicFolder;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionReloadMusicFolder() const
    {
        return m_actionReloadMusicfolder;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionCloseMusicFolder() const
    {
        return m_actionCloseMusicFolder;
    }

    Gtk::Button& HeaderBar::getBtnSaveTags()
    {
        return m_btnSaveTags;
    }

    Gtk::Popover& HeaderBar::getPopRemoveTags()
    {
        return m_popRemoveTags;
    }

    Gtk::Button& HeaderBar::getBtnRTRemove()
    {
        return m_btnRTRemove;
    }

    Gtk::MenuButton& HeaderBar::getBtnRemoveTags()
    {
        return m_btnRemoveTags;
    }

    Gtk::Popover& HeaderBar::getPopFilenameToTag()
    {
        return m_popFilenameToTag;
    }

    Gtk::ComboBoxText& HeaderBar::getCmbFTTFormatString()
    {
        return m_cmbFTTFormatString;
    }

    Gtk::Button& HeaderBar::getBtnFTTConvert()
    {
        return m_btnFTTConvert;
    }

    Gtk::MenuButton& HeaderBar::getBtnFilenameToTag()
    {
        return m_btnFilenameToTag;
    }

    Gtk::Popover& HeaderBar::getPopTagToFilename()
    {
        return m_popTagToFilename;
    }

    Gtk::ComboBoxText& HeaderBar::getCmbTTFFormatString()
    {
        return m_cmbTTFFormatString;
    }

    Gtk::Button& HeaderBar::getBtnTTFConvert()
    {
        return m_btnTTFConvert;
    }

    Gtk::MenuButton& HeaderBar::getBtnTagToFilename()
    {
        return m_btnTagToFilename;
    }

    Gtk::Button& HeaderBar::getBtnSettings()
    {
        return m_btnSettings;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionCheckForUpdates() const
    {
        return m_actionCheckForUpdates;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionGitHubRepo() const
    {
        return m_actionGitHubRepo;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionReportABug() const
    {
        return m_actionReportABug;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionChangelog() const
    {
        return m_actionChangelog;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionAbout() const
    {
        return m_actionAbout;
    }
}
