#include "HomePage.h"
#include <ctime>
#include "Pages.h"
#include "../Messenger.h"
#include "../../Helpers/ThemeHelpers.h"
#include "../../Models/Configuration.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;
using namespace NickvisionTagger::UI;

namespace NickvisionTagger::UI::Views
{
	HomePage::HomePage(QWidget* parent) : QWidget{ parent }
	{
        //==UI==//
		m_ui.setupUi(this);
        //Welcome
		std::time_t timeNow{ std::time(0) };
		int timeNowHour{ std::localtime(&timeNow)->tm_hour };
        if (timeNowHour >= 0 && timeNowHour < 12)
        {
            m_ui.lblWelcome->setText("Good morning!");
        }
        else if (timeNowHour >= 12 && timeNowHour < 18)
        {
            m_ui.lblWelcome->setText("Good afternoon!");
        }
        else if (timeNowHour >= 18)
        {
            m_ui.lblWelcome->setText("Good evening!");
        }
        //==Theme==//
        refreshTheme();
        //==Load Config==//
        m_ui.chkAlwaysStartOnHomePage->setChecked(Configuration::getInstance().getAlwaysStartOnHomePage());
	}

    void HomePage::refreshTheme()
    {
        m_ui.separator->setStyleSheet(ThemeHelpers::getThemedSeparatorStyle());
    }

    void HomePage::on_btnOpenMusicFolder_clicked()
    {
        Messenger::getInstance().sendMessage("TaggerPage.openMusicFolder", nullptr);
        Pages taggerPage{ Pages::Tagger };
        Messenger::getInstance().sendMessage("MainWindow.changePage", &taggerPage);
    }

    void HomePage::on_chkAlwaysStartOnHomePage_clicked()
    {
        Configuration& configuration{ Configuration::getInstance() };
        configuration.setAlwaysStartOnHomePage(m_ui.chkAlwaysStartOnHomePage->isChecked());
        configuration.save();
    }
}