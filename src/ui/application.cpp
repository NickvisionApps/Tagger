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
    m_appInfo.setVersion("2022.10.3");
    m_appInfo.setChangelog("<ul><li>Tagger will now notify the user of changes waiting to be applied to a file</li><li>The 'Delete Tag' action must now be applied to be saved to the file</li><li>The 'Tag to Filename' action must now be applied to change the filename on disk</li><li>Fixed an issue where the 'Apply' action would clear the file selection</li><li>Added the ability to right-click the music files list when files are selected to access a tag actions context menu</li></ul>");
    m_appInfo.setGitHubRepo("https://github.com/nlogozzo/NickvisionTagger");
    m_appInfo.setIssueTracker("https://github.com/nlogozzo/NickvisionTagger/issues/new");
    m_appInfo.setSupportUrl("https://github.com/nlogozzo/NickvisionTagger/discussions");
    m_appInfo.setAcoustIdClientAPIKey("Lz9ENGSGsX");
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


