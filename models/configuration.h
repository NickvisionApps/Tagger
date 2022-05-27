#pragma once

#include <mutex>
#include <string>

namespace NickvisionTagger::Models
{
    enum class Theme
    {
        System,
        Light,
        Dark
    };

    class Configuration
    {
    public:
        Configuration();
        Theme getTheme() const;
        void setTheme(Theme theme);
        bool getIncludeSubfolders() const;
        void setIncludeSubfolders(bool includeSubfolders);
        bool getRememberLastOpenedFolder() const;
        void setRememberLastOpenedFolder(bool rememberLastOpenedFolder);
        const std::string& getLastOpenedFolder() const;
        void setLastOpenedFolder(const std::string& lastOpenedFolder);
        void save() const;

    private:
        mutable std::mutex m_mutex;
        std::string m_configDir;
        Theme m_theme;
        bool m_includeSubfolders;
        bool m_rememberLastOpenedFolder;
        std::string m_lastOpenedFolder;
    };
}