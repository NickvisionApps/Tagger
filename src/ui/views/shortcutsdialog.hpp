#pragma once

#include <string>
#include <adwaita.h>

namespace NickvisionTagger::UI::Views
{
    /**
     * A dialog for displaying keyboard shortcuts
     */
    class ShortcutsDialog
    {
    public:
	/**
	 * Constructs a ShortcutsDialog
	 *
	 * @param parent The parent window for the dialog
	 */
    	ShortcutsDialog(GtkWindow* parent);
    	/**
    	 * Destroys the ShortcutsDialog
    	 */
	~ShortcutsDialog();
	/**
    	 * Gets the GtkWidget* representing the ShortcutsDialog
    	 *
    	 * @returns The GtkWidget* representing the ShortcutsDialog
    	 */
	GtkWidget* gobj();
	/**
	 * Shows the ShortcutsDialog
	 */
	void show();

    private:
	std::string m_xml;
	GtkWidget* m_gobj;
    };
}