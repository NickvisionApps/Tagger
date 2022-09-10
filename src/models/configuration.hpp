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
    	 * Gets whether or not the application is being opened for the first time
    	 * 
    	 * @returns True if the application is being opened for the first time, else false
    	 */
    	bool getIsFirstTimeOpen() const;
    	/**
    	 * Sets whether or not the application will be treated as being opened for the first time 
    	 *
    	 * @param isFirstTimeOpen True to be treated as being opened for the first time, else false
    	 */
    	void setIsFirstTimeOpen(bool isFirstTimeOpen);
    	/**
    	 * Saves the configuration to disk
    	 */
    	void save() const;
    
    private:
    	std::string m_configDir;
    	Theme m_theme;
    	bool m_isFirstTimeOpen;
    };
}