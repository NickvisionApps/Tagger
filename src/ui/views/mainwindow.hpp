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
	GtkWidget* m_btnOpenMusicFolder{ nullptr };
	GtkWidget* m_btnReloadMusicFolder{ nullptr };
	GtkWidget* m_btnMenuHelp{ nullptr };
	GtkWidget* m_sepHeaderEnd{ nullptr };
	GtkWidget* m_btnApply{ nullptr };
	GtkWidget* m_toastOverlay{ nullptr };
	GtkWidget* m_viewStack{ nullptr };
	GtkWidget* m_pageStatusNoFiles{ nullptr };
	GtkWidget* m_pageFlapTagger{ nullptr };
	GSimpleAction* m_actOpenMusicFolder{ nullptr };
	GSimpleAction* m_actReloadMusicFolder{ nullptr };
	GSimpleAction* m_actApply{ nullptr };
	GSimpleAction* m_actPreferences{ nullptr };
	GSimpleAction* m_actKeyboardShortcuts{ nullptr };
	GSimpleAction* m_actAbout{ nullptr };
	/**
	 * Runs startup functions
	 */
	void onStartup();
	/**
    	 * Updates the UI when the music folder is updated
    	 */
    	void onMusicFolderUpdated();
    	/**
    	 * Prompts the user to open a music folder from disk and load it in the app
    	 */
    	void onOpenMusicFolder();
    	/**
    	 * Applys the changes to the selected music file's tag
    	 */
    	void onApply();
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