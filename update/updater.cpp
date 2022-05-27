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
    if(!CurlHelpers::downloadFile(m_updateConfig->getLinkToTarGz(), tarGzPath))
    {
        m_updateSuccessful = false;
        return m_updateSuccessful;
    }
    if(!validateUpdate(tarGzPath))
    {
        m_updateSuccessful = false;
    }
    m_updateSuccessful = true;
    return m_updateSuccessful;
}

bool Updater::validateUpdate(const std::string& pathToUpdate)
{
    std::string copyUpdatePath{std::string(getpwuid(getuid())->pw_dir) + "/.config/Nickvision/NickvisionTagger/update.tar.gz"};
    std::filesystem::copy(pathToUpdate, copyUpdatePath, std::filesystem::copy_options::overwrite_existing);
    std::string cmdUnzip{"tar --overwrite -zxf " + copyUpdatePath};
    std::string cmdOutput{""};
    std::array<char, 128> buffer;
    FILE* pipe{popen(cmdUnzip.c_str(), "r")};
    if(!pipe)
    {
        return false;
    }
    while (!feof(pipe))
    {
        if (fgets(buffer.data(), 128, pipe) != nullptr)
        {
            cmdOutput += buffer.data();
        }
    }
    int resultCode = pclose(pipe);
    if(resultCode != EXIT_SUCCESS)
    {
        std::filesystem::remove(pathToUpdate);
        return false;
    }
    return true;
}
