#include "mainwindow.h"
#include <filesystem>
#include <regex>
#include "../../helpers/gtkhelpers.h"
#include "../../helpers/mediahelpers.h"
#include "../controls/progressdialog.h"
#include "../controls/comboboxdialog.h"
#include "preferencesdialog.h"
#include "shortcutsdialog.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;
using namespace NickvisionTagger::UI::Controls;
using namespace NickvisionTagger::UI::Views;

MainWindow::MainWindow(Configuration& configuration) : Widget{"/org/nickvision/tagger/ui/views/mainwindow.xml", "adw_winMain"}, m_configuration{configuration}, m_opened{false}
{
    //==Signals==//
    g_signal_connect(m_gobj, "show", G_CALLBACK((void (*)(GtkWidget*, gpointer*))[](GtkWidget* widget, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onStartup(); }), this);
    //==Tagger Actions==//
    //Open Music Folder
    m_gio_actOpenMusicFolder = g_simple_action_new("openMusicFolder", nullptr);
    g_signal_connect(m_gio_actOpenMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->openMusicFolder(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actOpenMusicFolder));
    //Reload Music Folder
    m_gio_actReloadMusicFolder = g_simple_action_new("reloadMusicFolder", nullptr);
    g_signal_connect(m_gio_actReloadMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->reloadMusicFolder(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actReloadMusicFolder));
    //Save Tags
    m_gio_actSaveTags = g_simple_action_new("saveTags", nullptr);
    g_signal_connect(m_gio_actSaveTags, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->saveTags(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actSaveTags));
    //Remove Tags
    m_gio_actRemoveTags = g_simple_action_new("removeTags", nullptr);
    g_signal_connect(m_gio_actRemoveTags, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->removeTags(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actRemoveTags));
    //Filename To Tag
    m_gio_actFilenameToTag = g_simple_action_new("filenameToTag", nullptr);
    g_signal_connect(m_gio_actFilenameToTag, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->filenameToTag(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actFilenameToTag));
    //Tag To Filename
    m_gio_actTagToFilename = g_simple_action_new("tagToFilename", nullptr);
    g_signal_connect(m_gio_actTagToFilename, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->tagToFilename(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actTagToFilename));
    //Download Metadata
    m_gio_actDownloadMetadata = g_simple_action_new("downloadMetadata", nullptr);
    g_signal_connect(m_gio_actDownloadMetadata, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->downloadMetadata(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actDownloadMetadata));
    //Insert Album Art
    m_gio_actInsertAlbumArt = g_simple_action_new("insertAlbumArt", nullptr);
    g_signal_connect(m_gio_actInsertAlbumArt, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->insertAlbumArt(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actInsertAlbumArt));
    //==Help Actions==//
    //Preferences
    m_gio_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_gio_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->preferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actPreferences));
    //Keyboard Shortcuts
    m_gio_actKeyboardShortcuts = g_simple_action_new("keyboardShortcuts", nullptr);
    g_signal_connect(m_gio_actKeyboardShortcuts, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->keyboardShortcuts(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actKeyboardShortcuts));
    //Changelog
    m_gio_actChangelog = g_simple_action_new("changelog", nullptr);
    g_signal_connect(m_gio_actChangelog, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->changelog(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actChangelog));
    //About
    m_gio_actAbout = g_simple_action_new("about", nullptr);
    g_signal_connect(m_gio_actAbout, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->about(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actAbout));
    //==Tag Actions Menu Button==//
    GtkBuilder* builderMenuTagActions{gtk_builder_new_from_resource("/org/nickvision/tagger/ui/views/menutagactions.xml")};
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(gtk_builder_get_object(m_builder, "gtk_btnMenuTagActions")), G_MENU_MODEL(gtk_builder_get_object(builderMenuTagActions, "gio_menuTagActions")));
    g_object_unref(builderMenuTagActions);
    //==Help Menu Button==//
    GtkBuilder* builderMenuHelp{gtk_builder_new_from_resource("/org/nickvision/tagger/ui/views/menuhelp.xml")};
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(gtk_builder_get_object(m_builder, "gtk_btnMenuHelp")), G_MENU_MODEL(gtk_builder_get_object(builderMenuHelp, "gio_menuHelp")));
    g_object_unref(builderMenuHelp);
    //==List Music Files==//
    g_signal_connect(gtk_builder_get_object(m_builder, "gtk_listMusicFiles"), "selected-rows-changed", G_CALLBACK((void (*)(GtkListBox*, gpointer*))[](GtkListBox* listBox, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onListMusicFilesSelectionChanged(); }), this);
}

MainWindow::~MainWindow()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

void MainWindow::showMaximized()
{
    gtk_widget_show(m_gobj);
    gtk_window_maximize(GTK_WINDOW(m_gobj));
}

void MainWindow::onStartup()
{
    if(!m_opened)
    {
        //==Set Action Shortcuts==//
        //Open Music Folder
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.openMusicFolder", new const char*[2]{ "<Ctrl>o", nullptr });
        //Reload Music Folder
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.reloadMusicFolder", new const char*[2]{ "F5", nullptr });
        //Save Tags
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.saveTags", new const char*[2]{ "<Ctrl>s", nullptr });
        //Remove Tags
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.removeTags", new const char*[2]{ "Delete", nullptr });
        //Filename To Tag
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.filenameToTag", new const char*[2]{ "<Ctrl>f", nullptr });
        //Tag To Filename
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.tagToFilename", new const char*[2]{ "<Ctrl>t", nullptr });
        //Download Metadata
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.downloadMetadata", new const char*[2]{ "<Ctrl>m", nullptr });
        //Insert Album Art
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.insertAlbumArt", new const char*[2]{ "<Ctrl><Shift>o", nullptr });
        //About
        gtk_application_set_accels_for_action(gtk_window_get_application(GTK_WINDOW(m_gobj)), "win.about", new const char*[2]{ "F1", nullptr });
        //==Load Configuration==//
        m_musicFolder.setIncludeSubfolders(m_configuration.getIncludeSubfolders());
        if(m_configuration.getRememberLastOpenedFolder() && std::filesystem::exists(m_configuration.getLastOpenedFolder()))
        {
            m_musicFolder.setPath(m_configuration.getLastOpenedFolder());
            adw_window_title_set_subtitle(ADW_WINDOW_TITLE(gtk_builder_get_object(GTK_BUILDER(m_builder), "adw_title")), m_musicFolder.getPath().c_str());
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnReloadMusicFolder")), true);
            reloadMusicFolder();
            sendToast("Loaded " + std::to_string(m_musicFolder.getFiles().size()) + " music files.");
        }
        m_opened = true;
    }
}

void MainWindow::openMusicFolder()
{
    GtkFileChooserNative* openFolderDialog{gtk_file_chooser_native_new("Open Music Folder", GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_SELECT_FOLDER, "_Open", "_Cancel")};
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openFolderDialog), true);
    g_signal_connect(openFolderDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer*))([](GtkNativeDialog* dialog, gint response_id, gpointer* data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
            GtkFileChooser* chooser{GTK_FILE_CHOOSER(dialog)};
            GFile* file{gtk_file_chooser_get_file(chooser)};
            std::string path{g_file_get_path(file)};
            g_object_unref(file);
            mainWindow->m_musicFolder.setPath(path);
            adw_window_title_set_subtitle(ADW_WINDOW_TITLE(gtk_builder_get_object(GTK_BUILDER(mainWindow->m_builder), "adw_title")), path.c_str());
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(mainWindow->m_builder, "gtk_btnReloadMusicFolder")), true);
            if(mainWindow->m_configuration.getRememberLastOpenedFolder())
            {
                mainWindow->m_configuration.setLastOpenedFolder(mainWindow->m_musicFolder.getPath());
                mainWindow->m_configuration.save();
            }
            mainWindow->reloadMusicFolder();
            mainWindow->sendToast("Loaded " + std::to_string(mainWindow->m_musicFolder.getFiles().size()) + " music files.");
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openFolderDialog));
}

void MainWindow::reloadMusicFolder()
{
    gtk_list_box_unselect_all(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "gtk_listMusicFiles")));
    for(GtkWidget* row : m_listMusicFilesRows)
    {
        gtk_list_box_remove(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "gtk_listMusicFiles")), row);
    }
    m_listMusicFilesRows.clear();
    ProgressDialog* progDialogReloading{new ProgressDialog(m_gobj, "Loading music files...", [&]() { m_musicFolder.reloadFiles(); }, [&]()
    {
        int id = 1;
        for(const std::shared_ptr<MusicFile>& musicFile : m_musicFolder.getFiles())
        {
            GtkWidget* row{adw_action_row_new()};
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), std::regex_replace(musicFile->getFilename(), std::regex("\\&"), "&amp;").c_str());
            adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::to_string(id).c_str());
            gtk_list_box_append(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "gtk_listMusicFiles")), row);
            m_listMusicFilesRows.push_back(row);
            g_main_context_iteration(g_main_context_default(), false);
            ++id;
        }
        if(m_musicFolder.getFiles().size() > 0)
        {
            adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "adw_viewStack")), "page_tagger");
        }
        else
        {
            adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "adw_viewStack")), "page_noFiles");
        }
    })};
    progDialogReloading->show();
}

void MainWindow::saveTags()
{
    ProgressDialog* progDialogSaving{new ProgressDialog(m_gobj, "Saving tags...", [&]() 
    { 
        for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
        {
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")))) != musicFile->getFilename() && std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")))) != "<keep>")
            {
                musicFile->setFilename(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTitle")))) != "<keep>")
            {
                musicFile->setTitle(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTitle"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtArtist")))) != "<keep>")
            {
                musicFile->setArtist(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtArtist"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbum")))) != "<keep>")
            {
                musicFile->setAlbum(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbum"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtYear")))) != "<keep>")
            {
                try
                {
                    musicFile->setYear(MediaHelpers::stoui(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtYear")))));
                }
                catch(...) { }
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTrack")))) != "<keep>")
            {
                try
                {
                    musicFile->setTrack(MediaHelpers::stoui(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTrack")))));
                }
                catch(...) { }
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbumArtist")))) != "<keep>")
            {
                musicFile->setAlbumArtist(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbumArtist"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtGenre")))) != "<keep>")
            {
                musicFile->setGenre(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtGenre"))));
            }
            if(std::string(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtComment")))) != "<keep>")
            {
                musicFile->setComment(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtComment"))));
            }
            musicFile->saveTag();
        }
    }, [&]() { reloadMusicFolder(); })};
    progDialogSaving->show();
}

void MainWindow::removeTags()
{
    GtkWidget* removeDialog{gtk_message_dialog_new(GTK_WINDOW(m_gobj), GtkDialogFlags(GTK_DIALOG_MODAL),
        GTK_MESSAGE_INFO, GTK_BUTTONS_YES_NO, "Remove Tags?")};
    gtk_message_dialog_format_secondary_text(GTK_MESSAGE_DIALOG(removeDialog), "Are you sure you want to remove the selected tags?\nThis action is irreversible.");
    g_signal_connect(removeDialog, "response", G_CALLBACK((void (*)(GtkDialog*, gint, gpointer*))([](GtkDialog* dialog, gint response_id, gpointer* data)
    {
        gtk_window_destroy(GTK_WINDOW(dialog));
        if(response_id == GTK_RESPONSE_YES)
        {
            MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
            ProgressDialog* progDialogRemoving{new ProgressDialog(mainWindow->m_gobj, "Removing tags...", [mainWindow]() 
            { 
                for(const std::shared_ptr<MusicFile>& musicFile : mainWindow->m_selectedMusicFiles)
                {
                    musicFile->removeTag();
                }
            }, [mainWindow]() { mainWindow->reloadMusicFolder(); })};
            progDialogRemoving->show();
        }
    })), this);
    gtk_widget_show(removeDialog);
}

void MainWindow::filenameToTag()
{
    ComboBoxDialog* formatStringDialog{new ComboBoxDialog(m_gobj, "Filename to Tag", "Please select a format string.", "Format String", { "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" })};
    std::pair<ComboBoxDialog*, MainWindow*>* pointers{new std::pair<ComboBoxDialog*, MainWindow*>(formatStringDialog, this)};
    g_signal_connect(formatStringDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget* widget, gpointer* data)
    {
        std::pair<ComboBoxDialog*, MainWindow*>* pointers{reinterpret_cast<std::pair<ComboBoxDialog*, MainWindow*>*>(data)};
        std::string formatString{pointers->first->getSelectedChoice()};
        delete pointers->first;
        if(!formatString.empty())
        {
            MainWindow* mainWindow{pointers->second};
            ProgressDialog* progDialogConverting{new ProgressDialog(pointers->second->m_gobj, "Converting filenames to tags...", [mainWindow, formatString]() 
            { 
                for(const std::shared_ptr<MusicFile>& musicFile : mainWindow->m_selectedMusicFiles)
                {
                    musicFile->filenameToTag(formatString);
                }
            }, [mainWindow]() { mainWindow->reloadMusicFolder(); })};
            progDialogConverting->show();
        }
        delete pointers;
    })), pointers);
    formatStringDialog->show();
}

void MainWindow::tagToFilename()
{
    ComboBoxDialog* formatStringDialog{new ComboBoxDialog(m_gobj, "Tag to Filename", "Please select a format string.", "Format String", { "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" })};
    std::pair<ComboBoxDialog*, MainWindow*>* pointers{new std::pair<ComboBoxDialog*, MainWindow*>(formatStringDialog, this)};
    g_signal_connect(formatStringDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget* widget, gpointer* data)
    {
        std::pair<ComboBoxDialog*, MainWindow*>* pointers{reinterpret_cast<std::pair<ComboBoxDialog*, MainWindow*>*>(data)};
        std::string formatString{pointers->first->getSelectedChoice()};
        delete pointers->first;
        if(!formatString.empty())
        {
            MainWindow* mainWindow{pointers->second};
            ProgressDialog* progDialogConverting{new ProgressDialog(pointers->second->m_gobj, "Converting tags to filenames...", [mainWindow, formatString]() 
            { 
                for(const std::shared_ptr<MusicFile>& musicFile : mainWindow->m_selectedMusicFiles)
                {
                    musicFile->tagToFilename(formatString);
                }
            }, [mainWindow]() { mainWindow->reloadMusicFolder(); })};
            progDialogConverting->show();
        }
        delete pointers;
    })), pointers);
    formatStringDialog->show();
}

void MainWindow::downloadMetadata()
{
    GtkWidget* downloadDialog{gtk_message_dialog_new(GTK_WINDOW(m_gobj), GtkDialogFlags(GTK_DIALOG_MODAL),
        GTK_MESSAGE_INFO, GTK_BUTTONS_YES_NO, "Required Information")};
    gtk_message_dialog_format_secondary_text(GTK_MESSAGE_DIALOG(downloadDialog), "Downloading tag metadata from the internet requires the music file to have its title and artist properties already set.\nAre you sure you want to continue?");
    g_signal_connect(downloadDialog, "response", G_CALLBACK((void (*)(GtkDialog*, gint, gpointer*))([](GtkDialog* dialog, gint response_id, gpointer* data)
    {
        gtk_window_destroy(GTK_WINDOW(dialog));
        if(response_id == GTK_RESPONSE_YES)
        {
            MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
            ProgressDialog* progDialogDownloading{new ProgressDialog(mainWindow->m_gobj, "Downloading metadata from internet...", [mainWindow]() 
            { 
                for(const std::shared_ptr<MusicFile>& musicFile : mainWindow->m_selectedMusicFiles)
                {
                    musicFile->downloadMusicBrainzMetadata();
                }
            }, [mainWindow]() { mainWindow->reloadMusicFolder(); })};
            progDialogDownloading->show();
        }
    })), this);
    gtk_widget_show(downloadDialog);
}

void MainWindow::insertAlbumArt()
{
    GtkFileChooserNative* openPictureDialog{gtk_file_chooser_native_new("Insert Album Art", GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_OPEN, "_Open", "_Cancel")};
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openPictureDialog), true);
    GtkFileFilter* imageFilter{gtk_file_filter_new()};
    gtk_file_filter_add_mime_type(imageFilter, "image/*");
    gtk_file_chooser_add_filter(GTK_FILE_CHOOSER(openPictureDialog), imageFilter);
    g_object_unref(imageFilter);
    g_signal_connect(openPictureDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer*))([](GtkNativeDialog* dialog, gint response_id, gpointer* data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
            GtkFileChooser* chooser{GTK_FILE_CHOOSER(dialog)};
            GFile* file{gtk_file_chooser_get_file(chooser)};
            std::string path{g_file_get_path(file)};
            g_object_unref(file);
            TagLib::ByteVector byteVector{MediaHelpers::byteVectorFromFile(path)};
            ProgressDialog* progDialogInserting{new ProgressDialog(mainWindow->m_gobj, "Inserting album art...", [mainWindow, byteVector]() 
            { 
                for(const std::shared_ptr<MusicFile>& musicFile : mainWindow->m_selectedMusicFiles)
                {
                    musicFile->setAlbumArt(byteVector);
                    musicFile->saveTag();
                }
            }, [mainWindow]() { mainWindow->reloadMusicFolder(); })};
            progDialogInserting->show();
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openPictureDialog));
}

void MainWindow::preferences()
{
    PreferencesDialog* preferencesDialog{new PreferencesDialog(m_gobj, m_configuration)};
    std::pair<PreferencesDialog*, MainWindow*>* pointers{new std::pair<PreferencesDialog*, MainWindow*>(preferencesDialog, this)};
    g_signal_connect(preferencesDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget* widget, gpointer* data)
    {
        std::pair<PreferencesDialog*, MainWindow*>* pointers{reinterpret_cast<std::pair<PreferencesDialog*, MainWindow*>*>(data)};
        delete pointers->first;
        if(pointers->second->m_configuration.getTheme() == Theme::System)
        {
           adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_PREFER_LIGHT);
        }
        else if(pointers->second->m_configuration.getTheme() == Theme::Light)
        {
           adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
        }
        else if(pointers->second->m_configuration.getTheme() == Theme::Dark)
        {
           adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
        }
        if(pointers->second->m_configuration.getIncludeSubfolders() != pointers->second->m_musicFolder.getIncludeSubfolders())
        {
            pointers->second->m_musicFolder.setIncludeSubfolders(pointers->second->m_configuration.getIncludeSubfolders());
            pointers->second->reloadMusicFolder();
        }
        delete pointers;
    })), pointers);
    preferencesDialog->show();
}

void MainWindow::keyboardShortcuts()
{
    ShortcutsDialog* shortcutsDialog{new ShortcutsDialog(m_gobj)};
    g_signal_connect(shortcutsDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget* widget, gpointer* data)
    {
        ShortcutsDialog* dialog{reinterpret_cast<ShortcutsDialog*>(data)};
        delete dialog;
    })), shortcutsDialog);
    shortcutsDialog->show();
}

void MainWindow::changelog()
{
    GtkWidget* changelogDialog{gtk_message_dialog_new(GTK_WINDOW(m_gobj), GtkDialogFlags(GTK_DIALOG_MODAL),
        GTK_MESSAGE_INFO, GTK_BUTTONS_OK, "What's New?")};
    gtk_message_dialog_format_secondary_text(GTK_MESSAGE_DIALOG(changelogDialog), "- Added support for OPUS file extension\n\n- Improved ListBox selection handling. (Double-click to select, Ctrl+Click to expand selection, etc...)\n\n- Fixed an issue where attempting to tag an ogg file would crash the program\n\n- Fixed an issue where Tag to Filename wasn't working correctly for \"track- title\"");
    g_signal_connect(changelogDialog, "response", G_CALLBACK(gtk_window_destroy), nullptr);
    gtk_widget_show(changelogDialog);
}

void MainWindow::about()
{
    gtk_show_about_dialog(GTK_WINDOW(m_gobj), "program-name", "Nickvision Tagger", "version", "2022.6.3", "comments", "An easy-to-use music tag (metadata) editor.",
                          "copyright", "(C) Nickvision 2021-2022", "license-type", GTK_LICENSE_GPL_3_0, "website", "https://github.com/nlogozzo/NickvisionTagger", "website-label", "GitHub",
                          "authors", new const char*[2]{ "Nicholas Logozzo", nullptr }, "artists", new const char*[4]{ "Nicholas Logozzo", "daudix-UFO (Icons)", "jannuary (Icons)", nullptr }, "logo-icon-name", "org.nickvision.tagger-devel", nullptr);
}

void MainWindow::sendToast(const std::string& message)
{
    AdwToast* toast{adw_toast_new(message.c_str())};
    adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(gtk_builder_get_object(m_builder, "adw_toastOverlay")), toast);
}

void MainWindow::onListMusicFilesSelectionChanged()
{
    //==Update Selected Music Files==//
    m_selectedMusicFiles.clear();
    GList* selectedRows = gtk_list_box_get_selected_rows(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "gtk_listMusicFiles")));
    for(GList* list = selectedRows; list; list = list->next)
    {
        GtkListBoxRow* row = GTK_LIST_BOX_ROW(list->data);
        m_selectedMusicFiles.push_back(m_musicFolder.getFiles()[gtk_list_box_row_get_index(row)]);
    }
    g_list_free(selectedRows);
    //==Update UI==//
    gtk_editable_set_editable(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")), true);
    gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnSaveTags")), true);
    gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnMenuTagActions")), true);
    adw_flap_set_reveal_flap(ADW_FLAP(gtk_builder_get_object(m_builder, "adw_flap")), true);
    //==No Files Selected==//
    if(m_selectedMusicFiles.size() == 0)
    {
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTitle")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtArtist")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbum")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtYear")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTrack")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbumArtist")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtGenre")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtComment")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtDuration")), "");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFileSize")), "");
        gtk_image_clear(GTK_IMAGE(gtk_builder_get_object(m_builder, "gtk_imgAlbumArt")));
        adw_flap_set_reveal_flap(ADW_FLAP(gtk_builder_get_object(m_builder, "adw_flap")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnSaveTags")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnMenuTagActions")), false);
    }
    //==One File Selected==//
    else if(m_selectedMusicFiles.size() == 1)
    {
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")), std::regex_replace(m_selectedMusicFiles[0]->getFilename(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTitle")), std::regex_replace(m_selectedMusicFiles[0]->getTitle(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtArtist")), std::regex_replace(m_selectedMusicFiles[0]->getArtist(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbum")), std::regex_replace(m_selectedMusicFiles[0]->getAlbum(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtYear")), std::to_string(m_selectedMusicFiles[0]->getYear()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTrack")), std::to_string(m_selectedMusicFiles[0]->getTrack()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbumArtist")), std::regex_replace(m_selectedMusicFiles[0]->getAlbumArtist(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtGenre")), std::regex_replace(m_selectedMusicFiles[0]->getGenre(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtComment")), std::regex_replace(m_selectedMusicFiles[0]->getComment(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtDuration")), m_selectedMusicFiles[0]->getDurationAsString().c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFileSize")), m_selectedMusicFiles[0]->getFileSizeAsString().c_str());
        GtkHelpers::gtk_image_set_from_byte_vector(GTK_IMAGE(gtk_builder_get_object(m_builder, "gtk_imgAlbumArt")), m_selectedMusicFiles[0]->getAlbumArt());
    }
    //==Multiple Files Selected==//
    else
    {
        bool haveSameTitle = true;
        bool haveSameArtist = true;
        bool haveSameAlbum = true;
        bool haveSameYear = true;
        bool haveSameTrack = true;
        bool haveSameAlbumArtist = true;
        bool haveSameGenre = true;
        bool haveSameComment = true;
        bool haveSameAlbumArt = true;
        int totalDuration = 0;
        std::uintmax_t totalFileSize = 0;
        for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
        {
            if (m_selectedMusicFiles[0]->getTitle() != musicFile->getTitle())
            {
                haveSameTitle = false;
            }
            if (m_selectedMusicFiles[0]->getArtist() != musicFile->getArtist())
            {
                haveSameArtist = false;
            }
            if (m_selectedMusicFiles[0]->getAlbum() != musicFile->getAlbum())
            {
                haveSameAlbum = false;
            }
            if (m_selectedMusicFiles[0]->getYear() != musicFile->getYear())
            {
                haveSameYear = false;
            }
            if (m_selectedMusicFiles[0]->getTrack() != musicFile->getTrack())
            {
                haveSameTrack = false;
            }
            if (m_selectedMusicFiles[0]->getAlbumArtist() != musicFile->getAlbumArtist())
            {
                haveSameAlbumArtist = false;
            }
            if (m_selectedMusicFiles[0]->getGenre() != musicFile->getGenre())
            {
                haveSameGenre = false;
            }
            if (m_selectedMusicFiles[0]->getComment() != musicFile->getComment())
            {
                haveSameComment = false;
            }
            if  (m_selectedMusicFiles[0]->getAlbumArt() != musicFile->getAlbumArt())
            {
                haveSameAlbumArt = false;
            }
            totalDuration += musicFile->getDuration();
            totalFileSize += musicFile->getFileSize();
        }
        gtk_editable_set_editable(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")), false);
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFilename")), "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTitle")), haveSameTitle ? std::regex_replace(m_selectedMusicFiles[0]->getTitle(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtArtist")), haveSameArtist ? std::regex_replace(m_selectedMusicFiles[0]->getArtist(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbum")), haveSameAlbum ? std::regex_replace(m_selectedMusicFiles[0]->getAlbum(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtYear")), haveSameYear ? std::to_string(m_selectedMusicFiles[0]->getYear()).c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtTrack")), haveSameTrack ? std::to_string(m_selectedMusicFiles[0]->getTrack()).c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtAlbumArtist")), haveSameAlbumArtist ? std::regex_replace(m_selectedMusicFiles[0]->getAlbumArtist(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtGenre")), haveSameGenre ? std::regex_replace(m_selectedMusicFiles[0]->getGenre(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtComment")), haveSameComment ? std::regex_replace(m_selectedMusicFiles[0]->getComment(), std::regex("\\&"), "&amp;").c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtDuration")), MediaHelpers::durationToString(totalDuration).c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "gtk_txtFileSize")), MediaHelpers::fileSizeToString(totalFileSize).c_str());
        if(haveSameAlbumArt)
        {
            GtkHelpers::gtk_image_set_from_byte_vector(GTK_IMAGE(gtk_builder_get_object(m_builder, "gtk_imgAlbumArt")), m_selectedMusicFiles[0]->getAlbumArt());
        }
        else
        {
            gtk_image_clear(GTK_IMAGE(gtk_builder_get_object(m_builder, "gtk_imgAlbumArt")));
        }
    }
}
