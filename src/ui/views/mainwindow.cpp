#include "mainwindow.hpp"
#include <algorithm>
#include <filesystem>
#include <unordered_map>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"
#include "../controls/comboboxdialog.hpp"
#include "../controls/progressdialog.hpp"
#include "../../helpers/gtkhelpers.hpp"
#include "../../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI::Controls;
using namespace NickvisionTagger::UI::Views;

MainWindow::MainWindow(GtkApplication* application, const MainWindowController& controller) : m_controller{ controller }, m_gobj{ adw_application_window_new(application) }
{
    //Window Settings
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 1000, 800);
    g_signal_connect(m_gobj, "close_request", G_CALLBACK((void (*)(GtkWidget*, gpointer))[](GtkWidget*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onCloseRequest(); }), this);
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
    g_menu_append(menuTagActions, "Delete Tags", "win.deleteTags");
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
    adw_flap_set_fold_policy(ADW_FLAP(m_pageFlapTagger), ADW_FLAP_FOLD_POLICY_NEVER);
    //SearchBar Music Files
    m_txtSearchMusicFiles = gtk_search_entry_new();
    g_object_set(m_txtSearchMusicFiles, "placeholder-text", "Search...", nullptr);
    g_signal_connect(m_txtSearchMusicFiles, "search-changed", G_CALLBACK((void (*)(GtkSearchEntry*, gpointer))[](GtkSearchEntry*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onTxtSearchMusicFilesChanged(); }), this);
    //List Music Files
    m_listMusicFiles = gtk_list_box_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_listMusicFiles), "boxed-list");
    gtk_list_box_set_selection_mode(GTK_LIST_BOX(m_listMusicFiles), GTK_SELECTION_MULTIPLE);
    gtk_list_box_set_activate_on_single_click(GTK_LIST_BOX(m_listMusicFiles), false);
    g_signal_connect(m_listMusicFiles, "selected-rows-changed", G_CALLBACK((void (*)(GtkListBox*, gpointer))[](GtkListBox*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onListMusicFilesSelectionChanged(); }), this);
    //Tagger Flap Content
    m_scrollTaggerContent = gtk_scrolled_window_new();
    gtk_widget_set_vexpand(m_scrollTaggerContent, true);
    gtk_widget_set_size_request(m_scrollTaggerContent, 400, -1);
    gtk_scrolled_window_set_child(GTK_SCROLLED_WINDOW(m_scrollTaggerContent), m_listMusicFiles);
    m_boxTaggerContent = gtk_box_new(GTK_ORIENTATION_VERTICAL, 10);
    gtk_widget_set_margin_start(m_boxTaggerContent, 10);
    gtk_widget_set_margin_top(m_boxTaggerContent, 10);
    gtk_widget_set_margin_end(m_boxTaggerContent, 10);
    gtk_widget_set_margin_bottom(m_boxTaggerContent, 10);
    gtk_box_append(GTK_BOX(m_boxTaggerContent), m_txtSearchMusicFiles);
    gtk_box_append(GTK_BOX(m_boxTaggerContent), m_scrollTaggerContent);
    adw_flap_set_content(ADW_FLAP(m_pageFlapTagger), m_boxTaggerContent);
    //Tagger Flap Separator
    m_sepTagger = gtk_separator_new(GTK_ORIENTATION_VERTICAL);
    adw_flap_set_separator(ADW_FLAP(m_pageFlapTagger), m_sepTagger);
    //Album Art
    m_stackAlbumArt = adw_view_stack_new();
    gtk_widget_set_halign(m_stackAlbumArt, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_stackAlbumArt, 300, 300);
    m_statusNoAlbumArt = adw_status_page_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_statusNoAlbumArt), "card");
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_statusNoAlbumArt), "folder-music-symbolic");
    adw_view_stack_add_named(ADW_VIEW_STACK(m_stackAlbumArt), m_statusNoAlbumArt, "noImage");
    m_imgAlbumArt = gtk_image_new();
    adw_view_stack_add_named(ADW_VIEW_STACK(m_stackAlbumArt), m_imgAlbumArt, "image");
    //Properties Group
    m_adwGrpProperties = adw_preferences_group_new();
    //Filename
    m_txtFilename = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtFilename), "Filename");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtFilename);
    //Title
    m_txtTitle = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtTitle), "Title");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtTitle);
    //Artist
    m_txtArtist = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtArtist), "Artist");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtArtist);
    //Album
    m_txtAlbum = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtAlbum), "Album");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtAlbum);
    //Year
    m_txtYear = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtYear), "Year");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtYear);
    //Track
    m_txtTrack = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtTrack), "Track");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtTrack);
    //Album Artist
    m_txtAlbumArtist = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtAlbumArtist), "Album Artist");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtAlbumArtist);
    //Genre
    m_txtGenre = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtGenre), "Genre");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtGenre);
    //Comment
    m_txtComment = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtComment), "Comment");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtComment);
    //Duration
    m_txtDuration = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtDuration), "Duration");
    gtk_editable_set_editable(GTK_EDITABLE(m_txtDuration), false);
    gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), "00:00:00");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtDuration);
    //Chromaprint Fingerprint
    m_txtChromaprintFingerprint = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtChromaprintFingerprint), "Fingerprint");
    gtk_editable_set_editable(GTK_EDITABLE(m_txtChromaprintFingerprint), false);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtChromaprintFingerprint);
    //File Size
    m_txtFileSize = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_txtFileSize), "File Size");
    gtk_editable_set_editable(GTK_EDITABLE(m_txtFileSize), false);
    gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), "0 MB");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_adwGrpProperties), m_txtFileSize);
    //Tagger Flap Flap
    m_scrollTaggerFlap = gtk_scrolled_window_new();
    gtk_widget_set_hexpand(m_scrollTaggerFlap, true);
    m_boxTaggerFlap = gtk_box_new(GTK_ORIENTATION_VERTICAL, 40);
    gtk_widget_set_margin_start(m_boxTaggerFlap, 60);
    gtk_widget_set_margin_top(m_boxTaggerFlap, 20);
    gtk_widget_set_margin_end(m_boxTaggerFlap, 60);
    gtk_widget_set_margin_bottom(m_boxTaggerFlap, 20);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_stackAlbumArt);
    gtk_box_append(GTK_BOX(m_boxTaggerFlap), m_adwGrpProperties);
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
    m_controller.registerMusicFolderUpdatedCallback([&](bool sendToast) { onMusicFolderUpdated(sendToast); });
    //Open Music Folder Action
    m_actOpenMusicFolder = g_simple_action_new("openMusicFolder", nullptr);
    g_signal_connect(m_actOpenMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onOpenMusicFolder(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actOpenMusicFolder));
    gtk_application_set_accels_for_action(application, "win.openMusicFolder", new const char*[2]{ "<Ctrl>o", nullptr });
    //Reload Music Folder Action
    m_actReloadMusicFolder = g_simple_action_new("reloadMusicFolder", nullptr);
    g_signal_connect(m_actReloadMusicFolder, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onMusicFolderUpdated(true); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actReloadMusicFolder));
    gtk_application_set_accels_for_action(application, "win.reloadMusicFolder", new const char*[2]{ "F5", nullptr });
    //Apply Action
    m_actApply = g_simple_action_new("apply", nullptr);
    g_signal_connect(m_actApply, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onApply(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actApply));
    gtk_application_set_accels_for_action(application, "win.apply", new const char*[2]{ "<Ctrl>s", nullptr });
    //Delete Tags Action
    m_actDeleteTags = g_simple_action_new("deleteTags", nullptr);
    g_signal_connect(m_actDeleteTags, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onDeleteTags(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actDeleteTags));
    gtk_application_set_accels_for_action(application, "win.deleteTags", new const char*[2]{ "Delete", nullptr });
    //Insert Album Art
    m_actInsertAlbumArt = g_simple_action_new("insertAlbumArt", nullptr);
    g_signal_connect(m_actInsertAlbumArt, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onInsertAlbumArt(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actInsertAlbumArt));
    gtk_application_set_accels_for_action(application, "win.insertAlbumArt", new const char*[2]{ "<Ctrl><Shift>o", nullptr });
    //Remove Album Art
    m_actRemoveAlbumArt = g_simple_action_new("removeAlbumArt", nullptr);
    g_signal_connect(m_actRemoveAlbumArt, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onRemoveAlbumArt(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actRemoveAlbumArt));
    gtk_application_set_accels_for_action(application, "win.removeAlbumArt", new const char*[2]{ "<Ctrl>Delete", nullptr });
    //Filename to Tag
    m_actFilenameToTag = g_simple_action_new("filenameToTag", nullptr);
    g_signal_connect(m_actFilenameToTag, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onFilenameToTag(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actFilenameToTag));
    gtk_application_set_accels_for_action(application, "win.filenameToTag", new const char*[2]{ "<Ctrl>f", nullptr });
    //Tag to Filename
    m_actTagToFilename = g_simple_action_new("tagToFilename", nullptr);
    g_signal_connect(m_actTagToFilename, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onTagToFilename(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actTagToFilename));
    gtk_application_set_accels_for_action(application, "win.tagToFilename", new const char*[2]{ "<Ctrl>t", nullptr });
    //Preferences Action
    m_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onPreferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actPreferences));
    gtk_application_set_accels_for_action(application, "win.preferences", new const char*[2]{ "<Ctrl>period", nullptr });
    //Keyboard Shortcuts Action
    m_actKeyboardShortcuts = g_simple_action_new("keyboardShortcuts", nullptr);
    g_signal_connect(m_actKeyboardShortcuts, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onKeyboardShortcuts(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actKeyboardShortcuts));
    //About Action
    m_actAbout = g_simple_action_new("about", nullptr);
    g_signal_connect(m_actAbout, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onAbout(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actAbout));
    gtk_application_set_accels_for_action(application, "win.about", new const char*[2]{ "F1", nullptr });
    //Drop Target
    m_dropTarget = gtk_drop_target_new(G_TYPE_FILE, GDK_ACTION_COPY);
    g_signal_connect(m_dropTarget, "drop", G_CALLBACK((int (*)(GtkDropTarget*, const GValue*, gdouble, gdouble, gpointer))[](GtkDropTarget*, const GValue* value, gdouble, gdouble, gpointer data) -> int { return reinterpret_cast<MainWindow*>(data)->onDrop(value); }), this);
    gtk_widget_add_controller(m_gobj, GTK_EVENT_CONTROLLER(m_dropTarget));
}

GtkWidget* MainWindow::gobj()
{
    return m_gobj;
}

void MainWindow::start()
{
    gtk_widget_show(m_gobj);
    m_controller.startup();
}

void MainWindow::onCloseRequest()
{
    gtk_list_box_unselect_all(GTK_LIST_BOX(m_listMusicFiles));
}

void MainWindow::onMusicFolderUpdated(bool sendToast)
{
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(m_adwTitle), m_controller.getMusicFolderPath().c_str());
    gtk_widget_set_visible(m_btnReloadMusicFolder, !m_controller.getMusicFolderPath().empty());
    gtk_list_box_unselect_all(GTK_LIST_BOX(m_listMusicFiles));
    for(GtkWidget* row : m_listMusicFilesRows)
    {
        gtk_list_box_remove(GTK_LIST_BOX(m_listMusicFiles), row);
    }
    m_listMusicFilesRows.clear();
    ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Loading music files...", [&]() { m_controller.reloadMusicFolder(); } };
    progressDialog.run();
    std::size_t musicFilesCount{ m_controller.getMusicFileCount() };
    int id{ 1 };
    adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), musicFilesCount > 0 ? "pageTagger" : "pageNoFiles");
    for(const std::shared_ptr<MusicFile>& musicFile : m_controller.getMusicFiles())
    {
        GtkWidget* row{ adw_action_row_new() };
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), musicFile->getFilename().c_str());
        adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::to_string(id).c_str());
        gtk_list_box_append(GTK_LIST_BOX(m_listMusicFiles), row);
        m_listMusicFilesRows.push_back(row);
        g_main_context_iteration(g_main_context_default(), false);
        id++;
    }
    if(musicFilesCount > 0 && sendToast)
    {
        adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(m_toastOverlay), adw_toast_new(std::string("Loaded " + std::to_string(musicFilesCount) + " music files.").c_str()));
    }
}

void MainWindow::onOpenMusicFolder()
{
    GtkFileChooserNative* openFolderDialog{ gtk_file_chooser_native_new("Open Music Folder", GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_SELECT_FOLDER, "_Open", "_Cancel") };
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openFolderDialog), true);
    g_signal_connect(openFolderDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer))([](GtkNativeDialog* dialog, gint response_id, gpointer data)
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
    std::unordered_map<std::string, std::string> tagMap;
    tagMap.insert({ "filename", gtk_editable_get_text(GTK_EDITABLE(m_txtFilename)) });
    tagMap.insert({ "title", gtk_editable_get_text(GTK_EDITABLE(m_txtTitle)) });
    tagMap.insert({ "artist", gtk_editable_get_text(GTK_EDITABLE(m_txtArtist)) });
    tagMap.insert({ "album", gtk_editable_get_text(GTK_EDITABLE(m_txtAlbum)) });
    tagMap.insert({ "year", gtk_editable_get_text(GTK_EDITABLE(m_txtYear)) });
    tagMap.insert({ "track", gtk_editable_get_text(GTK_EDITABLE(m_txtTrack)) });
    tagMap.insert({ "albumArtist", gtk_editable_get_text(GTK_EDITABLE(m_txtAlbumArtist)) });
    tagMap.insert({ "genre", gtk_editable_get_text(GTK_EDITABLE(m_txtGenre)) });
    tagMap.insert({ "comment", gtk_editable_get_text(GTK_EDITABLE(m_txtComment)) });
    ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Saving tags...", [&, tagMap]() { m_controller.saveTags(tagMap); } };
    progressDialog.run();
}

void MainWindow::onDeleteTags()
{
    GtkWidget* messageDialog{ adw_message_dialog_new(GTK_WINDOW(m_gobj), "Delete Tags?", "Are you sure you want to delete the tags of the selected files?") };
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(messageDialog), "no", "No", "yes", "Yes", nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(messageDialog), "yes", ADW_RESPONSE_DESTRUCTIVE);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(messageDialog), "no");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(messageDialog), "no");
    g_signal_connect(messageDialog, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog* dialog, gchar* response, gpointer data)
    {
        gtk_window_destroy(GTK_WINDOW(dialog));
        MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
        if(strcmp(response, "yes") == 0)
        {
            ProgressDialog progressDialog{ GTK_WINDOW(mainWindow->m_gobj), "Deleting tags...", [mainWindow]() { mainWindow->m_controller.deleteTags(); } };
            progressDialog.run();
        }
    })), this);
    gtk_widget_show(messageDialog);
}

void MainWindow::onInsertAlbumArt()
{
    GtkFileChooserNative* openPictureDialog{ gtk_file_chooser_native_new("Insert Album Art", GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_OPEN, "_Open", "_Cancel") };
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openPictureDialog), true);
    GtkFileFilter* imageFilter{ gtk_file_filter_new() };
    gtk_file_filter_add_mime_type(imageFilter, "image/*");
    gtk_file_chooser_add_filter(GTK_FILE_CHOOSER(openPictureDialog), imageFilter);
    g_object_unref(imageFilter);
    g_signal_connect(openPictureDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer))([](GtkNativeDialog* dialog, gint response_id, gpointer data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
            GFile* file{ gtk_file_chooser_get_file(GTK_FILE_CHOOSER(dialog)) };
            std::string path{ g_file_get_path(file) };
            ProgressDialog progressDialog{ GTK_WINDOW(mainWindow->m_gobj), "Inserting album art...", [mainWindow, path]() { mainWindow->m_controller.insertAlbumArt(path); } };
            progressDialog.run();
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openPictureDialog));
}

void MainWindow::onRemoveAlbumArt()
{
    GtkWidget* messageDialog{ adw_message_dialog_new(GTK_WINDOW(m_gobj), "Remove Album Art?", "Are you sure you want to remove the album art from the selected files?") };
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(messageDialog), "no", "No", "yes", "Yes", nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(messageDialog), "yes", ADW_RESPONSE_DESTRUCTIVE);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(messageDialog), "no");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(messageDialog), "no");
    g_signal_connect(messageDialog, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog* dialog, gchar* response, gpointer data)
    {
        gtk_window_destroy(GTK_WINDOW(dialog));
        MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
        if(strcmp(response, "yes") == 0)
        {
            ProgressDialog progressDialog{ GTK_WINDOW(mainWindow->m_gobj), "Removing album art...", [mainWindow]() { mainWindow->m_controller.removeAlbumArt(); } };
            progressDialog.run();
        }
    })), this);
    gtk_widget_show(messageDialog);
}

void MainWindow::onFilenameToTag()
{
    ComboBoxDialog formatStringDialog{ GTK_WINDOW(m_gobj), "Filename to Tag", "Please select a format string.", "Format String", { "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" } };
    std::string formatString = formatStringDialog.run();
    if(!formatString.empty())
    {
        ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Converting filenames to tags...", [&, formatString]() { m_controller.filenameToTag(formatString); } };
        progressDialog.run();
    }
}

void MainWindow::onTagToFilename()
{
    ComboBoxDialog formatStringDialog{ GTK_WINDOW(m_gobj), "Tag to Filename", "Please select a format string.", "Format String", { "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" } };
    std::string formatString = formatStringDialog.run();
    if(!formatString.empty())
    {
        ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Converting tags to filenames...", [&, formatString]() { m_controller.tagToFilename(formatString); } };
        progressDialog.run();
    }
}

void MainWindow::onPreferences()
{
    PreferencesDialog preferencesDialog{ GTK_WINDOW(m_gobj), m_controller.createPreferencesDialogController() };
    preferencesDialog.run();
    m_controller.onConfigurationChanged();
}

void MainWindow::onKeyboardShortcuts()
{
    ShortcutsDialog shortcutsDialog{ GTK_WINDOW(m_gobj) };
    shortcutsDialog.run();
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
                          "designers", new const char*[2]{ "Nicholas Logozzo", nullptr },
                          "artists", new const char*[3]{ "David Lapshin", "noëlle (@jannuary)", nullptr },
                          "release-notes", m_controller.getAppInfo().getChangelog().c_str(),
                          nullptr);
}

void MainWindow::onTxtSearchMusicFilesChanged()
{
    std::string* searchEntry{ new std::string(gtk_editable_get_text(GTK_EDITABLE(m_txtSearchMusicFiles))) };
    std::transform(searchEntry->begin(), searchEntry->end(), searchEntry->begin(), ::tolower);
    gtk_list_box_set_filter_func(GTK_LIST_BOX(m_listMusicFiles), [](GtkListBoxRow* row, gpointer data) -> int
    {
        std::string* searchEntry{ reinterpret_cast<std::string*>(data) };
        std::string rowFilename{ adw_preferences_row_get_title(ADW_PREFERENCES_ROW(row)) };
        std::transform(rowFilename.begin(), rowFilename.end(), rowFilename.begin(), ::tolower);
        bool found{ false };
        if(searchEntry->empty() || rowFilename.find(*searchEntry) != std::string::npos)
        {
            found = true;
        }
        return found;
    }, searchEntry, [](gpointer data)
    {
        std::string* searchEntry{ reinterpret_cast<std::string*>(data) };
        delete searchEntry;
    });
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
        gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), "00:00:00");
        gtk_editable_set_text(GTK_EDITABLE(m_txtChromaprintFingerprint), "");
        gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), "0 MB");
        adw_view_stack_set_visible_child(ADW_VIEW_STACK(m_stackAlbumArt), m_statusNoAlbumArt);
        gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
    }
    //One File Selected
    else if(m_controller.getSelectedMusicFiles().size() == 1)
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_controller.getSelectedMusicFiles()[0] };
        gtk_editable_set_text(GTK_EDITABLE(m_txtFilename), firstMusicFile->getFilename().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtTitle), firstMusicFile->getTitle().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtArtist), firstMusicFile->getArtist().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbum), firstMusicFile->getAlbum().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtYear), std::to_string(firstMusicFile->getYear()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtTrack), std::to_string(firstMusicFile->getTrack()).c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbumArtist), firstMusicFile->getAlbumArtist().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtGenre), firstMusicFile->getGenre().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtComment), firstMusicFile->getComment().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), firstMusicFile->getDurationAsString().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtChromaprintFingerprint), firstMusicFile->getChromaprintFingerprint().c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), firstMusicFile->getFileSizeAsString().c_str());
        if(!firstMusicFile->getAlbumArt().isEmpty())
        {
            adw_view_stack_set_visible_child(ADW_VIEW_STACK(m_stackAlbumArt), m_imgAlbumArt);
            GtkHelpers::gtk_image_set_from_byte_vector(GTK_IMAGE(m_imgAlbumArt), firstMusicFile->getAlbumArt());
        }
        else
        {
            adw_view_stack_set_visible_child(ADW_VIEW_STACK(m_stackAlbumArt), m_statusNoAlbumArt);
            gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
        }
    }
    //Multiple Files Selected
    else
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_controller.getSelectedMusicFiles()[0] };
        bool haveSameTitle{ true };
        bool haveSameArtist{ true };
        bool haveSameAlbum{ true };
        bool haveSameYear{ true };
        bool haveSameTrack{ true };
        bool haveSameAlbumArtist{ true };
        bool haveSameGenre{ true };
        bool haveSameComment{ true };
        bool haveSameAlbumArt{ true };
        int totalDuration{ 0 };
        std::uintmax_t totalFileSize{ 0 };
        for(const std::shared_ptr<MusicFile>& musicFile : m_controller.getSelectedMusicFiles())
        {
            if (firstMusicFile->getTitle() != musicFile->getTitle())
            {
                haveSameTitle = false;
            }
            if (firstMusicFile->getArtist() != musicFile->getArtist())
            {
                haveSameArtist = false;
            }
            if (firstMusicFile->getAlbum() != musicFile->getAlbum())
            {
                haveSameAlbum = false;
            }
            if (firstMusicFile->getYear() != musicFile->getYear())
            {
                haveSameYear = false;
            }
            if (firstMusicFile->getTrack() != musicFile->getTrack())
            {
                haveSameTrack = false;
            }
            if (firstMusicFile->getAlbumArtist() != musicFile->getAlbumArtist())
            {
                haveSameAlbumArtist = false;
            }
            if (firstMusicFile->getGenre() != musicFile->getGenre())
            {
                haveSameGenre = false;
            }
            if (firstMusicFile->getComment() != musicFile->getComment())
            {
                haveSameComment = false;
            }
            if  (firstMusicFile->getAlbumArt() != musicFile->getAlbumArt())
            {
                haveSameAlbumArt = false;
            }
            totalDuration += musicFile->getDuration();
            totalFileSize += musicFile->getFileSize();
        }
        gtk_editable_set_editable(GTK_EDITABLE(m_txtFilename), false);
        gtk_editable_set_text(GTK_EDITABLE(m_txtFilename), "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtTitle), haveSameTitle ? firstMusicFile->getTitle().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtArtist), haveSameArtist ? firstMusicFile->getArtist().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbum), haveSameAlbum ? firstMusicFile->getAlbum().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtYear), haveSameYear ? std::to_string(firstMusicFile->getYear()).c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtTrack), haveSameTrack ? std::to_string(firstMusicFile->getTrack()).c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtAlbumArtist), haveSameAlbumArtist ? firstMusicFile->getAlbumArtist().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtGenre), haveSameGenre ? firstMusicFile->getGenre().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtComment), haveSameComment ? firstMusicFile->getComment().c_str() : "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), MediaHelpers::durationToString(totalDuration).c_str());
        gtk_editable_set_text(GTK_EDITABLE(m_txtChromaprintFingerprint), "<keep>");
        gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), MediaHelpers::fileSizeToString(totalFileSize).c_str());
        if(haveSameAlbumArt && !firstMusicFile->getAlbumArt().isEmpty())
        {
            adw_view_stack_set_visible_child(ADW_VIEW_STACK(m_stackAlbumArt), m_imgAlbumArt);
            GtkHelpers::gtk_image_set_from_byte_vector(GTK_IMAGE(m_imgAlbumArt), firstMusicFile->getAlbumArt());
        }
        else
        {
            adw_view_stack_set_visible_child(ADW_VIEW_STACK(m_stackAlbumArt), m_statusNoAlbumArt);
            gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
        }
    }
}

bool MainWindow::onDrop(const GValue* value)
{
    void* file{ g_value_get_object(value) };
    std::string path{ g_file_get_path(G_FILE(file)) };
    if(std::filesystem::is_directory(path))
    {
        m_controller.openMusicFolder(path);
        return true;
    }
    return false;
}
