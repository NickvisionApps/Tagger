#pragma once

#include <string>

namespace NickvisionTagger::Models
{
    /**
     * Themes for the application
     */
    enum class Theme
    {
    	System = 0,
    	Light,
    	Dark
    };

    /**
     * A model for the settings of the application
     */
    class Configuration
    {
    public:
    	/**
    	 * Constructs a Configuration (loading the configuraton from disk)
    	 */
    	Configuration();
    	/**
    	 * Gets the requested theme
    	 *
    	 * @returns The requested theme
    	 */
    	Theme getTheme() const;
    	/**
    	 * Sets the requested theme
    	 *
    	 * @param theme The new theme
    	 */
    	void setTheme(Theme theme);
    	/**
    	 * Gets whether or not to include subfolders when scanning for music files in a music folder
    	 *
    	 * @returns True to include subfolders, else false
    	 */
    	bool getIncludeSubfolders() const;
    	/**
    	 * Sets whether or not to include subfolders when scanning for music files in a music folder
    	 *
    	 * @param includeSubfolders True to include subfolders, else false
    	 */
    	void setIncludeSubfolders(bool includeSubfolders);
    	/**
    	 * Gets whether or not to remember last opened music folder to reopen on application startup
    	 *
    	 * @returns True to remember last opened music folder, else false
    	 */
    	bool getRememberLastOpenedFolder() const;
    	/**
    	 * Sets whether or not to remember last opened music folder to reopen on application startup
    	 *
    	 * @param rememberLastOpenedFolder True to remember last opened music folder, else false
    	 */
    	void setRememberLastOpenedFolder(bool rememberLastOpenedFolder);
    	/**
    	 * Gets the last opened music folder
    	 *
    	 * @returns The last opened music folder
    	 */
    	const std::string& getLastOpenedFolder() const;
    	/**
    	 * Sets the last opened music folder
    	 *
    	 * @param lastOpenedFolder The new last opened music folder
    	 */
    	void setLastOpenedFolder(const std::string& lastOpenedFolder);
    	/**
    	 * Gets whether or not to preserve the modification time stamp of a music file
    	 *
    	 * @returns True to preserve modification time stamp, else false
    	 */
    	bool getPreserveModificationTimeStamp() const;
    	/**
    	 * Sets whether or not to preserve the modification time stamp of a music file
    	 *
    	 * @param preserveModificationTimeStamp True to preserve modification time stamp, else false
    	 */
    	void setPreserveModificationTimeStamp(bool preserveModificationTimeStamp);
    	/**
    	 * Gets whether or not to overwrite a tag with data from MusicBrainz
    	 *
    	 * @returns True to overwrite tag, false to preserve already filled-in properties
    	 */
    	bool getOverwriteTagWithMusicBrainz() const;
    	/**
    	 * Sets whether or not to overwrite a tag with data from MusicBrainz
    	 *
    	 * @param overwriteTagWithMusicBrainz True to overwrite tag, false to preserve already filled-in properties
    	 */
    	void setOverwriteTagWithMusicBrainz(bool overwriteTagWithMusicBrainz);
    	/**
    	 * Saves the configuration to disk
    	 */
    	void save() const;
    
    private:
    	std::string m_configDir;
    	Theme m_theme;
    	bool m_includeSubfolders;
    	bool m_rememberLastOpenedFolder;
    	std::string m_lastOpenedFolder;
    	bool m_preserveModificationTimeStamp;
    	bool m_overwriteTagWithMusicBrainz;
    };
}