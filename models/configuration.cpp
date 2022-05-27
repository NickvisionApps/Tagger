#include "configuration.h"
#include <filesystem>
#include <fstream>
#include <unistd.h>
#include <pwd.h>
#include <json/json.h>

using namespace NickvisionTagger::Models;

Configuration::Configuration() : m_configDir{std::string(getpwuid(getuid())->pw_dir) + "/.config/Nickvision/NickvisionTagger/"}, m_theme{Theme::System}, m_includeSubfolders{true}, m_rememberLastOpenedFolder{true}, m_lastOpenedFolder{""}
{
    if (!std::filesystem::exists(m_configDir))
    {
        std::filesystem::create_directories(m_configDir);
    }
    std::ifstream configFile{m_configDir + "config.json"};
    if (configFile.is_open())
    {
        Json::Value json;
        configFile >> json;
        m_theme = static_cast<Theme>(json.get("Theme", 0).asInt());
        m_includeSubfolders = json.get("IncludeSubfolders", true).asBool();
        m_rememberLastOpenedFolder = json.get("RememberLastOpenedFolder", true).asBool();
        m_lastOpenedFolder = json.get("LastOpenedFolder", "").asString();
    }
}

Theme Configuration::getTheme() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_theme;
}

void Configuration::setTheme(Theme theme)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_theme = theme;
}

bool Configuration::getIncludeSubfolders() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_includeSubfolders;
}

void Configuration::setIncludeSubfolders(bool includeSubfolders)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_includeSubfolders = includeSubfolders;
}

bool Configuration::getRememberLastOpenedFolder() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_rememberLastOpenedFolder;
}

void Configuration::setRememberLastOpenedFolder(bool rememberLastOpenedFolder)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_rememberLastOpenedFolder = rememberLastOpenedFolder;
}

const std::string& Configuration::getLastOpenedFolder() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_lastOpenedFolder;
}

void Configuration::setLastOpenedFolder(const std::string& lastOpenedFolder)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_lastOpenedFolder = lastOpenedFolder;
}

void Configuration::save() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    std::ofstream configFile{m_configDir + "config.json"};
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
