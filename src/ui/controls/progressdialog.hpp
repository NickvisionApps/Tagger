#pragma once

#include <functional>
#include <string>
#include <adwaita.h>

namespace NickvisionTagger::UI::Controls
{
    /**
     * A dialog for managing a long task
     */
    class ProgressDialog
    {
    public:
        /**
         * Constructs a ProgressDialog
         *
         * @param parent The parent window for the dialog
         * @param description The description of the long task
         * @param work The long task to preform
         */
    	ProgressDialog(GtkWindow* parent, const std::string& description, const std::function<void()>& work);
    	/**
    	 * Destroys the ProgressDialog
    	 */
    	~ProgressDialog();
    	/**
    	 * Gets the GtkWidget* representing the ProgressDialog
    	 *
    	 * @returns The GtkWidget* representing the ProgressDialog
    	 */
    	GtkWidget* gobj();
    	/**
    	 * Runs the ProgressDialog
    	 */
    	void run();

    private:
    	std::function<void()> m_work;
		GtkWidget* m_gobj{ nullptr };
		GtkWidget* m_mainBox{ nullptr };
		GtkWidget* m_lblDescription{ nullptr };
		GtkWidget* m_progBar{ nullptr };
    };
}