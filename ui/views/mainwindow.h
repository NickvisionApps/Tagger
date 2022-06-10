#pragma once

#include <string>
#include <vector>
#include <adwaita.h>
#include "../widget.h"
#include "../../models/configuration.h"
#include "../../models/musicfolder.h"

namespace NickvisionTagger::UI::Views
{
    class MainWindow : public NickvisionTagger::UI::Widget
    {
    public:
        MainWindow(NickvisionTagger::Models::Configuration& configuration);
        ~MainWindow();
        void showMaximized();

    private:
        NickvisionTagger::Models::Configuration& m_configuration;
        bool m_opened;
        NickvisionTagger::Models::MusicFolder m_musicFolder;
        std::vector<GtkWidget*> m_listMusicFilesRows;
        std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
        //==Tagger Actions==//
        GSimpleAction* m_gio_actOpenMusicFolder;
        GSimpleAction* m_gio_actReloadMusicFolder;
        GSimpleAction* m_gio_actSaveTags;
        GSimpleAction* m_gio_actRemoveTags;
        GSimpleAction* m_gio_actFilenameToTag;
        GSimpleAction* m_gio_actTagToFilename;
        GSimpleAction* m_gio_actDownloadMetadata;
        GSimpleAction* m_gio_actInsertAlbumArt;
        //==Help Actions==//
        GSimpleAction* m_gio_actPreferences;
        GSimpleAction* m_gio_actKeyboardShortcuts;
        GSimpleAction* m_gio_actChangelog;
        GSimpleAction* m_gio_actAbout;
        //==Signals==//
        void onStartup();
        void openMusicFolder();
        void reloadMusicFolder();
        void saveTags();
        void removeTags();
        void filenameToTag();
        void tagToFilename();
        void downloadMetadata();
        void insertAlbumArt();
        void preferences();
        void keyboardShortcuts();
        void changelog();
        void about();
        void onListMusicFilesSelectionChanged();
        //==Other Functions==//
        void sendToast(const std::string& message);
    };
}
