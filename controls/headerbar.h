#ifndef HEADERBAR_H
#define HEADERBAR_H

#include <memory>
#include <gtkmm.h>

namespace NickvisionTagger::Controls
{
    class HeaderBar : public Gtk::HeaderBar
    {
    public:
        HeaderBar();
        void setTitle(const std::string& title);
        void setSubtitle(const std::string& subtitle);
        const std::shared_ptr<Gio::SimpleAction>& getActionOpenMusicFolder() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionReloadMusicFolder() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionCloseMusicFolder() const;
        Gtk::Button& getBtnSaveTags();
        Gtk::Popover& getPopRemoveTags();
        Gtk::Button& getBtnRTRemove();
        Gtk::MenuButton& getBtnRemoveTags();
        Gtk::Popover& getPopFilenameToTag();
        Gtk::ComboBoxText& getCmbFTTFormatString();
        Gtk::Button& getBtnFTTConvert();
        Gtk::MenuButton& getBtnFilenameToTag();
        Gtk::Popover& getPopTagToFilename();
        Gtk::ComboBoxText& getCmbTTFFormatString();
        Gtk::Button& getBtnTTFConvert();
        Gtk::MenuButton& getBtnTagToFilename();
        const std::shared_ptr<Gio::SimpleAction>& getActionCheckForUpdates() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionGitHubRepo() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionReportABug() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionSettings() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionChangelog() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionAbout() const;

    private:
        //==Title Widget==//
        Gtk::Box m_boxTitle;
        Gtk::Label m_lblTitle;
        Gtk::Label m_lblSubtitle;
        //==Folder Actions==//
        std::shared_ptr<Gio::SimpleActionGroup> m_actionFolder;
        std::shared_ptr<Gio::SimpleAction> m_actionOpenMusicFolder;
        std::shared_ptr<Gio::SimpleAction> m_actionReloadMusicfolder;
        std::shared_ptr<Gio::SimpleAction> m_actionCloseMusicFolder;
        std::shared_ptr<Gio::Menu> m_menuFolder;
        Gtk::MenuButton m_btnFolder;
        //==Save Tags==//
        Gtk::Button m_btnSaveTags;
        //==Remove Tags==//
        Gtk::Popover m_popRemoveTags;
        Gtk::Box m_boxRemoveTags;
        Gtk::Label m_lblRemoveTags;
        Gtk::Button m_btnRTRemove;
        Gtk::Button m_btnRTCancel;
        Gtk::Box m_boxRTBtns;
        Gtk::MenuButton m_btnRemoveTags;
        //==Filename To Tag==//
        Gtk::Popover m_popFilenameToTag;
        Gtk::Box m_boxFilenameToTag;
        Gtk::Label m_lblFTTFormatString;
        Gtk::ComboBoxText m_cmbFTTFormatString;
        Gtk::Button m_btnFTTConvert;
        Gtk::Button m_btnFTTCancel;
        Gtk::Box m_boxFTTBtns;
        Gtk::MenuButton m_btnFilenameToTag;
        //==Tag To Filename==//
        Gtk::Popover m_popTagToFilename;
        Gtk::Box m_boxTagToFilename;
        Gtk::Label m_lblTTFFormatString;
        Gtk::ComboBoxText m_cmbTTFFormatString;
        Gtk::Button m_btnTTFConvert;
        Gtk::Button m_btnTTFCancel;
        Gtk::Box m_boxTTFBtns;
        Gtk::MenuButton m_btnTagToFilename;
        //==Help==//
        std::shared_ptr<Gio::SimpleActionGroup> m_actionHelp;
        std::shared_ptr<Gio::SimpleAction> m_actionCheckForUpdates;
        std::shared_ptr<Gio::SimpleAction> m_actionGitHubRepo;
        std::shared_ptr<Gio::SimpleAction> m_actionReportABug;
        std::shared_ptr<Gio::SimpleAction> m_actionChangelog;
        std::shared_ptr<Gio::SimpleAction> m_actionSettings;
        std::shared_ptr<Gio::SimpleAction> m_actionAbout;
        std::shared_ptr<Gio::Menu> m_menuHelp;
        std::shared_ptr<Gio::Menu> m_menuHelpUpdate;
        std::shared_ptr<Gio::Menu> m_menuHelpLinks;
        std::shared_ptr<Gio::Menu> m_menuHelpActions;
        Gtk::MenuButton m_btnHelp;
        //==Separators==//
        Gtk::Separator m_sep1;
        Gtk::Separator m_sep2;
    };
}

#endif // HEADERBAR_H
