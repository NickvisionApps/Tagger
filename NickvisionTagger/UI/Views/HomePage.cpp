#include "HomePage.h"
#include <ctime>
#include <filesystem>
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
        //Recent Folder Buttons
        m_ui.btnRecentFolder1->setVisible(false);
        m_ui.btnRecentFolder2->setVisible(false);
        m_ui.btnRecentFolder3->setVisible(false);
        //==Theme==//
        refreshTheme();
        //==Messages==//
        Messenger::getInstance().registerMessage("HomePage.updateRecentFoldersList", [&](void* parameter) { updateRecentFoldersList(); });
        //==Load Config==//
        m_ui.chkAlwaysStartOnHomePage->setChecked(Configuration::getInstance().getAlwaysStartOnHomePage());
        updateRecentFoldersList();
	}

    void HomePage::refreshTheme()
    {
        m_ui.separator1->setStyleSheet(ThemeHelpers::getThemedSeparatorStyle());
    }

    void HomePage::on_btnRecentFolder1_clicked()
    {
        std::string recentFolderPath{ m_ui.btnRecentFolder1->text().toStdString() };
        Messenger::getInstance().sendMessage("TaggerPage.openMusicFolderByPath", &recentFolderPath);
        Pages taggerPage{ Pages::Tagger };
        Messenger::getInstance().sendMessage("MainWindow.changePage", &taggerPage);
    }

    void HomePage::on_btnRecentFolder2_clicked()
    {
        std::string recentFolderPath{ m_ui.btnRecentFolder2->text().toStdString() };
        Messenger::getInstance().sendMessage("TaggerPage.openMusicFolderByPath", &recentFolderPath);
        Pages taggerPage{ Pages::Tagger };
        Messenger::getInstance().sendMessage("MainWindow.changePage", &taggerPage);
    }

    void HomePage::on_btnRecentFolder3_clicked()
    {
        std::string recentFolderPath{ m_ui.btnRecentFolder3->text().toStdString() };
        Messenger::getInstance().sendMessage("TaggerPage.openMusicFolderByPath", &recentFolderPath);
        Pages taggerPage{ Pages::Tagger };
        Messenger::getInstance().sendMessage("MainWindow.changePage", &taggerPage);
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

    void HomePage::updateRecentFoldersList()
    {
        Configuration& configuration{ Configuration::getInstance() };
        if (!configuration.getRecentFolder1().empty() && std::filesystem::exists(configuration.getRecentFolder1()))
        {
            m_ui.btnRecentFolder1->setText(QString::fromStdString(configuration.getRecentFolder1()));
            m_ui.btnRecentFolder1->setVisible(true);
        }
        if (!configuration.getRecentFolder2().empty() && std::filesystem::exists(configuration.getRecentFolder2()))
        {
            m_ui.btnRecentFolder2->setText(QString::fromStdString(configuration.getRecentFolder2()));
            m_ui.btnRecentFolder2->setVisible(true);
        }
        if (!configuration.getRecentFolder3().empty() && std::filesystem::exists(configuration.getRecentFolder3()))
        {
            m_ui.btnRecentFolder3->setText(QString::fromStdString(configuration.getRecentFolder3()));
            m_ui.btnRecentFolder3->setVisible(true);
        }
    }
}