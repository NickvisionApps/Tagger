#pragma once

#include <functional>
#include <string>
#include <unordered_map>
#include <vector>
#include "preferencesdialogcontroller.hpp"
#include "../models/appinfo.hpp"
#include "../models/configuration.hpp"
#include "../models/musicfile.hpp"
#include "../models/musicfolder.hpp"

namespace NickvisionTagger::Controllers
{
    /**
     * A controller for the MainWindow
     */
    class MainWindowController
    {
    public:
    	/**
    	 * Constructs a MainWindowController
    	 *
    	 * @param appInfo The AppInfo for the application (Stored as a reference)
    	 * @param configuration The Configuration for the application (Stored as a reference)
    	 */
    	MainWindowController(NickvisionTagger::Models::AppInfo& appInfo, NickvisionTagger::Models::Configuration& configuration);
    	/**
    	 * Gets the AppInfo object representing the application's information
    	 *
    	 * @returns The AppInfo object for the application
    	 */
    	const NickvisionTagger::Models::AppInfo& getAppInfo() const;
    	/**
    	 * Creates a PreferencesDialogController
    	 *
    	 * @returns A new PreferencesDialogController
    	 */
    	PreferencesDialogController createPreferencesDialogController() const;
    	/**
    	 * Registers a callback for sending a toast notification on the MainWindow
    	 *
    	 * @param callback A void(const std::string&) function
    	 */
    	void registerSendToastCallback(const std::function<void(const std::string&)>& callback);
    	/**
    	 * Runs startup functions
    	 */
    	void startup();
    	/**
    	 * Updates the controller based on the configuration changes
    	 */
    	void onConfigurationChanged();
    	/**
    	 * Gets the opened music folder path
    	 *
    	 * @returns The opened music folder path or "No Folder Path" if no folder is opened
    	 */
	 	const std::filesystem::path& getMusicFolderPath() const;
	 	/**
	 	 * Gets the list of music files in the music folder
	 	 *
	 	 * @returns The list of music files in the music folder
	 	 */
		const std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>>& getMusicFiles() const;
    	/**
    	 * Opens a music folder with the given path
    	 * 
    	 * @param folderPath The path to the folder to open
    	 */
    	void openMusicFolder(const std::string& folderPath);
    	/**
    	 * Reloads a music folder
    	 */
    	void reloadMusicFolder();
    	/**
    	 * Saves the tags of the selected music files
    	 *
    	 * @param tagMap An unordered_map of the tag properties to save
    	 */
    	void saveTags(const std::unordered_map<std::string, std::string>& tagMap);
    	/**
    	 * Deletes the tags of the selected music files
    	 */
    	void deleteTags();
    	/**
    	 * Uses the provided image path to set the album art for the selected music files
    	 *
    	 * @param pathToImage The path to the image to use as album art
    	 */
    	void insertAlbumArt(const std::string& pathToImage);
    	/**
    	 * Removes the album art from the selected music files
    	 */
    	void removeAlbumArt();
    	/**
    	 * Runs the filename to tag conversion on the selected music files
    	 *
    	 * @param formatString The format string for the conversion
    	 */
    	void filenameToTag(const std::string& formatString);
    	/**
    	 * Runs the tag to filename conversion on the selected music files
    	 *
    	 * @param formatString The format string for the conversion
    	 */
    	void tagToFilename(const std::string& formatString);
    	/**
    	 * Downloads and applys tag metadata from MusicBrainz for the selected files
    	 */
    	void downloadMusicBrainzMetadata();
    	/**
    	 * Uploads tag metadata of the selected files to AcoustId
    	 */
    	void submitToAcoustId();
    	/**
    	 * Registers a callback for when the music folder is changed
    	 *
    	 * @param callback A void(bool) function
    	 */
    	void registerMusicFolderUpdatedCallback(const std::function<void(bool)>& callback);
    	/**
    	 * Gets the list of selected music files from the UI
    	 *
    	 * @returns The list of selected music files from the UI
    	 */
    	const std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>>& getSelectedMusicFiles() const;
    	/**
    	 * Gets a tag properties map for the selected music files
    	 *
    	 * @returns The tag properties map for the selected music files
    	 */
    	std::unordered_map<std::string, std::string> getSelectedTagMap() const;
    	/**
    	 * updates the list of selected music files from the list of indexes
    	 *
    	 * @param indexes The list of selected indexes
    	 */
    	void updateSelectedMusicFiles(std::vector<int> indexes);
    	/**
    	 * Checks whether or not the configuration contains a valid AcoustId User API Key
    	 *
    	 * @returns True if valid, else false
    	 */
    	bool checkIfValidAcoustIdUserAPIKey();
    	
    private:
    	NickvisionTagger::Models::AppInfo& m_appInfo;
    	NickvisionTagger::Models::Configuration& m_configuration;
    	bool m_isOpened;
    	std::function<void(const std::string& message)> m_sendToastCallback;
    	NickvisionTagger::Models::MusicFolder m_musicFolder;
    	std::function<void(bool sendToast)> m_musicFolderUpdatedCallback;
    	std::vector<std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
    };
}