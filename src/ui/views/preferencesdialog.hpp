#pragma once

#include <adwaita.h>
#include "../../controllers/preferencesdialogcontroller.hpp"

namespace NickvisionTagger::UI::Views
{
    /**
     * A dialog for managing appplication preferences
     */
    class PreferencesDialog
    {
    public:
    	/**
    	 * Constructs a PreferencesDialog
    	 *
    	 * @param parent The parent window for the dialog
    	 * @param controller PreferencesDialogController
    	 */
    	PreferencesDialog(GtkWindow* parent, const NickvisionTagger::Controllers::PreferencesDialogController& controller);
    	/**
    	 * Destroys the PreferencesDialog
    	 */
    	~PreferencesDialog();
    	/**
    	 * Gets the GtkWidget* representing the PreferencesDialog
    	 *
    	 * @returns The GtkWidget* representing the PreferencesDialog
    	 */
    	GtkWidget* gobj();
    	/**
    	 * Runs the PreferencesDialog
    	 */
    	void run();

    private:
    	NickvisionTagger::Controllers::PreferencesDialogController m_controller;
		GtkWidget* m_gobj{ nullptr };
		GtkWidget* m_mainBox{ nullptr };
		GtkWidget* m_headerBar{ nullptr };
		GtkWidget* m_page{ nullptr };
		GtkWidget* m_grpUserInterface{ nullptr };
		GtkWidget* m_rowTheme{ nullptr };
		GtkWidget* m_grpMusicFolder{ nullptr };
		GtkWidget* m_rowIncludeSubfolders{ nullptr };
		GtkWidget* m_switchIncludeSubfolders{ nullptr };
		GtkWidget* m_rowRememberLastOpenedFolder{ nullptr };
		GtkWidget* m_switchRememberLastOpenedFolder{ nullptr };
		GtkWidget* m_grpMusicFile{ nullptr };
		GtkWidget* m_rowPreserveModificationTimeStamp{ nullptr };
		GtkWidget* m_switchPreserveModificationTimeStamp{ nullptr };
		GtkWidget* m_rowOverwriteTagWithMusicBrainz{ nullptr };
		GtkWidget* m_switchOverwriteTagWithMusicBrainz{ nullptr };
    };
}