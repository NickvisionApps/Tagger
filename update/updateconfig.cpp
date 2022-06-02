#include "updateconfig.h"
#include <fstream>
#include <unistd.h>
#include <pwd.h>
#include <json/json.h>
#include "../helpers/curlhelpers.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Update;

UpdateConfig::UpdateConfig() : m_latestVersion{"0.0.0"}, m_changelog{""}, m_linkToTarGz{""}
{

}

std::optional<UpdateConfig> UpdateConfig::loadFromUrl(const std::string& url)
{
    std::string configFilePath{std::string(getpwuid(getuid())->pw_dir) + "/.config/Nickvision/NickvisionTagger/updateConfig.json"};
    if(!CurlHelpers::downloadFile(url, configFilePath))
    {
        return std::nullopt;
    }
    std::ifstream updateConfigFileIn{configFilePath};
    if (updateConfigFileIn.is_open())
    {
        UpdateConfig updateConfig;
        Json::Value json;
        try
        {
            updateConfigFileIn >> json;
        }
        catch(...)
        {
            return updateConfig;
        }
        updateConfigFileIn >> json;
        updateConfig.m_latestVersion = { json.get("LatestVersion", "0.0.0").asString() };
        updateConfig.m_changelog = json.get("Changelog", "").asString();
        updateConfig.m_linkToTarGz = json.get("LinkToTarGz", "").asString();
        return updateConfig;
    }
    else
    {
        return std::nullopt;
    }
}

const Version& UpdateConfig::getLatestVersion() const
{
    return m_latestVersion;
}

const std::string& UpdateConfig::getChangelog() const
{
    return m_changelog;
}

const std::string& UpdateConfig::getLinkToTarGz() const
{
    return m_linkToTarGz;
}
