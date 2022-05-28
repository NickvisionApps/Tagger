#pragma once

#include <string>
#include <vector>
#include <adwaita.h>
#include "../widget.h"

namespace NickvisionTagger::UI::Controls
{
    class ComboBoxDialog : public NickvisionTagger::UI::Widget
    {
    public:
        ComboBoxDialog(GtkWidget* parent, const std::string& title, const std::string& description, const std::string& rowTitle, const std::vector<std::string>& choices);
        ~ComboBoxDialog();
        const std::string& getSelectedChoice();

    private:
        std::vector<std::string> m_choices;
        std::string m_selectedChoice;
        //==Signals==//
        void cancel();
        void ok();
    };
}
