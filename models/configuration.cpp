#include "configuration.h"
#include <filesystem>
#include <fstream>
#include <unistd.h>
#include <pwd.h>
#include <json/json.h>

namespace NickvisionTagger::Models
{
    Configuration::Configuration() : m_configDir(std::string(getpwuid(getuid())->pw_dir) + "/.config/Nickvision/NickvisionTagger/"), m_includeSubfolders(true), m_rememberLastOpenedFolder(true), m_lastOpenedFolder("")
    {
        if (!std::filesystem::exists(m_configDir))
        {
            std::filesystem::create_directories(m_configDir);
        }
        std::ifstream configFile(m_configDir + "config.json");
        if (configFile.is_open())
        {
            Json::Value json;
            try
            {
                configFile >> json;
                setIncludeSubfolders(json.get("IncludeSubfolders", true).asBool());
                setRememberLastOpenedFolder(json.get("RememberLastOpenedFolder", true).asBool());
                setLastOpenedFolder(json.get("LastOpenedFolder", "").asString());
            }
            catch (...) { }
        }
    }

    bool Configuration::includeSubfolders() const
    {
        return m_includeSubfolders;
    }

    void Configuration::setIncludeSubfolders(bool includeSubfolders)
    {
        m_includeSubfolders = includeSubfolders;
    }

    bool Configuration::rememberLastOpenedFolder() const
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
        std::ofstream configFile(m_configDir + "config.json");
        if (configFile.is_open())
        {
            Json::Value json;
            json["IncludeSubfolders"] = includeSubfolders();
            json["RememberLastOpenedFolder"] = rememberLastOpenedFolder();
            json["LastOpenedFolder"] = getLastOpenedFolder();
            configFile << json;
        }
    }
}
