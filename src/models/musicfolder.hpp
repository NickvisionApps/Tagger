#pragma once

#include <filesystem>
#include <memory>
#include <string>
#include <vector>
#include "musicfile.hpp"

namespace NickvisionTagger::Models
{
    /**
     * A model of a folder containing music files
     */
    class MusicFolder
    {
    public:
     	/**
     	 * Constructs a MusicFolder
     	 */
    	MusicFolder();
    	/**
    	 * Gets the parent path of the music folder
    	 *
    	 * @returns The parent path of the music folder
    	 */
    	const std::filesystem::path& getParentPath() const;
    	/**
    	 * Sets the parent path of the music folder
    	 *
    	 * @param parentPath The new parent path of the music folder
    	 */
    	void setParentPath(const std::filesystem::path& parentPath);
    	/**
    	 * Gets whether or not to include subfolders in the MusicFolder's scan for music files
    	 *
    	 * @returns True to include subfolders, else false
    	 */
    	bool getIncludeSubfolders() const;
    	/**
    	 * Sets whether or not to include subfolders in the MusicFolder's scan for music files
    	 *
    	 * @param includeSubfolders True to include subfolders, else false
    	 */
    	void setIncludeSubfolders(bool includeSubfolders);
		/**
		 * Gets a list of folder paths in the music folder
		 *
		 * @returns If includeSubfolders is true, a list containing the parent path and all subfolder paths of the music folder. If false, a list containing just the parent path of the music folder
		 */
    	std::vector<std::filesystem::path> getFolderPaths() const;
    	/**
    	 * Gets a list of MusicFile objects representing music files found in the music folder
    	 *
    	 * @returns A list of MusicFile objects representing music ifles found in the music folder
    	 */
    	const std::vector<std::shared_ptr<MusicFile>>& getMusicFiles() const;
    	/**
    	 * Scans the music folder for music files and populates the files list. If includeSubfolders is true, scans subfolders as well. If false, only the parent path
    	 */
    	void reloadMusicFiles();

    private:
		std::filesystem::path m_parentPath;
		bool m_includeSubfolders;
		std::vector<std::shared_ptr<MusicFile>> m_files;
    };
}