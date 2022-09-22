#pragma once

#include <functional>
#include <mutex>
#include <string>
#include <thread>
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
         * @param then A callback to call after the work has completed
         */
    	ProgressDialog(GtkWindow* parent, const std::string& description, const std::function<void()>& work, const std::function<void()>& then = []() {});
    	/**
    	 * Destroys the ProgressDialog
    	 */
    	~ProgressDialog();
    	/**
    	 * Starts the ProgressDialog
    	 */
    	void start();

    private:
    	std::mutex m_mutex;
    	std::function<void()> m_work;
    	std::function<void()> m_then;
    	bool m_isFinished;
    	std::jthread m_thread;
	GtkWidget* m_gobj{ nullptr };
	GtkWidget* m_mainBox{ nullptr };
	GtkWidget* m_lblDescription{ nullptr };
	GtkWidget* m_progBar{ nullptr };
	bool onTimeout();
    };
}