#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionTagger::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionTagger/" }, m_theme{ Theme::System }, m_isFirstTimeOpen{ true }, m_includeSubfolders{ true }, m_rememberLastOpenedFolder{ true }, m_lastOpenedFolder{ "" }, m_preserveModificationTimeStamp{ false }
{
    if(!std::filesystem::exists(m_configDir))
    {
        std::filesystem::create_directories(m_configDir);
    }
    std::ifstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        configFile >> json;
        m_theme = static_cast<Theme>(json.get("Theme", 0).asInt());
        m_isFirstTimeOpen = json.get("IsFirstTimeOpen", true).asBool();
        m_includeSubfolders = json.get("IncludeSubfolders", true).asBool();
        m_rememberLastOpenedFolder = json.get("RememberLastOpenedFolder", true).asBool();
        m_lastOpenedFolder = json.get("LastOpenedFolder", "").asString();
        m_preserveModificationTimeStamp = json.get("PreserveModificationTimeStamp", false).asBool();
    }
}

Theme Configuration::getTheme() const
{
    return m_theme;
}

void Configuration::setTheme(Theme theme)
{
    m_theme = theme;
}

bool Configuration::getIsFirstTimeOpen() const
{
    return m_isFirstTimeOpen;
}

void Configuration::setIsFirstTimeOpen(bool isFirstTimeOpen)
{
    m_isFirstTimeOpen = isFirstTimeOpen;
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


bool Configuration::getPreserveModificationTimeStamp() const
{
    return m_preserveModificationTimeStamp;
}

void Configuration::setPreserveModificationTimeStamp(bool preserveModificationTimeStamp)
{
    m_preserveModificationTimeStamp = preserveModificationTimeStamp;
}

void Configuration::save() const
{
    std::ofstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        json["Theme"] = static_cast<int>(m_theme);
        json["IsFirstTimeOpen"] = m_isFirstTimeOpen;
        json["IncludeSubfolders"] = m_includeSubfolders;
        json["RememberLastOpenedFolder"] = m_rememberLastOpenedFolder;
        json["LastOpenedFolder"] = m_lastOpenedFolder;
        json["PreserveModificationTimeStamp"] = m_preserveModificationTimeStamp;
        configFile << json;
    }
}
