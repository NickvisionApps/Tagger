#pragma once

#include <adwaita.h>
#include "../../controllers/mainwindowcontroller.hpp"

namespace NickvisionTagger::UI::Views
{
    /**
     * The MainWindow for the application
     */
    class MainWindow
    {
    public:
    	/**
    	 * Constructs a MainWindow
    	 *
    	 * @param application GtkApplication*
    	 * @param appInfo The AppInfo for the application
    	 */
    	MainWindow(GtkApplication* application, const NickvisionTagger::Controllers::MainWindowController& controller);
    	/**
    	 * Gets the GtkWidget* representing the MainWindow
    	 *
    	 * @returns The GtkWidget* representing the MainWindow 
    	 */
    	GtkWidget* gobj();
    	/**
    	 * Shows the MainWindow
    	 */
    	void show();
    	
    private:
    	NickvisionTagger::Controllers::MainWindowController m_controller;
	GtkWidget* m_gobj{ nullptr };
	GtkWidget* m_mainBox{ nullptr };
	GtkWidget* m_headerBar{ nullptr };
	GtkWidget* m_adwTitle{ nullptr };
	GtkWidget* m_btnOpenFolder{ nullptr };
	GtkWidget* m_btnCloseFolder{ nullptr };
	GtkWidget* m_btnMenuHelp{ nullptr };
	GtkWidget* m_toastOverlay{ nullptr };
	GSimpleAction* m_actOpenFolder{ nullptr };
	GSimpleAction* m_actCloseFolder{ nullptr };
	GSimpleAction* m_actPreferences{ nullptr };
	GSimpleAction* m_actKeyboardShortcuts{ nullptr };
	GSimpleAction* m_actAbout{ nullptr };
	/**
	 * Runs startup functions
	 */
	void onStartup();
	/**
    	 * Updates the UI with the current folder
    	 */
    	void onFolderChanged();
    	/**
    	 * Prompts the user to open a folder from disk and load it in the app
    	 */
    	void onOpenFolder();
    	/**
    	 * Displays the preferences dialog
    	 */
    	void onPreferences();
    	/**
    	 * Displays the keyboard shortcuts dialog
    	 */
    	void onKeyboardShortcuts();
    	/**
    	 * Displays the about dialog
    	 */
    	void onAbout();
    };
}