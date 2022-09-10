#pragma once

#include <memory>
#include <string>
#include <adwaita.h>
#include "views/mainwindow.hpp"
#include "../models/appinfo.hpp"
#include "../models/configuration.hpp"

namespace NickvisionTagger::UI
{
    /**
     * A gtk application implementation
     */
    class Application
    {
    public:
    	/**
    	 * Constructs an Application
    	 *
    	 * @param id The application id
    	 * @param flags GApplicationFlags
    	 */
    	Application(const std::string& id, GApplicationFlags flags = G_APPLICATION_FLAGS_NONE);
    	/**
    	 * Runs the application
    	 *
    	 * @param argc The number of arguments
    	 * @param argv The array of arguments
    	 *
    	 * @returns Exit code from g_application_run
    	 */
    	int run(int argc, char* argv[]);

    private:
    	AdwApplication* m_adwApp{ nullptr };
    	NickvisionTagger::Models::AppInfo m_appInfo;
    	NickvisionTagger::Models::Configuration m_configuration;
    	std::shared_ptr<Views::MainWindow> m_mainWindow;
    	/**
    	 * Loads the application and shows the MainWindow
    	 *
    	 * @param app GtkApplication*
    	 */
    	void onActivate(GtkApplication* app);
    };
}