#include "musicfolder.h"
#include <algorithm>
#include <filesystem>
#include <optional>

using namespace NickvisionTagger::Models;

MusicFolder::MusicFolder() : m_path{""}, m_includeSubfolders{true}
{

}

const std::filesystem::path& MusicFolder::getPath() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_path;
}

void MusicFolder::setPath(const std::filesystem::path& path)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_path = path;
}

bool MusicFolder::getIncludeSubfolders() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_includeSubfolders;
}

void MusicFolder::setIncludeSubfolders(bool includeSubfolders)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_includeSubfolders = includeSubfolders;
}

const std::vector<std::shared_ptr<MusicFile>>& MusicFolder::getFiles() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_files;
}

void MusicFolder::reloadFiles()
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_files.clear();
    if(std::filesystem::exists(m_path))
    {
        if(m_includeSubfolders)
        {
            for(const std::filesystem::path& file : std::filesystem::recursive_directory_iterator(m_path))
            {
                std::optional<MediaFileType> fileType{MediaFileType::parse(file)};
                if(fileType.has_value() && fileType.value().isAudio())
                {
                    m_files.push_back(std::make_shared<MusicFile>(file, fileType.value()));
                }
            }
        }
        else
        {
            for(const std::filesystem::path& file : std::filesystem::directory_iterator(m_path))
            {
                std::optional<MediaFileType> fileType{MediaFileType::parse(file)};
                if(fileType.has_value() && fileType.value().isAudio())
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
