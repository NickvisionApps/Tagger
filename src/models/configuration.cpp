#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionTagger::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionTagger/" }, m_theme{ Theme::System }, m_isFirstTimeOpen{ true }
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

void Configuration::save() const
{
    std::ofstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        json["Theme"] = static_cast<int>(m_theme);
        json["IsFirstTimeOpen"] = m_isFirstTimeOpen;
        configFile << json;
    }
}
