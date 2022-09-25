#include "shortcutsdialog.hpp"

using namespace NickvisionTagger::UI::Views;

ShortcutsDialog::ShortcutsDialog(GtkWindow* parent)
{
    m_xml = R"(
    <?xml version="1.0" encoding="UTF-8"?>
    <interface>
        <object class="GtkShortcutsWindow" id="m_dialog">
            <property name="default-width">600</property>
            <property name="default-height">500</property>
            <property name="modal">true</property>
            <property name="resizable">true</property>
            <property name="destroy-with-parent">false</property>
            <property name="hide-on-close">true</property>
            <child>
                <object class="GtkShortcutsSection">
                    <child>
                        <object class="GtkShortcutsGroup">
                            <property name="title">Music Folder</property>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Open Music Folder</property>
                                    <property name="accelerator">&lt;Control&gt;o</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Reload Music Folder</property>
                                    <property name="accelerator">F5</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class="GtkShortcutsGroup">
                            <property name="title">Tag</property>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Apply</property>
                                    <property name="accelerator">&lt;Control&gt;s</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Delete Tag</property>
                                    <property name="accelerator">Delete</property>
                                </object>
                            </child>
                             <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Inset Album Art</property>
                                    <property name="accelerator">&lt;Control&gt;&lt;Shift&gt;o</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Remove Album Art</property>
                                    <property name="accelerator">&lt;Control&gt;Delete</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Filename to Tag</property>
                                    <property name="accelerator">&lt;Control&gt;f</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Tag to Filename</property>
                                    <property name="accelerator">&lt;Control&gt;t</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Download MusicBrainz Metadata</property>
                                    <property name="accelerator">&lt;Control&gt;m</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class="GtkShortcutsGroup">
                            <property name="title">Application</property>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Settings</property>
                                    <property name="accelerator">&lt;Control&gt;period</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">Keyboard Shortcuts</property>
                                    <property name="accelerator">&lt;Control&gt;question</property>
                                </object>
                            </child>
                            <child>
                                <object class="GtkShortcutsShortcut">
                                    <property name="title">About</property>
                                    <property name="accelerator">F1</property>
                                </object>
                            </child>
                        </object>
                    </child>
                </object>
            </child>
        </object>
    </interface>
    )";
    GtkBuilder* builder{ gtk_builder_new_from_string(m_xml.c_str(), -1) };
    m_gobj = GTK_WIDGET(gtk_builder_get_object(builder, "m_dialog"));
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), GTK_WINDOW(parent));
}

ShortcutsDialog::~ShortcutsDialog()
{
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

GtkWidget* ShortcutsDialog::gobj()
{
    return m_gobj;
}

void ShortcutsDialog::run()
{
    gtk_widget_show(m_gobj);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
}
