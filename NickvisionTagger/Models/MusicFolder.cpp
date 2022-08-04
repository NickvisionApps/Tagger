#include "MusicFolder.h"
#include <algorithm>
#include <optional>

namespace NickvisionTagger::Models
{
    MusicFolder::MusicFolder() : m_parentPath{ "" }, m_includeSubfolders{ true }
    {

    }

    const std::filesystem::path& MusicFolder::getParentPath() const
    {
        return m_parentPath;
    }

    void MusicFolder::setParentPath(const std::filesystem::path& parentPath)
    {
        m_parentPath = parentPath;
    }

    bool MusicFolder::getIncludeSubfolders() const
    {
        return m_includeSubfolders;
    }

    void MusicFolder::setIncludeSubfolders(bool includeSubfolders)
    {
        m_includeSubfolders = includeSubfolders;
    }

    std::vector<std::filesystem::path> MusicFolder::getFolderPaths() const
    {
        std::vector<std::filesystem::path> paths;
        paths.push_back(m_parentPath);
        if (m_includeSubfolders)
        {
            for (const std::filesystem::directory_entry& path : std::filesystem::recursive_directory_iterator(m_parentPath))
            {
                if (path.is_directory())
                {
                    paths.push_back(path);
                }
            }
        }
        return paths;
    }

    const std::vector<std::shared_ptr<MusicFile>>& MusicFolder::getFiles() const
    {
        return m_files;
    }

    void MusicFolder::reloadFiles()
    {
        m_files.clear();
        if (std::filesystem::exists(m_parentPath))
        {
            if (m_includeSubfolders)
            {
                for (const std::filesystem::path& path : std::filesystem::recursive_directory_iterator(m_parentPath))
                {
                    std::optional<MediaFileType> fileType{ MediaFileType::parse(path.string()) };
                    if (fileType.has_value() && fileType.value().isAudio())
                    {
                        m_files.push_back(std::make_shared<MusicFile>(path, fileType.value()));
                    }
                }
            }
            else
            {
                for (const std::filesystem::path& file : std::filesystem::directory_iterator(m_parentPath))
                {
                    std::optional<MediaFileType> fileType{ MediaFileType::parse(file.string()) };
                    if (fileType.has_value() && fileType.value().isAudio())
                    {
                        m_files.push_back(std::make_shared<MusicFile>(file, fileType.value()));
                    }
                }
            }
            std::sort(m_files.begin(), m_files.end(), [](const std::shared_ptr<MusicFile>& a, const std::shared_ptr<MusicFile>& b)
            {
                return *a < *b;
            });
        }
    }
}