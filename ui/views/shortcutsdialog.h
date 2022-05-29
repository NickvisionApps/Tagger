#pragma once

#include <adwaita.h>
#include "../widget.h"

namespace NickvisionTagger::UI::Views
{
    class ShortcutsDialog : public NickvisionTagger::UI::Widget
    {
    public:
        ShortcutsDialog(GtkWidget* parent);
        ~ShortcutsDialog();

    private:

    };
}
