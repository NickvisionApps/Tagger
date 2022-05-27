#include "mainwindow.h"
#include "../controls/progressdialog.h"
#include "../controls/progresstracker.h"
#include "preferencesdialog.h"

using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;
using namespace NickvisionTagger::UI::Controls;
using namespace NickvisionTagger::UI::Views;
using namespace NickvisionTagger::Update;

MainWindow::MainWindow(Configuration& configuration) : Widget{"/ui/views/mainwindow.xml", "adw_winMain"}, m_configuration{configuration}, m_updater{"https://raw.githubusercontent.com/nlogozzo/NickvisionTagger/main/UpdateConfig.json", { "2022.5.0" }}, m_opened{false}
{
    //==Signals==//
    g_signal_connect(m_gobj, "show", G_CALLBACK((void (*)(GtkWidget*, gpointer*))[](GtkWidget* widget, gpointer* data) { reinterpret_cast<MainWindow*>(data)->onStartup(); }), this);
    //==Open Folder==//
    g_signal_connect(gtk_builder_get_object(m_builder, "gtk_btnOpenFolder"), "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton* button, gpointer* data) { reinterpret_cast<MainWindow*>(data)->openFolder(); }), this);
    //==Close Folder==//
    g_signal_connect(gtk_builder_get_object(m_builder, "gtk_btnCloseFolder"), "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer*))[](GtkButton* button, gpointer* data) { reinterpret_cast<MainWindow*>(data)->closeFolder(); }), this);
    //==Help Actions==//
    //Check for Updates
    m_gio_actUpdate = g_simple_action_new("update", nullptr);
    g_signal_connect(m_gio_actUpdate, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->update(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actUpdate));
    //GitHub Repo
    m_gio_actGitHubRepo = g_simple_action_new("gitHubRepo", nullptr);
    g_signal_connect(m_gio_actGitHubRepo, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->gitHubRepo(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actGitHubRepo));
    //Report a Bug
    m_gio_actReportABug = g_simple_action_new("reportABug", nullptr);
    g_signal_connect(m_gio_actReportABug, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->reportABug(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actReportABug));
    //Settings
    m_gio_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_gio_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->preferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actPreferences));
    //Changelog
    m_gio_actChangelog = g_simple_action_new("changelog", nullptr);
    g_signal_connect(m_gio_actChangelog, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->changelog(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actChangelog));
    //About
    m_gio_actAbout = g_simple_action_new("about", nullptr);
    g_signal_connect(m_gio_actAbout, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer*))[](GSimpleAction* action, GVariant* parameter, gpointer* data) { reinterpret_cast<MainWindow*>(data)->about(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_gio_actAbout));
    //==Help Menu Button==//
    GtkBuilder* builderMenu{gtk_builder_new_from_resource("/ui/views/menuhelp.xml")};
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(gtk_builder_get_object(m_builder, "gtk_btnMenuHelp")), G_MENU_MODEL(gtk_builder_get_object(builderMenu, "gio_menuHelp")));
    g_object_unref(builderMenu);
}

MainWindow::~MainWindow()
{
    m_configuration.save();
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
        //==Load Configuration==//
        
        //==Check for Updates==//
        ProgressTracker* progTrackerUpdate{new ProgressTracker("Checking for updates...", [&]() { m_updater.checkForUpdates(); }, [&]()
        {
            if(m_updater.getUpdateAvailable())
            {
                sendToast("A new update is avaliable.");
            }
        })};
        adw_header_bar_pack_end(ADW_HEADER_BAR(gtk_builder_get_object(m_builder, "adw_headerBar")), progTrackerUpdate->gobj());
        progTrackerUpdate->show();
        m_opened = true;
    }
}

void MainWindow::openFolder()
{
    GtkWidget* openFolderDialog {gtk_file_chooser_dialog_new("Open Folder", GTK_WINDOW(gtk_widget_get_root(gobj())), 
        GTK_FILE_CHOOSER_ACTION_SELECT_FOLDER, "_Cancel", GTK_RESPONSE_CANCEL, "_Select", GTK_RESPONSE_ACCEPT, nullptr)};
    gtk_window_set_modal(GTK_WINDOW(openFolderDialog), true);
    g_signal_connect(openFolderDialog, "response", G_CALLBACK((void (*)(GtkDialog*, gint, gpointer*))([](GtkDialog* dialog, gint response_id, gpointer* data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
            GtkFileChooser* chooser{GTK_FILE_CHOOSER(dialog)};
            GFile* file{gtk_file_chooser_get_file(chooser)};
            std::string path{g_file_get_path(file)};
            g_object_unref(file);
            adw_window_title_set_subtitle(ADW_WINDOW_TITLE(gtk_builder_get_object(GTK_BUILDER(mainWindow->m_builder), "adw_title")), path.c_str());
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(mainWindow->m_builder, "gtk_btnCloseFolder")), true);
        }
        gtk_window_destroy(GTK_WINDOW(dialog));
    })), this);
    gtk_widget_show(openFolderDialog);
}

void MainWindow::closeFolder()
{
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(gtk_builder_get_object(GTK_BUILDER(m_builder), "adw_title")), nullptr);
    gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "gtk_btnCloseFolder")), false);
}

void MainWindow::update()
{
    if(m_updater.getUpdateAvailable())
    {
        GtkWidget* updateDialog{gtk_message_dialog_new(GTK_WINDOW(m_gobj), GtkDialogFlags(GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_MODAL),
            GTK_MESSAGE_INFO, GTK_BUTTONS_YES_NO, "Update Available")};
        gtk_message_dialog_format_secondary_text(GTK_MESSAGE_DIALOG(updateDialog), std::string("\n===V" + m_updater.getLatestVersion().toString() + " Changelog===\n" + m_updater.getChangelog() + "\n\nTagger can automatically download the update tar.gz file to your Downloads directory. Would you like to continue?").c_str());
        g_signal_connect(updateDialog, "response", G_CALLBACK((void (*)(GtkDialog*, gint, gpointer*))([](GtkDialog* dialog, gint response_id, gpointer* data)
        {
            gtk_window_destroy(GTK_WINDOW(dialog));
            if(response_id == GTK_RESPONSE_YES)
            {
                MainWindow* mainWindow{reinterpret_cast<MainWindow*>(data)};
                ProgressTracker* proTrackerDownloading{new ProgressTracker("Downloading the update...", [&]() { mainWindow->m_updater.update(); }, [&]()
                {
                    if(mainWindow->m_updater.getUpdateSuccessful())
                    {
                        mainWindow->sendToast("Update downloaded successfully. Please visit your Downloads folder to upack and run the new update.");
                    }
                    else
                    {
                        mainWindow->sendToast("Error: Unable to download the update.");
                    }
                })};
                adw_header_bar_pack_end(ADW_HEADER_BAR(gtk_builder_get_object(mainWindow->m_builder, "adw_headerBar")), proTrackerDownloading->gobj());
                proTrackerDownloading->show();
            }
        })), this);
        gtk_widget_show(updateDialog);
    }
    else
    {
        sendToast("There is no update at this time. Please try again later.");
    }
}

void MainWindow::gitHubRepo()
{
    g_app_info_launch_default_for_uri("https://github.com/nlogozzo/NickvisionTagger", nullptr, nullptr);
}

void MainWindow::reportABug()
{
    g_app_info_launch_default_for_uri("https://github.com/nlogozzo/NickvisionTagger/issues/new", nullptr, nullptr);
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
        delete pointers;
    })), pointers);
    preferencesDialog->show();
}

void MainWindow::changelog()
{
    GtkWidget* changelogDialog{gtk_message_dialog_new(GTK_WINDOW(m_gobj), GtkDialogFlags(GTK_DIALOG_DESTROY_WITH_PARENT | GTK_DIALOG_MODAL),
        GTK_MESSAGE_INFO, GTK_BUTTONS_OK, "What's New?")};
    gtk_message_dialog_format_secondary_text(GTK_MESSAGE_DIALOG(changelogDialog), "- Initial Release");
    g_signal_connect(changelogDialog, "response", G_CALLBACK(gtk_window_destroy), nullptr);
    gtk_widget_show(changelogDialog);
}

void MainWindow::about()
{
    const char* authors[]{ "Nicholas Logozzo", nullptr };
    gtk_show_about_dialog(GTK_WINDOW(m_gobj), "program-name", "Nickvision Tagger", "version", "2022.5.0", "comments", "An easy-to-use music tag (metadata) editor.",
                          "copyright", "(C) Nickvision 2021-2022", "license-type", GTK_LICENSE_GPL_3_0, "website", "https://github.com/nlogozzo", "website-label", "GitHub",
                          "authors", authors, nullptr);
}

void MainWindow::sendToast(const std::string& message)
{
    AdwToast* toast{adw_toast_new(message.c_str())};
    adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(gtk_builder_get_object(m_builder, "adw_toastOverlay")), toast);
}
