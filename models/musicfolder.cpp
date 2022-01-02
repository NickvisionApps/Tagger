#include "musicfolder.h"
#include <algorithm>
#include <filesystem>

namespace NickvisionTagger::Models
{
    MusicFolder::MusicFolder() : m_path(""), m_includeSubfolders(true)
    {

    }

    const std::string& MusicFolder::getPath() const
    {
        return m_path;
    }

    void MusicFolder::setPath(const std::string& path)
    {
        m_path = path;
    }

    bool MusicFolder::includeSubfolders() const
    {
        return m_includeSubfolders;
    }

    void MusicFolder::setIncludeSubfolders(bool includeSubfolders)
    {
        m_includeSubfolders = includeSubfolders;
    }

    const std::vector<std::shared_ptr<MusicFile>>& MusicFolder::getFiles() const
    {
        return m_files;
    }

    void MusicFolder::reloadFiles()
    {
        m_files.clear();
        if(std::filesystem::exists(m_path))
        {
            std::vector<std::string> supportedExtensions{ ".mp3", ".wav", ".wma", ".flac", ".ogg" };
            if(m_includeSubfolders)
            {
                for(const std::filesystem::path& path : std::filesystem::recursive_directory_iterator(m_path))
                {
                    if(std::find(supportedExtensions.begin(), supportedExtensions.end(), path.extension()) != supportedExtensions.end())
                    {
                        m_files.push_back(std::make_shared<MusicFile>(path));
                    }
                }
            }
            else
            {
                for(const std::filesystem::path& path : std::filesystem::directory_iterator(m_path))
                {
                    if(std::find(supportedExtensions.begin(), supportedExtensions.end(), path.extension()) != supportedExtensions.end())
                    {
                        m_files.push_back(std::make_shared<MusicFile>(path));
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
