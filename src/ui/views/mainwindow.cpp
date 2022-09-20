#include "mainwindow.hpp"
#include <regex>
#include <utility>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"
#include "../controls/progressdialog.hpp"
#include "../../helpers/gtkhelpers.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI::Controls;
using namespace NickvisionTagger::UI::Views;

MainWindow::MainWindow(GtkApplication* application, const MainWindowController& controller) : m_controller{ controller }, m_gobj{ adw_application_window_new(application) }
{
    //Window Settings
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 1000, 800);
    g_signal_connect(m_gobj, "show", G_CALLBACK((void (*)(GtkWidget*, gpointer*))[](GtkWidget*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onStartup(); }), this);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_gobj), "devel");
    //Header Bar
    m_headerBar = adw_header_bar_new();
    m_adwTitle = adw_window_title_new(m_controller.getAppInfo().getShortName().c_str(), m_controller.getMusicFolderPath().c_str());
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), m_adwTitle);
    //Open Music Folder Button
    m_btnOpenMusicFolder = gtk_button_new();
    GtkWidget* btnOpenMusicFolderContent{ adw_button_content_new() };
    adw_button_content_set_icon_name(ADW_BUTTON_CONTENT(btnOpenMusicFolderContent), "folder-open-symbolic");
    adw_button_content_set_label(ADW_BUTTON_CONTENT(btnOpenMusicFolderContent), "Open");
    gtk_button_set_child(GTK_BUTTON(m_btnOpenMusicFolder), btnOpenMusicFolderContent);
    gtk_widget_set_tooltip_text(m_btnOpenMusicFolder, "Open Music Folder (Ctrl+O)");
    gtk_actionable_set_action_name(GTK_ACTIONABLE(m_btnOpenMusicFolder), "win.openMusicFolder");
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnOpenMusicFolder);
    //Reload Music Folder Button
    m_btnReloadMusicFolder = gtk_button_new();
    gtk_button_set_icon_name(GTK_BUTTON(m_btnReloadMusicFolder), "view-refresh-symbolic");
    gtk_widget_set_tooltip_text(m_btnReloadMusicFolder, "Reload Music Folder (F5)");
    gtk_widget_set_visible(m_btnReloadMusicFolder, false);
    gtk_actionable_set_action_name(GTK_ACTIONABLE(m_btnReloadMusicFolder), "win.reloadMusicFolder");
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnReloadMusicFolder);
    //Menu Help Button
    m_btnMenuHelp = gtk_menu_button_new();
    GMenu* menuHelp{ g_menu_new() };
    g_menu_append(menuHelp, "Preferences", "win.preferences");
    g_menu_append(menuHelp, "Keyboard Shortcuts", "win.keyboardShortcuts");
    g_menu_append(menuHelp, std::string("About " + m_controller.getAppInfo().getShortName()).c_str(), "win.about");
    gtk_menu_button_set_direction(GTK_MENU_BUTTON(m_btnMenuHelp), GTK_ARROW_NONE);
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuHelp), G_MENU_MODEL(menuHelp));
    gtk_widget_set_tooltip_text(m_btnMenuHelp, "Main Menu");
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuHelp);
    g_object_unref(menuHelp);
    //Header End Separator
    m_sepHeaderEnd = gtk_separator_new(GTK_ORIENTATION_HORIZONTAL);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_sepHeaderEnd), "spacer");
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_sepHeaderEnd);
    //Apply Button
    m_btnApply = gtk_button_new();
    gtk_button_set_label(GTK_BUTTON(m_btnApply), "Apply");
    gtk_widget_set_tooltip_text(m_btnApply, "Apply (Ctrl+S)");
    gtk_widget_set_visible(m_btnApply, false);
    gtk_actionable_set_action_name(GTK_ACTIONABLE(m_btnApply), "win.apply");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnApply), "suggested-action");
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnApply);
    //Menu Tag Actions Button
    m_btnMenuTagActions = gtk_menu_button_new();
    GMenu* menuTagActions{ g_menu_new() };
    GMenu* menuAlbumArt{ g_menu_new() };
    GMenu* menuConvert{ g_menu_new() };
    g_menu_append(menuAlbumArt, "Insert Album Art", "win.insertAlbumArt");
    g_menu_append(menuAlbumArt, "Remove Album Art", "win.removeAlbumArt");
    g_menu_append(menuConvert, "Filename to Tag", "win.filenameToTag");
    g_menu_append(menuConvert, "Tag to Filename", "win.tagToFilename");
    g_menu_append(menuTagActions, "Delete Tag", "win.deleteTag");
    g_menu_append_section(menuTagActions, nullptr, G_MENU_MODEL(menuAlbumArt));
    g_menu_append_section(menuTagActions, nullptr, G_MENU_MODEL(menuConvert));
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuTagActions), "document-properties-symbolic");
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuTagActions), G_MENU_MODEL(menuTagActions));
    gtk_widget_set_tooltip_text(m_btnMenuTagActions, "Tag Actions");
    gtk_widget_set_visible(m_btnMenuTagActions, false);
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuTagActions);
    g_object_unref(menuTagActions);
    g_object_unref(menuAlbumArt);
    g_object_unref(menuConvert);
    //Toast Overlay
    m_toastOverlay = adw_toast_overlay_new();
    gtk_widget_set_hexpand(m_toastOverlay, true);
    gtk_widget_set_vexpand(m_toastOverlay, true);
    //No Files Status Page
    m_pageStatusNoFiles = adw_status_page_new();
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_pageStatusNoFiles), "org.nickvision.tagger-symbolic");
    adw_status_page_set_title(ADW_STATUS_PAGE(m_pageStatusNoFiles), "No Music Files Found");
    adw_status_page_set_description(ADW_STATUS_PAGE(m_pageStatusNoFiles), "Open a folder with music files inside to get started.");
    //Tagger Flap Page
    m_pageFlapTagger = adw_flap_new();
    adw_flap_set_flap_position(ADW_FLAP(m_pageFlapTagger), GTK_PACK_END);
    adw_flap_set_reveal_flap(ADW_FLAP(m_pageFlapTagger), false);
    //Tagger Flap Content
    m_scrollTaggerContent = gtk_scrolled_window_new();
    gtk_widget_set_margin_start(m_scrollTaggerContent, 10);
    gtk_widget_set_margin_top(m_scrollTaggerContent, 10);
    gtk_widget_set_margin_end(m_scrollTaggerContent, 10);
    gtk_widget_set_margin_bottom(m_scrollTaggerContent, 10);
    //List Music Files
    m_listMusicFiles = gtk_list_box_new();
    g_signal_connect(m_listMusicFiles, "selected-rows-changed", G_CALLBACK((void (*)(GtkListBox*, gpointer*))[](GtkListBox*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onListMusicFilesSelectionChanged(); }), this);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_listMusicFiles), "boxed-list");
    gtk_list_box_set_selection_mode(GTK_LIST_BOX(m_listMusicFiles), GTK_SELECTION_MULTIPLE);
    gtk_list_box_set_activate_on_single_click(GTK_LIST_BOX(m_listMusicFiles), false);
    gtk_scrolled_window_set_child(GTK_SCROLLED_WINDOW(m_scrollTaggerContent), m_listMusicFiles);
    adw_flap_set_content(ADW_FLAP(m_pageFlapTagger), m_scrollTaggerContent);
    //Tagger Flap Separator
    m_sepTagger = gtk_separator_new(GTK_ORIENTATION_VERTICAL);
    adw_flap_set_separator(ADW_FLAP(m_pageFlapTagger), m_sepTagger);
    //Tagger Flap Box
    m_boxTaggerFlap = gtk_box_new(GTK_ORIENTATION_VERTICAL, 6);
    gtk_widget_set_margin_start(m_boxTaggerFlap, 10);
    gtk_widget_set_margin_top(m_boxTaggerFlap, 10);
    gtk_widget_set_margin_end(m_boxTaggerFlap, 10);
    gtk_widget_set_margin_bottom(m_boxTaggerFlap, 10);
    //Filename
    m_lblFilename = gtk_label_new("Filename");
    gtk_widget_set_halign(m_lblFilename, GTK_ALIGN_START);
    m_txtFilename = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtFilename), "Enter filename here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblFilename);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtFilename);
    //Title
    m_lblTitle = gtk_label_new("Title");
    gtk_widget_set_halign(m_lblTitle, GTK_ALIGN_START);
    m_txtTitle = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtTitle), "Enter title here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblTitle);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtTitle);
    //Artist
    m_lblArtist = gtk_label_new("Artist");
    gtk_widget_set_halign(m_lblArtist, GTK_ALIGN_START);
    m_txtArtist = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtArtist), "Enter artist here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblArtist);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtArtist);
    //Album
    m_lblAlbum = gtk_label_new("Album");
    gtk_widget_set_halign(m_lblAlbum, GTK_ALIGN_START);
    m_txtAlbum = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtAlbum), "Enter album here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblAlbum);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtAlbum);
    //Year
    m_lblYear = gtk_label_new("Year");
    gtk_widget_set_halign(m_lblYear, GTK_ALIGN_START);
    m_txtYear = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtYear), "Enter year here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblYear);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtYear);
    //Track
    m_lblTrack = gtk_label_new("Track");
    gtk_widget_set_halign(m_lblTrack, GTK_ALIGN_START);
    m_txtTrack = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtTrack), "Enter track here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblTrack);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtTrack);
    //Album Artist
    m_lblAlbumArtist = gtk_label_new("Album Artist");
    gtk_widget_set_halign(m_lblAlbumArtist, GTK_ALIGN_START);
    m_txtAlbumArtist = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtAlbumArtist), "Enter album artist here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblAlbumArtist);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtAlbumArtist);
    //Genre
    m_lblGenre = gtk_label_new("Genre");
    gtk_widget_set_halign(m_lblGenre, GTK_ALIGN_START);
    m_txtGenre = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtGenre), "Enter genre here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblGenre);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtGenre);
    //Comment
    m_lblComment = gtk_label_new("Comment");
    gtk_widget_set_halign(m_lblComment, GTK_ALIGN_START);
    m_txtComment = gtk_entry_new();
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtComment), "Enter comment here");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblComment);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtComment);
    //Duration
    m_lblDuration = gtk_label_new("Duration");
    gtk_widget_set_halign(m_lblDuration, GTK_ALIGN_START);
    m_txtDuration = gtk_entry_new();
    gtk_editable_set_editable(GTK_EDITABLE(m_txtDuration), false);
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtDuration), "00:00:00");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblDuration);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtDuration);
    //File Size
    m_lblFileSize = gtk_label_new("File Size");
    gtk_widget_set_halign(m_lblFileSize, GTK_ALIGN_START);
    m_txtFileSize = gtk_entry_new();
    gtk_editable_set_editable(GTK_EDITABLE(m_txtFileSize), false);
    gtk_entry_set_placeholder_text(GTK_ENTRY(m_txtFileSize), "0 MB");
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblFileSize);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_txtFileSize);
    //Album Art
    m_lblAlbumArt = gtk_label_new("Album Art");
    gtk_widget_set_halign(m_lblAlbumArt, GTK_ALIGN_START);
    m_frmAlbumArt = gtk_frame_new(nullptr);
    m_imgAlbumArt = gtk_image_new();
    gtk_widget_set_size_request(m_imgAlbumArt, 380, 380);
    gtk_frame_set_child(GTK_FRAME(m_frmAlbumArt), m_imgAlbumArt);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_lblAlbumArt);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_frmAlbumArt);
    //Tagger Flap Flap
    m_scrollTaggerFlap = gtk_scrolled_window_new();
    gtk_widget_set_size_request(m_scrollTaggerFlap, 420, -1);
    gtk_scrolled_window_set_child(GTK_SCROLLED_WINDOW(m_scrollTaggerFlap), m_boxTaggerFlap);
    adw_flap_set_flap(ADW_FLAP(m_pageFlapTagger), m_scrollTaggerFlap);
    //View Stack
    m_viewStack = adw_view_stack_new();
    adw_view_stack_add_named(ADW_VIEW_STACK(m_viewStack), m_pageStatusNoFiles, "pageNoFiles");
    adw_view_stack_add_named(ADW_VIEW_STACK(m_viewStack), m_pageFlapTagger, "pageTagger");
    adw_toast_overlay_set_child(ADW_TOAST_OVERLAY(m_toastOverlay), m_viewStack);
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_toastOverlay);
    adw_application_window_set_content(ADW_APPLICATION_WINDOW(m_gobj), m_mainBox);
    //Send Toast Callback
    m_controller.registerSendToastCallback([&](const std::string& message) { adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(m_toastOverlay), adw_toast_new(message.c_str())); });
    //Send Notification Callback
    m_controller.registerSendNotificationCallback([&](const std::string& title, const std::string& message)
    {
        GNotification* notification{ g_notification_new(title.c_str()) };
        g_notification_set_body(notification, message.c_str());
        g_application_send_notification(g_application_get_default(), title.c_str(), notification);
        g_object_unref(notification);
    });
    //Music Folder Updated Callback
    m_controller.registerMusicFolderUpdatedCallback([&]() { onMusicFolderUpdated(); });
    //Open Music Folder Action
    m_actOpenMusicFolder = g_simple_action_new("openMusicFolder", nullptr);
    g_signal_connect(m_actOpenMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onOpenMusicFolder(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actOpenMusicFolder));
    gtk_application_set_accels_for_action(application, "win.openMusicFolder", new const char*[2]{ "<Ctrl>o", nullptr });
    //Reload Music Folder Action
    m_actReloadMusicFolder = g_simple_action_new("reloadMusicFolder", nullptr);
    g_signal_connect(m_actReloadMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onMusicFolderUpdated(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actReloadMusicFolder));
    gtk_application_set_accels_for_action(application, "win.reloadMusicFolder", new const char*[2]{ "F5", nullptr });
    //Apply Action
    m_actApply = g_simple_action_new("apply", nullptr);
    g_signal_connect(m_actApply, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onApply(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actApply));
    gtk_application_set_accels_for_action(application, "win.apply", new const char*[2]{ "<Ctrl>s", nullptr });
    //Delete Tag Action
    m_actDeleteTag = g_simple_action_new("deleteTag", nullptr);
    g_signal_connect(m_actDeleteTag, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onDeleteTag(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actDeleteTag));
    gtk_application_set_accels_for_action(application, "win.deleteTag", new const char*[2]{ "Delete", nullptr });
    //Insert Album Art
    m_actInsertAlbumArt = g_simple_action_new("insertAlbumArt", nullptr);
    g_signal_connect(m_actInsertAlbumArt, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onInsertAlbumArt(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actInsertAlbumArt));
    //Remove Album Art
    m_actRemoveAlbumArt = g_simple_action_new("removeAlbumArt", nullptr);
    g_signal_connect(m_actRemoveAlbumArt, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onRemoveAlbumArt(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actRemoveAlbumArt));
    //Filename to Tag
    m_actFilenameToTag = g_simple_action_new("filenameToTag", nullptr);
    g_signal_connect(m_actFilenameToTag, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onFilenameToTag(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actFilenameToTag));
    gtk_application_set_accels_for_action(application, "win.filenameToTag", new const char*[2]{ "<Ctrl>f", nullptr });
    //Tag to Filename
    m_actTagToFilename = g_simple_action_new("tagToFilename", nullptr);
    g_signal_connect(m_actTagToFilename, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onTagToFilename(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actTagToFilename));
    gtk_application_set_accels_for_action(application, "win.tagToFilename", new const char*[2]{ "<Ctrl>t", nullptr });
    //Preferences Action
    m_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onPreferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actPreferences));
    gtk_application_set_accels_for_action(application, "win.preferences", new const char*[2]{ "<Ctrl>period", nullptr });
    //Keyboard Shortcuts Action
    m_actKeyboardShortcuts = g_simple_action_new("keyboardShortcuts", nullptr);
    g_signal_connect(m_actKeyboardShortcuts, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onKeyboardShortcuts(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actKeyboardShortcuts));
    //About Action
    m_actAbout = g_simple_action_new("about", nullptr);
    g_signal_connect(m_actAbout, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction*, GVariant*, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onAbout(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actAbout));
    gtk_application_set_accels_for_action(application, "win.about", new const char*[2]{ "F1", nullptr });
}

GtkWidget* MainWindow::gobj()
{
    return m_gobj;
}

void MainWindow::show()
{
    gtk_widget_show(m_gobj);
}

void MainWindow::onStartup()
{
    ProgressDialog* progressDialog{ new ProgressDialog(GTK_WINDOW(m_gobj), "Starting application...", [&]()
    {
        m_controller.startup();
    }) };
    progressDialog->show();
}

void MainWindow::onMusicFolderUpdated()
{
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(m_adwTitle), m_controller.getMusicFolderPath().c_str());
    gtk_widget_set_visible(m_btnReloadMusicFolder, !m_controller.getMusicFolderPath().empty());
    gtk_list_box_unselect_all(GTK_LIST_BOX(m_listMusicFiles));
    for(GtkWidget* row : m_listMusicFilesRows)
    {
        gtk_list_box_remove(GTK_LIST_BOX(m_listMusicFiles), row);
    }
    m_listMusicFilesRows.clear();
    ProgressDialog* progressDialog{ new ProgressDialog(GTK_WINDOW(m_gobj), "Loading music files...", [&]()
    {
        m_controller.reloadMusicFolder();
    }, [&]()
    {
        std::size_t musicFilesCount{ m_controller.getMusicFileCount() };
        int id{ 1 };
        adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), musicFilesCount > 0 ? "pageTagger" : "pageNoFiles");
        if(musicFilesCount > 0)
        {
            for(const std::shared_ptr<MusicFile>& musicFile : m_controller.getMusicFiles())
            {
                GtkWidget* row{ adw_action_row_new() };
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), std::regex_replace(musicFile->getFilename(), std::regex("\\&"), "&amp;").c_str());
                adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::to_string(id).c_str());
                gtk_list_box_append(GTK_LIST_BOX(m_listMusicFiles), row);
                m_listMusicFilesRows.push_back(row);
                g_main_context_iteration(g_main_context_default(), false);
                id++;
            }
            adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(m_toastOverlay), adw_toast_new(std::string("Loaded " + std::to_string(musicFilesCount) + " music files.").c_str()));
        }
    }) };
    progressDialog->show();
}

void MainWindow::onOpenMusicFolder()
{
    GtkFileChooserNative* openFolderDialog{ gtk_file_chooser_native_new("Open Music Folder", GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_SELECT_FOLDER, "_Open", "_Cancel") };
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openFolderDialog), true);
    g_signal_connect(openFolderDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer*))([](GtkNativeDialog* dialog, gint response_id, gpointer* data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
            GFile* file{ gtk_file_chooser_get_file(GTK_FILE_CHOOSER(dialog)) };
            mainWindow->m_controller.openMusicFolder(g_file_get_path(file));
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openFolderDialog));
}

void MainWindow::onApply()
{

}

void MainWindow::onDeleteTag()
{

}

void MainWindow::onInsertAlbumArt()
{

}

void MainWindow::onRemoveAlbumArt()
{

}

void MainWindow::onFilenameToTag()
{

}

void MainWindow::onTagToFilename()
{

}

void MainWindow::onPreferences()
{
    PreferencesDialog* preferencesDialog{ new PreferencesDialog(GTK_WINDOW(m_gobj), m_controller.createPreferencesDialogController()) };
    std::pair<PreferencesDialog*, MainWindow*>* pointers{ new std::pair<PreferencesDialog*, MainWindow*>(preferencesDialog, this) };
    g_signal_connect(preferencesDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget*, gpointer* data)
    {
        std::pair<PreferencesDialog*, MainWindow*>* pointers{ reinterpret_cast<std::pair<PreferencesDialog*, MainWindow*>*>(data) };
        delete pointers->first;
        pointers->second->m_controller.onConfigurationChanged();
        delete pointers;
    })), pointers);
    preferencesDialog->show();
}

void MainWindow::onKeyboardShortcuts()
{
    ShortcutsDialog* shortcutsDialog{ new ShortcutsDialog(GTK_WINDOW(m_gobj)) };
    g_signal_connect(shortcutsDialog->gobj(), "hide", G_CALLBACK((void (*)(GtkWidget*, gpointer*))([](GtkWidget*, gpointer* data)
    {
        ShortcutsDialog* dialog{ reinterpret_cast<ShortcutsDialog*>(data) };
        delete dialog;
    })), shortcutsDialog);
    shortcutsDialog->show();
}

void MainWindow::onAbout()
{
    adw_show_about_window(GTK_WINDOW(m_gobj),
                          "application-name", m_controller.getAppInfo().getShortName().c_str(),
                          "application-icon", m_controller.getAppInfo().getId().c_str(),
                          "version", m_controller.getAppInfo().getVersion().c_str(),
                          "comments", m_controller.getAppInfo().getDescription().c_str(),
                          "developer-name", "Nickvision",
                          "license-type", GTK_LICENSE_GPL_3_0,
                          "copyright", "(C) Nickvision 2021-2022",
                          "issue-url", m_controller.getAppInfo().getIssueTracker().c_str(),
                          "website", m_controller.getAppInfo().getGitHubRepo().c_str(),
                          "developers", new const char*[2]{ "Nicholas Logozzo", nullptr },
                          "release-notes", m_controller.getAppInfo().getChangelog().c_str(),
                          nullptr);
}

void MainWindow::onListMusicFilesSelectionChanged()
{
    //Update Selected Music Files
    std::vector<int> selectedIndexes;
    GList* selectedRows{ gtk_list_box_get_selected_rows(GTK_LIST_BOX(m_listMusicFiles)) };
    for(GList* list{ selectedRows }; list; list = list->next)
    {
        selectedIndexes.push_back(gtk_list_box_row_get_index(GTK_LIST_BOX_ROW(list->data)));
    }
    m_controller.updateSelectedMusicFiles(selectedIndexes);
    //Update UI
    gtk_editable_set_editable(GTK_EDITABLE(m_txtFilename), true);
    gtk_widget_set_visible(m_btnApply, true);
    gtk_widget_set_visible(m_btnMenuTagActions, true);
    adw_flap_set_reveal_flap(ADW_FLAP(m_pageFlapTagger), true);
    //No Selected Files
    if(m_controller.getSelectedMusicFiles().size() == 0)
    {
        gtk_widget_set_visible(m_btnApply, false);
        gtk_widget_set_visible(m_btnMenuTagActions, false);
        adw_flap_set_reveal_flap(ADW_FLAP(m_pageFlapTagger), false);
        gtk_editable_set_text(GTK_EDITABLE(m_txtFilename), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtTitle), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtArtist), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbum), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtYear), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtTrack), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbumArtist), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtGenre), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtComment), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), "");
        gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
    }
    //One File Selected
    else if(m_controller.getSelectedMusicFiles().size() == 1)
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_controller.getSelectedMusicFiles()[0] };
        gtk_editable_set_text(GTK_EDITABLE(m_txtFilename), std::regex_replace(firstMusicFile->getFilename(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtTitle), std::regex_replace(firstMusicFile->getTitle(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtArtist), std::regex_replace(firstMusicFile->getArtist(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbum), std::regex_replace(firstMusicFile->getAlbum(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtYear), std::to_string(firstMusicFile->getYear()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtTrack), std::to_string(firstMusicFile->getTrack()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbumArtist), std::regex_replace(firstMusicFile->getAlbumArtist(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtGenre), std::regex_replace(firstMusicFile->getGenre(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtComment), std::regex_replace(firstMusicFile->getComment(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), std::regex_replace(firstMusicFile->getDurationAsString(), std::regex("\\&"), "&amp;").c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), std::regex_replace(firstMusicFile->getFileSizeAsString(), std::regex("\\&"), "&amp;").c_str());
        GtkHelpers::gtk_image_set_from_byte_vector(GTK_IMAGE(m_imgAlbumArt), firstMusicFile->getAlbumArt());
    }
    //Multiple Files Selected
    else
    {

    }
}
