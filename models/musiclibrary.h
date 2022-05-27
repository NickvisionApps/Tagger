#pragma once

#include <mutex>
#include <string>
#include <vector>
#include <memory>
#include <filesystem>
#include "musicfile.h"

namespace NickvisionTagger::Models
{
    class MusicLibrary
    {
    public:
        MusicLibrary();
        const std::vector<std::filesystem::path>& getPaths() const;
        bool getIncludeSubfolders() const;
        void setIncludeSubfolders(bool includeSubfolders);
        const std::vector<std::shared_ptr<MusicFile>>& getFiles() const;
        void addPath(const std::filesystem::path& path);
        bool removePath(std::size_t index);
        void clearPaths();
        void reloadFiles();

    private:
        mutable std::mutex m_mutex;
        std::vector<std::filesystem::path> m_paths;
        bool m_includeSubfolders;
        std::vector<std::shared_ptr<MusicFile>> m_files;
    };
}