#include "mainwindow.hpp"
#include <algorithm>
#include <filesystem>
#include <regex>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"
#include "../controls/comboboxdialog.hpp"
#include "../controls/entrydialog.hpp"
#include "../controls/messagedialog.hpp"
#include "../controls/progressdialog.hpp"
#include "../../helpers/mediahelpers.hpp"
#include "../../models/musicfolder.hpp"
#include "../../models/tagmap.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI::Controls;
using namespace NickvisionTagger::UI::Views;

/**
 * Sets a GtkImage's source from the TagLib::ByteVector
 *
 * @param image The GtkImage
 * @param byteVector The TagLib::ByteVector representing the image
 */
void gtk_image_set_from_byte_vector(GtkImage* image, const TagLib::ByteVector& byteVector)
{
    if(byteVector.isEmpty())
    {
        gtk_image_clear(image);
    }
    else
    {
        GdkPixbufLoader* pixbufLoader{gdk_pixbuf_loader_new()};
        gdk_pixbuf_loader_write(pixbufLoader, (unsigned char*)byteVector.data(), byteVector.size(), nullptr);
        gtk_image_set_from_pixbuf(image, gdk_pixbuf_loader_get_pixbuf(pixbufLoader));
        gdk_pixbuf_loader_close(pixbufLoader, nullptr);
        g_object_unref(pixbufLoader);
    }
}

MainWindow::MainWindow(GtkApplication* application, const MainWindowController& controller) : m_controller{ controller }, m_gobj{ adw_application_window_new(application) }
{
    //Window Settings
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 900, 700);
    g_signal_connect(m_gobj, "close_request", G_CALLBACK((void (*)(GtkWidget*, gpointer))[](GtkWidget*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onCloseRequest(); }), this);
    //gtk_style_context_add_class(gtk_widget_get_style_context(m_gobj), "devel");
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
    gtk_widget_set_tooltip_text(m_btnApply, "Apply Changes To Tag (Ctrl+S)");
    gtk_widget_set_visible(m_btnApply, false);
    gtk_actionable_set_action_name(GTK_ACTIONABLE(m_btnApply), "win.apply");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnApply), "suggested-action");
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnApply);
    //Menu Tag Actions Button
    m_btnMenuTagActions = gtk_menu_button_new();
    GMenu* menuTagActions{ g_menu_new() };
    GMenu* menuAlbumArt{ g_menu_new() };
    GMenu* menuOtherActions{ g_menu_new() };
    g_menu_append(menuAlbumArt, "Insert Album Art", "win.insertAlbumArt");
    g_menu_append(menuAlbumArt, "Remove Album Art", "win.removeAlbumArt");
    g_menu_append(menuOtherActions, "Convert Filename to Tag", "win.filenameToTag");
    g_menu_append(menuOtherActions, "Convert Tag to Filename", "win.tagToFilename");
    g_menu_append(menuTagActions, "Delete Tags", "win.deleteTags");
    g_menu_append_section(menuTagActions, nullptr, G_MENU_MODEL(menuAlbumArt));
    g_menu_append_section(menuTagActions, nullptr, G_MENU_MODEL(menuOtherActions));
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuTagActions), "document-properties-symbolic");
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuTagActions), G_MENU_MODEL(menuTagActions));
    gtk_widget_set_tooltip_text(m_btnMenuTagActions, "Tag Actions");
    gtk_widget_set_visible(m_btnMenuTagActions, false);
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuTagActions);
    g_object_unref(menuAlbumArt);
    g_object_unref(menuOtherActions);
    g_object_unref(menuTagActions);
    //Menu Web Services Button
    m_btnMenuWebServices = gtk_menu_button_new();
    GMenu* menuWebServices{ g_menu_new() };
    g_menu_append(menuWebServices, "Download MusicBrainz Metadata", "win.downloadMusicBrainzMetadata");
    g_menu_append(menuWebServices, "Submit to AcoustId", "win.submitToAcoustId");
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuWebServices), "web-browser-symbolic");
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuWebServices), G_MENU_MODEL(menuWebServices));
    gtk_widget_set_tooltip_text(m_btnMenuWebServices, "Web Services");
    gtk_widget_set_visible(m_btnMenuWebServices, false);
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuWebServices);
    g_object_unref(menuWebServices);
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
    //Album Art Stack
    m_stackAlbumArt = adw_view_stack_new();
    gtk_widget_set_halign(m_stackAlbumArt, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_stackAlbumArt, 280, 280);
    //No Album Art
    m_btnNoAlbumArt = gtk_button_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnNoAlbumArt), "card");
    g_signal_connect(m_btnNoAlbumArt, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onInsertAlbumArt(); }), this);
    m_statusNoAlbumArt = adw_status_page_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_statusNoAlbumArt), "compact");
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_statusNoAlbumArt), "image-missing-symbolic");
    gtk_button_set_child(GTK_BUTTON(m_btnNoAlbumArt), m_statusNoAlbumArt);
    adw_view_stack_add_named(ADW_VIEW_STACK(m_stackAlbumArt), m_btnNoAlbumArt, "noImage");
    //Album Art
    m_btnAlbumArt = gtk_button_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnAlbumArt), "card");
    g_signal_connect(m_btnAlbumArt, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onInsertAlbumArt(); }), this);
    m_frmAlbumArt = gtk_frame_new(nullptr);
    m_imgAlbumArt = gtk_image_new();
    gtk_frame_set_child(GTK_FRAME(m_frmAlbumArt), m_imgAlbumArt);
    gtk_button_set_child(GTK_BUTTON(m_btnAlbumArt), m_frmAlbumArt);
    adw_view_stack_add_named(ADW_VIEW_STACK(m_stackAlbumArt), m_btnAlbumArt, "image");
    //Keep Album Art
    m_btnKeepAlbumArt = gtk_button_new();
    gtk_widget_set_tooltip_text(m_btnKeepAlbumArt, "Selected files have different album art images.");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnKeepAlbumArt), "card");
    g_signal_connect(m_btnKeepAlbumArt, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onInsertAlbumArt(); }), this);
    m_statusKeepAlbumArt = adw_status_page_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_statusKeepAlbumArt), "compact");
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_statusKeepAlbumArt), "folder-music-symbolic");
    gtk_button_set_child(GTK_BUTTON(m_btnKeepAlbumArt), m_statusKeepAlbumArt);
    adw_view_stack_add_named(ADW_VIEW_STACK(m_stackAlbumArt), m_btnKeepAlbumArt, "keepImage");
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
    gtk_widget_set_margin_start(m_boxTaggerFlap, 80);
    gtk_widget_set_margin_top(m_boxTaggerFlap, 20);
    gtk_widget_set_margin_end(m_boxTaggerFlap, 80);
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
    //Download MusicBrainz Metadata
    m_actDownloadMusicBrainzMetadata = g_simple_action_new("downloadMusicBrainzMetadata", nullptr);
    g_signal_connect(m_actDownloadMusicBrainzMetadata, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onDownloadMusicBrainzMetadata(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actDownloadMusicBrainzMetadata));
    gtk_application_set_accels_for_action(application, "win.downloadMusicBrainzMetadata", new const char*[2]{ "<Ctrl>m", nullptr });
    //Submit to AcoustId
    m_actSubmitToAcoustId = g_simple_action_new("submitToAcoustId", nullptr);
    g_signal_connect(m_actSubmitToAcoustId, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onSubmitToAcoustId(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actSubmitToAcoustId));
    gtk_application_set_accels_for_action(application, "win.submitToAcoustId", new const char*[2]{ "<Ctrl>u", nullptr });
    //Preferences Action
    m_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onPreferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actPreferences));
    gtk_application_set_accels_for_action(application, "win.preferences", new const char*[2]{ "<Ctrl>comma", nullptr });
    //Keyboard Shortcuts Action
    m_actKeyboardShortcuts = g_simple_action_new("keyboardShortcuts", nullptr);
    g_signal_connect(m_actKeyboardShortcuts, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onKeyboardShortcuts(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actKeyboardShortcuts));
    gtk_application_set_accels_for_action(application, "win.keyboardShortcuts", new const char*[2]{ "<Ctrl>question", nullptr });
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
    std::size_t musicFilesCount{ m_controller.getMusicFiles().size() };
    adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), musicFilesCount > 0 ? "pageTagger" : "pageNoFiles");
    for(const std::shared_ptr<MusicFile>& musicFile : m_controller.getMusicFiles())
    {
        GtkWidget* row{ adw_action_row_new() };
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), std::regex_replace(musicFile->getFilename(), std::regex("\\&"), "&amp;").c_str());
        gtk_list_box_append(GTK_LIST_BOX(m_listMusicFiles), row);
        m_listMusicFilesRows.push_back(row);
        g_main_context_iteration(g_main_context_default(), false);
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
    TagMap tagMap;
    tagMap.setFilename(gtk_editable_get_text(GTK_EDITABLE(m_txtFilename)));
    tagMap.setTitle(gtk_editable_get_text(GTK_EDITABLE(m_txtTitle)));
    tagMap.setArtist(gtk_editable_get_text(GTK_EDITABLE(m_txtArtist)));
    tagMap.setAlbum(gtk_editable_get_text(GTK_EDITABLE(m_txtAlbum)));
    tagMap.setYear(gtk_editable_get_text(GTK_EDITABLE(m_txtYear)));
    tagMap.setTrack(gtk_editable_get_text(GTK_EDITABLE(m_txtTrack)));
    tagMap.setAlbumArtist(gtk_editable_get_text(GTK_EDITABLE(m_txtAlbumArtist)));
    tagMap.setGenre(gtk_editable_get_text(GTK_EDITABLE(m_txtGenre)));
    tagMap.setComment(gtk_editable_get_text(GTK_EDITABLE(m_txtComment)));
    ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Saving tags...", [&, tagMap]() { m_controller.saveTags(tagMap); } };
    progressDialog.run();
    onMusicFolderUpdated(false);
}

void MainWindow::onDeleteTags()
{
    MessageDialog messageDialog{ GTK_WINDOW(m_gobj), "Delete Tags?", "Are you sure you want to delete the tags of the selected files?", "No", "Yes" };
    if(messageDialog.run() == MessageDialogResponse::Destructive)
    {
        ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Deleting tags...", [&]() { m_controller.deleteTags(); } };
        progressDialog.run();
        gtk_list_box_unselect_all(GTK_LIST_BOX(m_listMusicFiles));
    }
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
            mainWindow->onListMusicFilesSelectionChanged();
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openPictureDialog));
}

void MainWindow::onRemoveAlbumArt()
{
    ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Removing album art...", [&]() { m_controller.removeAlbumArt(); } };
    progressDialog.run();
    onListMusicFilesSelectionChanged();
}

void MainWindow::onFilenameToTag()
{
    ComboBoxDialog formatStringDialog{ GTK_WINDOW(m_gobj), "Filename to Tag", "Please select a format string.", "Format String", { "%artist%- %title%", "%title%- %artist%", "%track%- %title%", "%title%" } };
    std::string formatString = formatStringDialog.run();
    if(!formatString.empty())
    {
        ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Converting filenames to tags...", [&, formatString]() { m_controller.filenameToTag(formatString); } };
        progressDialog.run();
        onListMusicFilesSelectionChanged();
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
        onMusicFolderUpdated(false);
    }
}

void MainWindow::onDownloadMusicBrainzMetadata()
{
    ProgressDialog progressDialog{ GTK_WINDOW(m_gobj), "Downloading MusicBrainz metadata...\n<small>(This may take a while)</small>", [&]() { m_controller.downloadMusicBrainzMetadata(); } };
    progressDialog.run();
    onListMusicFilesSelectionChanged();
}

void MainWindow::onSubmitToAcoustId()
{
    //Check for one file selected
    if(m_controller.getSelectedMusicFiles().size() > 1)
    {
        MessageDialog messageDialog{ GTK_WINDOW(m_gobj), "Too Many Files Selected", "Only one file can be submitted to AcoustId at a time. Please select only one file and try again.", "OK" };
        messageDialog.run();
        return;
    }
    //Check for valid AcoustId User API Key
    bool validAcoustIdUserAPIKey{ false };
    ProgressDialog progressDialogChecking{ GTK_WINDOW(m_gobj), "Checking AcoustId user api key...", [&]() { validAcoustIdUserAPIKey = m_controller.checkIfAcoustIdUserAPIKeyValid(); } };
    progressDialogChecking.run();
    if(!validAcoustIdUserAPIKey)
    {
        MessageDialog messageDialog{ GTK_WINDOW(m_gobj), "Invalid User API Key", "The AcoustId User API Key is invalid.\nPlease provide a valid api key in Preferences.", "OK" };
        messageDialog.run();
        return;
    }
    //Get MusicBrainz Recording Id
    EntryDialog entryDialog{ GTK_WINDOW(m_gobj), "Submit to AcoustId", "AcoustId can associate a song's fingerprint with a MusicBrainz Recording Id for easy identification.\n\nIf you have a MusicBrainz Recording Id for this song, please provide it below.\n\nIf no id is provided, Tagger will submit your tag's metadata in association with the fingerprint instead.", "MusicBrainz Recording Id" };
    std::string result{ entryDialog.run() };
    ProgressDialog progressDialogSubmitting{ GTK_WINDOW(m_gobj), "Submitting metadata to AcoustId...", [&, result]() { m_controller.submitToAcoustId(result); } };
    progressDialogSubmitting.run();
    onListMusicFilesSelectionChanged();
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
    bool isDev{ m_controller.getAppInfo().getVersion().find("-") != std::string::npos };
    adw_show_about_window(GTK_WINDOW(m_gobj),
                          "application-name", m_controller.getAppInfo().getShortName().c_str(),
                          "application-icon", (m_controller.getAppInfo().getId() + (isDev ? "-devel" : "")).c_str(),
                          "version", m_controller.getAppInfo().getVersion().c_str(),
                          "comments", m_controller.getAppInfo().getDescription().c_str(),
                          "developer-name", "Nickvision",
                          "license-type", GTK_LICENSE_GPL_3_0,
                          "copyright", "(C) Nickvision 2021-2022",
                          "website", m_controller.getAppInfo().getGitHubRepo().c_str(),
                          "issue-url", m_controller.getAppInfo().getIssueTracker().c_str(),
                          "support-url", m_controller.getAppInfo().getSupportUrl().c_str(),
                          "developers", new const char*[3]{ "Nicholas Logozzo https://github.com/nlogozzo", "Contributors on GitHub ❤️ https://github.com/nlogozzo/NickvisionTagger/graphs/contributors", nullptr },
                          "designers", new const char*[2]{ "Nicholas Logozzo https://github.com/nlogozzo", nullptr },
                          "artists", new const char*[3]{ "David Lapshin https://github.com/daudix-UFO", "noëlle https://github.com/jannuary", nullptr },
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
    gtk_widget_set_visible(m_btnApply, true);
    gtk_widget_set_visible(m_btnMenuTagActions, true);
    gtk_widget_set_visible(m_btnMenuWebServices, true);
    gtk_editable_set_text(GTK_EDITABLE(m_txtSearchMusicFiles), "");
    adw_flap_set_reveal_flap(ADW_FLAP(m_pageFlapTagger), true);
    gtk_editable_set_editable(GTK_EDITABLE(m_txtFilename), true);
    if(selectedIndexes.size() == 0)
    {
        gtk_widget_set_visible(m_btnApply, false);
        gtk_widget_set_visible(m_btnMenuTagActions, false);
        gtk_widget_set_visible(m_btnMenuWebServices, false);
        adw_flap_set_reveal_flap(ADW_FLAP(m_pageFlapTagger), false);
    }
    else if(selectedIndexes.size() > 1)
    {
        gtk_editable_set_editable(GTK_EDITABLE(m_txtFilename), false);
    }
    TagMap tagMap{ m_controller.getSelectedTagMap() };
    gtk_editable_set_text(GTK_EDITABLE(m_txtFilename), tagMap.getFilename().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtTitle), tagMap.getTitle().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtArtist), tagMap.getArtist().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtAlbum), tagMap.getAlbum().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtYear), tagMap.getYear().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtTrack), tagMap.getTrack().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtAlbumArtist), tagMap.getAlbumArtist().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtGenre), tagMap.getGenre().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtComment), tagMap.getComment().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtDuration), tagMap.getDuration().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtChromaprintFingerprint), tagMap.getFingerprint().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_txtFileSize), tagMap.getFileSize().c_str());
    if(tagMap.getAlbumArt() == "hasArt")
    {
        adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_stackAlbumArt), "image");
        gtk_image_set_from_byte_vector(GTK_IMAGE(m_imgAlbumArt), m_controller.getSelectedMusicFiles()[0]->getAlbumArt());
    }
    else if(tagMap.getAlbumArt() == "keepArt")
    {
        adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_stackAlbumArt), "keepImage");
        gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
    }
    else
    {
        adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_stackAlbumArt), "noImage");
        gtk_image_clear(GTK_IMAGE(m_imgAlbumArt));
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
