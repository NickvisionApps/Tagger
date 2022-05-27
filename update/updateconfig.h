#pragma once

#include <string>
#include <optional>
#include "version.h"

namespace NickvisionTagger::Update
{
    class UpdateConfig
    {
    public:
        static std::optional<UpdateConfig> loadFromUrl(const std::string& url);
        const Version& getLatestVersion() const;
        const std::string& getChangelog() const;
        const std::string& getLinkToTarGz() const;

    private:
        UpdateConfig();
        Version m_latestVersion;
        std::string m_changelog;
        std::string m_linkToTarGz;
    };
}
