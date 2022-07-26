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
		/// Gets whether or not to remember last opened folder
		/// </summary>
		/// <returns>True to remember, else false</returns>
		bool getRememberLastOpenedFolder() const;
		/// <summary>
		/// Sets whether or not to remember last opened folder
		/// </summary>
		/// <param name="rememberLastOpenedFolder">True to remember, else false</param>
		void setRememberLastOpenedFolder(bool rememberLastOpenedFolder);
		/// <summary>
		/// Gets the last opened folder
		/// </summary>
		/// <returns>The last opened folder</returns>
		const std::string& getLastOpenedFolder() const;
		/// <summary>
		/// Sets the last opened folder
		/// </summary>
		/// <param name="lastOpenedFolder">The last opened folder</param>
		void setLastOpenedFolder(const std::string& lastOpenedFolder);
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
		bool m_includeSubfolders;
		bool m_rememberLastOpenedFolder;
		std::string m_lastOpenedFolder;
	};
}

