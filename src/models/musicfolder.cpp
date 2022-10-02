#include "musicfolder.hpp"
#include <algorithm>

using namespace NickvisionTagger::Models;

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

const std::vector<std::shared_ptr<MusicFile>>& MusicFolder::getMusicFiles() const
{
    return m_files;
}

void MusicFolder::reloadMusicFiles()
{
    m_files.clear();
    if (std::filesystem::exists(m_parentPath))
    {
        std::vector<std::string> supportedDotFileExtensions{ ".mp3", ".ogg", ".opus", ".flac", ".wma", ".wav" };
        if (m_includeSubfolders)
        {
            for (const std::filesystem::path& path : std::filesystem::recursive_directory_iterator(m_parentPath, std::filesystem::directory_options::skip_permission_denied))
            {
                if(std::find(supportedDotFileExtensions.begin(), supportedDotFileExtensions.end(), path.extension()) != supportedDotFileExtensions.end())
                {
                    m_files.push_back(std::make_shared<MusicFile>(path));
                }
            }
        }
        else
        {
            for (const std::filesystem::path& path : std::filesystem::directory_iterator(m_parentPath))
            {
                if(std::find(supportedDotFileExtensions.begin(), supportedDotFileExtensions.end(), path.extension()) != supportedDotFileExtensions.end())
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
