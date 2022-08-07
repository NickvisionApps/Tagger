#pragma once

#include <string>
#include "Theme.h"

namespace NickvisionTagger::Models
{
	/// <summary>
	/// A model for an application's configuration
	/// </summary>
	class Configuration
	{
	public:
		Configuration(const Configuration&) = delete;
		void operator=(const Configuration&) = delete;
		/// <summary>
		/// Gets the Configuration singleton object
		/// </summary>
		/// <returns>A reference to the AppInfo object</returns>
		static Configuration& getInstance();
		/// <summary>
		/// Gets the theme of the application
		/// </summary>
		/// <param name="calculateSystemTheme">Determines if Theme::System should be calculated into Theme::Light or Theme::Dark depending on system configuration. Set true to determine or false to leave as Theme::System</param>
		/// <returns>The theme of the application</returns>
		Theme getTheme(bool calculateSystemTheme = true) const;
		/// <summary>
		/// Sets the theme of the application
		/// </summary>
		/// <param name="theme">The theme of the application</param>
		void setTheme(Theme theme);
		/// <summary>
		/// Gets whether or not to start on home page
		/// </summary>
		/// <returns>True to start on home page, else false</returns>
		bool getAlwaysStartOnHomePage() const;
		/// <summary>
		/// Sets whether or not to start on home page
		/// </summary>
		/// <param name="alwaysStartOnHomePage">True for yes, false for no</param>
		void setAlwaysStartOnHomePage(bool alwaysStartOnHomePage);
		/// <summary>
		/// Gets whether or not to include subfolders
		/// </summary>
		/// <returns>True to include subfolders, else false</returns>
		bool getIncludeSubfolders() const;
		/// <summary>
		/// Sets whether or not to include subfolders
		/// </summary>
		/// <param name="includeSubfolders">True for yes, false for no</param>
		void setIncludeSubfolders(bool includeSubfolders);
		/// <summary>
		/// Gets whether or not to preserve the modification time stamp of a music file
		/// </summary>
		/// <returns>True to preserve, else false</returns>
		bool getPreserveModificationTimeStamp() const;
		/// <summary>
		/// Sets whether or not to preserve the modification time stamp of a music file
		/// </summary>
		/// <param name="preserveModificationTimeStamp">True for yes, false for no</param>
		void setPreserveModificationTimeStamp(bool preserveModificationTimeStamp);
		/// <summary>
		/// Gets the first recent folder
		/// </summary>
		/// <returns>The first recent folder</returns>
		const std::string& getRecentFolder1() const;
		/// <summary>
		/// Gets the second recent folder
		/// </summary>
		/// <returns>The second recent folder</returns>
		const std::string& getRecentFolder2() const;
		/// <summary>
		/// Gets the third recent folder
		/// </summary>
		/// <returns>The third recent folder</returns>
		const std::string& getRecentFolder3() const;
		/// <summary>
		/// Adds a recent folder to the list of recent folders
		/// </summary>
		/// <param name="newRecentFolder">The new recent folder to add to the list</param>
		void addRecentFolder(const std::string& newRecentFolder);
		/// <summary>
		/// Saves the configuration file to disk
		/// </summary>
		void save() const;

	private:
		/// <summary>
		/// Constructs a Configuration object
		/// </summary>
		Configuration();
		std::string m_configDir;
		Theme m_theme;
		bool m_alwaysStartOnHomePage;
		bool m_includeSubfolders;
		bool m_preserveModificationTimeStamp;
		std::string m_recentFolder1;
		std::string m_recentFolder2;
		std::string m_recentFolder3;
	};
}

