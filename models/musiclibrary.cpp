#include "musiclibrary.h"
#include <algorithm>
#include <filesystem>
#include <optional>

using namespace NickvisionTagger::Models;

MusicLibrary::MusicLibrary() : m_includeSubfolders{true}
{

}

const std::vector<std::filesystem::path>& MusicLibrary::getPaths() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_paths;
}

bool MusicLibrary::getIncludeSubfolders() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_includeSubfolders;
}

void MusicLibrary::setIncludeSubfolders(bool includeSubfolders)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_includeSubfolders = includeSubfolders;
}

const std::vector<std::shared_ptr<MusicFile>>& MusicLibrary::getFiles() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_files;
}

void MusicLibrary::addPath(const std::filesystem::path& path)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_paths.push_back(path);
}

bool MusicLibrary::removePath(std::size_t index)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_paths.size() == 0 || index > m_paths.size() - 1)
    {
        return false;
    }
    m_paths.erase(m_paths.begin() + index);
    return true;
}

void MusicLibrary::clearPaths()
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_paths.clear();
}

void MusicLibrary::reloadFiles()
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_files.clear();
    for(const std::filesystem::path& path : m_paths)
    {
        if(std::filesystem::exists(path))
        {
            if(m_includeSubfolders)
            {
                for(const std::filesystem::path& file : std::filesystem::recursive_directory_iterator(path))
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
                for(const std::filesystem::path& file : std::filesystem::directory_iterator(path))
                {
                    std::optional<MediaFileType> fileType{MediaFileType::parse(file)};
                    if(fileType.has_value() && fileType.value().isAudio())
                    {
                        m_files.push_back(std::make_shared<MusicFile>(file, fileType.value()));
                    }
                }
            }
        }
    }
    std::sort(m_files.begin(), m_files.end(), [](const std::shared_ptr<MusicFile>& a, const std::shared_ptr<MusicFile>& b)
    {
        return a.get() < b.get();
    });
}
