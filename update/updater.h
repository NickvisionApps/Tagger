#pragma once

#include <mutex>
#include <string>
#include <optional>
#include "version.h"
#include "updateconfig.h"

namespace NickvisionTagger::Update
{
    class Updater
    {
    public:
        Updater(const std::string& linkToConfig, const Version& currentVersion);
        bool getUpdateAvailable() const;
        Version getLatestVersion() const;
        std::string getChangelog() const;
        bool getUpdateSuccessful() const;
        bool checkForUpdates();
        bool update();

    private:
        mutable std::mutex m_mutex;
        std::string m_linkToConfig;
        Version m_currentVersion;
        std::optional<UpdateConfig> m_updateConfig;
        bool m_updateAvailable;
        bool m_updateSuccessful;
        bool validateUpdate(const std::string& pathToUpdate);
    };
}
