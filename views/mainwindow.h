#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <memory>
#include <gtkmm.h>
#include "../models/musicfile.h"
#include "../models/musicfolder.h"
#include "../models/datamusicfilescolumns.h"
#include "../controls/headerbar.h"
#include "../controls/infobar.h"

namespace NickvisionTagger::Views
{
    class MainWindow : public Gtk::ApplicationWindow
    {
    public:
        MainWindow();
        ~MainWindow();

    private:
        NickvisionTagger::Models::MusicFolder m_musicFolder;
        std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
        //==UI==//
        NickvisionTagger::Controls::HeaderBar m_headerBar;
        Gtk::Box m_mainBox;
        NickvisionTagger::Controls::InfoBar m_infoBar;
        Gtk::Box m_boxWindow;
        Gtk::ScrolledWindow m_scrollMusicProperties;
        Gtk::Box m_boxMusicProperties;
        Gtk::Label m_lblFilename;
        Gtk::Entry m_txtFilename;
        Gtk::Label m_lblTitle;
        Gtk::Entry m_txtTitle;
        Gtk::Label m_lblArtist;
        Gtk::Entry m_txtArtist;
        Gtk::Label m_lblAlbum;
        Gtk::Entry m_txtAlbum;
        Gtk::Label m_lblYear;
        Gtk::Entry m_txtYear;
        Gtk::Label m_lblTrack;
        Gtk::Entry m_txtTrack;
        Gtk::Label m_lblGenre;
        Gtk::Entry m_txtGenre;
        Gtk::Label m_lblComment;
        Gtk::Entry m_txtComment;
        Gtk::Label m_lblDuration;
        Gtk::Entry m_txtDuration;
        Gtk::Label m_lblFileSize;
        Gtk::Entry m_txtFileSize;
        Gtk::ScrolledWindow m_scrollDataMusicFiles;
        Gtk::TreeView m_dataMusicFiles;
        NickvisionTagger::Models::DataMusicFilesColumns m_dataMusicFilesColumns;
        std::shared_ptr<Gtk::ListStore> m_dataMusicFilesModel;
        //==Slots==//
        void openMusicFolder(const Glib::VariantBase& args);
        void reloadMusicFolder(const Glib::VariantBase& args);
        void closeMusicFolder(const Glib::VariantBase& args);
        void saveTags();
        void removeTags();
        void filenameToTag();
        void tagToFilename();
        void settings();
        void checkForUpdates(const Glib::VariantBase& args);
        void gitHubRepo(const Glib::VariantBase& args);
        void reportABug(const Glib::VariantBase& args);
        void changelog(const Glib::VariantBase& args);
        void about(const Glib::VariantBase& args);
        void dataMusicFilesSelectionChanged();
    };
}

#endif // MAINWINDOW_H
