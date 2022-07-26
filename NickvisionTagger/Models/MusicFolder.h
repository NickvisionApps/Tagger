#pragma once

#include <filesystem>
#include <memory>
#include <string>
#include <vector>
#include "MusicFile.h"

namespace NickvisionTagger::Models
{
    class MusicFolder
    {
    public:
        MusicFolder();
        const std::filesystem::path& getPath() const;
        void setPath(const std::filesystem::path& path);
        bool getIncludeSubfolders() const;
        void setIncludeSubfolders(bool includeSubfolders);
        const std::vector<std::shared_ptr<MusicFile>>& getFiles() const;
        void reloadFiles();

    private:
        std::filesystem::path m_path;
        bool m_includeSubfolders;
        std::vector<std::shared_ptr<MusicFile>> m_files;
    };
}

