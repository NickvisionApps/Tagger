#include "Configuration.h"
#include <filesystem>
#include <fstream>
#include <QSettings>
#include <QStandardPaths>
#include <json/json.h>

namespace NickvisionTagger::Models
{
    Configuration::Configuration() : m_configDir{ QStandardPaths::writableLocation(QStandardPaths::AppDataLocation).toStdString() }, m_theme{ Theme::System }, m_includeSubfolders{ true }, m_rememberLastOpenedFolder{ true }, m_lastOpenedFolder{ "" }
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
                m_includeSubfolders = json.get("IncludeSubfolders", true).asBool();
                m_rememberLastOpenedFolder = json.get("RememberLastOpenedFolder", true).asBool();
                m_lastOpenedFolder = json.get("LastOpenedFolder", "").asString();
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

    bool Configuration::getIncludeSubfolders() const
    {
        return m_includeSubfolders;
    }

    void Configuration::setIncludeSubfolders(bool includeSubfolders)
    {
        m_includeSubfolders = includeSubfolders;
    }

    bool Configuration::getRememberLastOpenedFolder() const
    {
        return m_rememberLastOpenedFolder;
    }

    void Configuration::setRememberLastOpenedFolder(bool rememberLastOpenedFolder)
    {
        m_rememberLastOpenedFolder = rememberLastOpenedFolder;
    }

    const std::string& Configuration::getLastOpenedFolder() const
    {
        return m_lastOpenedFolder;
    }

    void Configuration::setLastOpenedFolder(const std::string& lastOpenedFolder)
    {
        m_lastOpenedFolder = lastOpenedFolder;
    }

    void Configuration::save() const
    {
        std::ofstream configFile{ m_configDir + "/config.json" };
        if (configFile.is_open())
        {
            Json::Value json;
            json["Theme"] = static_cast<int>(m_theme);
            json["IncludeSubfolders"] = m_includeSubfolders;
            json["RememberLastOpenedFolder"] = m_rememberLastOpenedFolder;
            json["LastOpenedFolder"] = m_lastOpenedFolder;
            configFile << json;
        }
    }
}