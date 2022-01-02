#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <string>

namespace NickvisionTagger::Models
{
    class Configuration
    {
    public:
        Configuration();
        bool includeSubfolders() const;
        void setIncludeSubfolders(bool includeSubfolders);
        bool rememberLastOpenedFolder() const;
        void setRememberLastOpenedFolder(bool rememberLastOpenedFolder);
        const std::string& getLastOpenedFolder() const;
        void setLastOpenedFolder(const std::string& lastOpenedFolder);
        void save() const;

    private:
        std::string m_configDir;
        bool m_includeSubfolders;
        bool m_rememberLastOpenedFolder;
        std::string m_lastOpenedFolder;
    };
}

#endif // CONFIGURATION_H
