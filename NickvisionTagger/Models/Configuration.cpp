#include "Configuration.h"
#include <filesystem>
#include <fstream>
#include <QSettings>
#include <QStandardPaths>
#include <json/json.h>

namespace NickvisionTagger::Models
{
    Configuration::Configuration() : m_configDir{ QStandardPaths::writableLocation(QStandardPaths::AppDataLocation).toStdString() }, m_theme{ Theme::System }, m_alwaysStartOnHomePage{ true }, m_includeSubfolders{ true }, m_recentFolder1{ "" }, m_recentFolder2{ "" }, m_recentFolder3{ "" }
    {
        if (!std::filesystem::exists(m_configDir))
        {
            std::filesystem::create_directories(m_configDir);
        }
        std::ifstream configFile{ m_configDir + "/config.json" };
        if (configFile.is_open())
        {
            Json::Value json;
            configFile >> json;
            try
            {
                m_theme = static_cast<Theme>(json.get("Theme", 2).asInt());
                m_alwaysStartOnHomePage = json.get("AlwaysStartOnHomePage", true).asBool();
                m_includeSubfolders = json.get("IncludeSubfolders", true).asBool();
                m_recentFolder1 = json.get("RecentFolder1", "").asString();
                m_recentFolder2 = json.get("RecentFolder2", "").asString();
                m_recentFolder3 = json.get("RecentFolder3", "").asString();
            }
            catch (...) { }
        }
    }

    Configuration& Configuration::getInstance()
    {
        static Configuration instance;
        return instance;
    }

    Theme Configuration::getTheme(bool calculateSystemTheme) const
    {
        if (!calculateSystemTheme || m_theme != Theme::System)
        {
            return m_theme;
        }
        QSettings regKeyTheme{ "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", QSettings::NativeFormat };
        return regKeyTheme.value("AppsUseLightTheme").toBool() ? Theme::Light : Theme::Dark;
    }

    void Configuration::setTheme(Theme theme)
    {
        m_theme = theme;
    }

    bool Configuration::getAlwaysStartOnHomePage() const
    {
        return m_alwaysStartOnHomePage;
    }

    void Configuration::setAlwaysStartOnHomePage(bool alwaysStartOnHomePage)
    {
        m_alwaysStartOnHomePage = alwaysStartOnHomePage;
    }

    bool Configuration::getIncludeSubfolders() const
    {
        return m_includeSubfolders;
    }

    void Configuration::setIncludeSubfolders(bool includeSubfolders)
    {
        m_includeSubfolders = includeSubfolders;
    }

    const std::string& Configuration::getRecentFolder1() const
    {
        return m_recentFolder1;
    }

    const std::string& Configuration::getRecentFolder2() const
    {
        return m_recentFolder2;
    }

    const std::string& Configuration::getRecentFolder3() const
    {
        return m_recentFolder3;
    }

    void Configuration::addRecentFolder(const std::string& newRecentFolder)
    {
        if (newRecentFolder == m_recentFolder1)
        {
            return;
        }
        else if (newRecentFolder == m_recentFolder2)
        {
            std::string temp1 = m_recentFolder1;
            m_recentFolder1 = m_recentFolder2;
            m_recentFolder2 = temp1;
        }
        else if (newRecentFolder == m_recentFolder3)
        {
            std::string temp1 = m_recentFolder1;
            std::string temp2 = m_recentFolder2;
            m_recentFolder1 = m_recentFolder3;
            m_recentFolder2 = temp1;
            m_recentFolder3 = temp2;
        }
        else
        {
            m_recentFolder3 = m_recentFolder2;
            m_recentFolder2 = m_recentFolder1;
            m_recentFolder1 = newRecentFolder;
        }
    }

    void Configuration::save() const
    {
        std::ofstream configFile{ m_configDir + "/config.json" };
        if (configFile.is_open())
        {
            Json::Value json;
            json["Theme"] = static_cast<int>(m_theme);
            json["AlwaysStartOnHomePage"] = m_alwaysStartOnHomePage;
            json["IncludeSubfolders"] = m_includeSubfolders;
            json["RecentFolder1"] = m_recentFolder1;
            json["RecentFolder2"] = m_recentFolder2;
            json["RecentFolder3"] = m_recentFolder3;
            configFile << json;
        }
    }
}