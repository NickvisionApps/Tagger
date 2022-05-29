#include "updater.h"
#include <filesystem>
#include <fstream>
#include <unistd.h>
#include <pwd.h>
#include <cstdio>
#include <array>
#include "../helpers/curlhelpers.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Update;

Updater::Updater(const std::string& linkToConfig, const Version& currentVersion) : m_linkToConfig{linkToConfig}, m_currentVersion{currentVersion}, m_updateConfig{std::nullopt}, m_updateAvailable{false}, m_updateSuccessful{false}
{

}

bool Updater::getUpdateAvailable() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_updateAvailable;
}

Version Updater::getLatestVersion() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_updateConfig.has_value())
    {
        return m_updateConfig->getLatestVersion();
    }
    return { "-1.-1.-1" };
}

std::string Updater::getChangelog() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_updateConfig.has_value() ? m_updateConfig->getChangelog() : "";
}

bool Updater::getUpdateSuccessful() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_updateSuccessful;
}

bool Updater::checkForUpdates()
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_updateConfig = UpdateConfig::loadFromUrl(m_linkToConfig);
    m_updateAvailable = m_updateConfig.has_value() && m_updateConfig->getLatestVersion() > m_currentVersion;
    return m_updateAvailable;
}

bool Updater::update()
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(!m_updateAvailable)
    {
        m_updateSuccessful = false;
        return m_updateSuccessful;
    }
    std::string downloadsDir{std::string(getpwuid(getuid())->pw_dir) + "/Downloads"};
    if(std::filesystem::exists(downloadsDir))
    {
        std::filesystem::create_directories(downloadsDir);
    }
    std::string tarGzPath{downloadsDir + "/NickvisionTagger.tar.gz"};
    m_updateSuccessful = CurlHelpers::downloadFile(m_updateConfig->getLinkToTarGz(), tarGzPath);
    return m_updateSuccessful;
}
