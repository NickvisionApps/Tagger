#pragma once

#include <adwaita.h>
#include "../widget.h"
#include "../../models/configuration.h"

namespace NickvisionTagger::UI::Views
{
    class PreferencesDialog : public NickvisionTagger::UI::Widget
    {
    public:
        PreferencesDialog(GtkWidget* parent, NickvisionTagger::Models::Configuration& configuration);
        ~PreferencesDialog();

    private:
        NickvisionTagger::Models::Configuration& m_configuration;
        //==Signals==//
        void cancel();
        void save();
        void onRowIncludeSubfoldersActivate();
        void onRowRememberLastOpenedFolderActivate();
    };
}
