#include "application.hpp"
#include "../controllers/mainwindowcontroller.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;
using namespace NickvisionTagger::UI::Views;

Application::Application(const std::string& id, GApplicationFlags flags) : m_adwApp{ adw_application_new(id.c_str(), flags) }
{
    //AppInfo
    m_appInfo.setId(id);
    m_appInfo.setName("NickvisionTagger");
    m_appInfo.setShortName("Tagger");
    m_appInfo.setDescription("An easy-to-use music tag (metadata) editor.");
    m_appInfo.setVersion("2022.10.0-next");
    m_appInfo.setChangelog("<ul><li>Tagger now requires clicking the 'Apply' button to save changes to selected tags after inserting/removing album art, converting filenames to tags, or downloading metadata from MusicBrainz. Changing a file selection without clicking 'Apply' will preserve the changes to the tags, however, if the music folder is changed/reloaded or if the application is closed without clicking 'Apply', the changes will be lost. Deleting Tags continues to be a permanent action that will take affect as soon as the action is confirmed from the message box</li><li>Added the 'Overwrite Tag With MusicBrainz' option to preferences</li><li>Added the ability to click on the album art in the tag properties pane to insert album art</li><li>Fixed an issue where 'Tag to Filename' was not updating the music files list after completing</li></ul>");
    m_appInfo.setGitHubRepo("https://github.com/nlogozzo/NickvisionTagger");
    m_appInfo.setIssueTracker("https://github.com/nlogozzo/NickvisionTagger/issues/new");
    m_appInfo.setSupportUrl("https://github.com/nlogozzo/NickvisionTagger/discussions");
    //Signals
    g_signal_connect(m_adwApp, "activate", G_CALLBACK((void (*)(GtkApplication*, gpointer))[](GtkApplication* app, gpointer data) { reinterpret_cast<Application*>(data)->onActivate(app); }), this);
}

int Application::run(int argc, char* argv[])
{
    return g_application_run(G_APPLICATION(m_adwApp), argc, argv);
}

void Application::onActivate(GtkApplication* app)
{
    if(m_configuration.getTheme() == Theme::System)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_PREFER_LIGHT);
    }
    else if(m_configuration.getTheme() == Theme::Light)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
    }
    else if(m_configuration.getTheme() == Theme::Dark)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
    }
    m_mainWindow = std::make_shared<MainWindow>(app, MainWindowController(m_appInfo, m_configuration));
    gtk_application_add_window(app, GTK_WINDOW(m_mainWindow->gobj()));
    m_mainWindow->start();
}

