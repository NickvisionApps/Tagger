#pragma once

#include <filesystem>
#include <memory>
#include <string>
#include <vector>
#include "MusicFile.h"

namespace NickvisionTagger::Models
{
    /// <summary>
    /// A model of a folder containing music files
    /// </summary>
    class MusicFolder
    {
    public:
        /// <summary>
        /// Constructs a MusicFolder
        /// </summary>
        MusicFolder();
        /// <summary>
        /// Gets the parent path of the music folder
        /// </summary>
        /// <returns>The parent path of the music folder</returns>
        const std::filesystem::path& getParentPath() const;
        /// <summary>
        /// Sets the parent path of the music folder
        /// </summary>
        /// <param name="parentPath">The new parent path</param>
        void setParentPath(const std::filesystem::path& parentPath);
        /// <summary>
        /// Gets whether or not to include subfolders in the MusicFolder's scan for music files
        /// </summary>
        /// <returns>True to include, false to not include</returns>
        bool getIncludeSubfolders() const;
        /// <summary>
        /// Sets whether or not to include subfodlers in the MusicFolder's scan for music files
        /// </summary>
        /// <param name="includeSubfolders">True for yes, false for no</param>
        void setIncludeSubfolders(bool includeSubfolders);
        /// <summary>
        /// Gets a list of folder paths found in the music folder.
        /// </summary>
        /// <returns>If includeSubfolders is true, a list containing the parent path and all subfolder paths of the music folder. If false, a list containing just the parent path of the music folder</returns>
        std::vector<std::filesystem::path> getFolderPaths() const;
        /// <summary>
        /// Gets a list of MusicFile objects representing music files found in the music folder
        /// </summary>
        /// <returns>A list of MusicFiles objects representing music files found in the music folder</returns>
        const std::vector<std::shared_ptr<MusicFile>>& getFiles() const;
        /// <summary>
        /// Scans the music folder for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
        /// </summary>
        void reloadFiles();

    private:
        std::filesystem::path m_parentPath;
        bool m_includeSubfolders;
        std::vector<std::shared_ptr<MusicFile>> m_files;
    };
}

