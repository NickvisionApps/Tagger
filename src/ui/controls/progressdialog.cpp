#include "progressdialog.hpp"

using namespace NickvisionTagger::UI::Controls;

ProgressDialog::ProgressDialog(GtkWindow* parent, const std::string& description, const std::function<void()>& work, const std::function<void()>& then) : m_work{ work }, m_then{ then }, m_isFinished{ false }, m_gobj{ adw_window_new() }
{
    //Window Settings
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), parent);
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 400, 60);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    gtk_window_set_resizable(GTK_WINDOW(m_gobj), false);
    gtk_window_set_deletable(GTK_WINDOW(m_gobj), false);
    gtk_window_set_destroy_with_parent(GTK_WINDOW(m_gobj), false);
    //Description Label
    m_lblDescription = gtk_label_new(nullptr);
    gtk_label_set_markup(GTK_LABEL(m_lblDescription), std::string("<b>" + description + "</b>").c_str());
    gtk_widget_set_halign(m_lblDescription, GTK_ALIGN_START);
    //Progress Bar
    m_progBar = gtk_progress_bar_new();
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 20);
    gtk_widget_set_margin_start(m_mainBox, 10);
    gtk_widget_set_margin_top(m_mainBox, 10);
    gtk_widget_set_margin_end(m_mainBox, 10);
    gtk_widget_set_margin_bottom(m_mainBox, 10);
    gtk_box_append(GTK_BOX(m_mainBox), m_lblDescription);
    gtk_box_append(GTK_BOX(m_mainBox), m_progBar);
    adw_window_set_content(ADW_WINDOW(m_gobj), m_mainBox);
    //Timeout
    g_timeout_add(50, [](void* data) -> int
    {
        ProgressDialog* dialog{ reinterpret_cast<ProgressDialog*>(data) };
        bool result = dialog->timeout();
        if(!result)
        {
            delete dialog;
        }
        return result;
    }, this);
    //Thread
    m_thread = std::jthread([&]()
    {
        m_work();
        m_isFinished = true;
    });
}

void ProgressDialog::show()
{
    if(!m_isFinished)
    {
        gtk_widget_show(m_gobj);
    }
}

bool ProgressDialog::timeout()
{
    gtk_progress_bar_pulse(GTK_PROGRESS_BAR(m_progBar));
    if(m_isFinished)
    {
        m_then();
        gtk_window_destroy(GTK_WINDOW(m_gobj));
        return false;
    }
    return true;
}
