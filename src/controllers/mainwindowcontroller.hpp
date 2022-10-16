#pragma once

#include <functional>
#include <string>
#include <vector>
#include <unordered_map>
#include "preferencesdialogcontroller.hpp"
#include "../models/appinfo.hpp"
#include "../models/configuration.hpp"
#include "../models/musicfile.hpp"
#include "../models/musicfolder.hpp"
#include "../models/tagmap.hpp"

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
		 * Gets whether or not the application version is a development version or not
		 *
		 * @returns True for development version, else false
		 */
		bool getIsDevVersion() const;
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
    	 * Gets whether or not the window can close
    	 *
    	 * @returns True if window can close, else false
    	 */
    	bool getCanClose() const;
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
    	 * Registers a callback for when the music folder is changed
    	 *
    	 * @param callback A void(bool) function
    	 */
    	void registerMusicFolderUpdatedCallback(const std::function<void(bool)>& callback);
    	/**
    	 * Gets a list representing whether or not music files are saved
    	 *
    	 * @return The list representing whether or not music files are saved
    	 */
    	const std::vector<bool>& getMusicFilesSaved() const;
    	/**
    	 * Registers a callback for when the music files saved status is changed
    	 *
    	 * @param callback A void() function
    	 */
    	void registerMusicFilesSavedUpdatedCallback(const std::function<void()>& callback);
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
    	 * @param tagMap The TagMap
    	 */
    	void saveTags(const NickvisionTagger::Models::TagMap& tagMap);
    	/**
    	 * Discards unapplied changes to the selected music files
    	 */
    	void discardUnappliedChanges();
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
    	 * Checks whether or not the configuration contains a valid AcoustId User API Key
    	 *
    	 * @returns True if valid, else false
    	 */
    	bool checkIfAcoustIdUserAPIKeyValid();
    	/**
    	 * Uploads tag metadata of one selected file to AcoustId
    	 *
    	 * @param musicBrainzRecordingId A MusicBrainz recording id to associate with the selected file
    	 */
    	void submitToAcoustId(const std::string& musicBrainzRecordingId);
    	/**
    	 * Gets the count of the list of selected music files
    	 *
    	 * @returns The count of the list of selected music files
    	 */
    	size_t getSelectedMusicFilesCount() const;
    	/**
    	 * Gets the selected music file at the beginning of the map
    	 *
    	 * @returns The selected music file at the beginning of the map
    	 */
    	const std::shared_ptr<NickvisionTagger::Models::MusicFile>& getFirstSelectedMusicFile() const;
    	/**
    	 * Gets a TagMap for the selected music files
    	 *
    	 * @returns The TagMap for the selected music files
    	 */
    	NickvisionTagger::Models::TagMap getSelectedTagMap() const;
    	/**
    	 * updates the list of selected music files from the list of indexes
    	 *
    	 * @param indexes The list of selected indexes
    	 */
    	void updateSelectedMusicFiles(std::vector<int> indexes);
    	
    private:
    	NickvisionTagger::Models::AppInfo& m_appInfo;
    	NickvisionTagger::Models::Configuration& m_configuration;
    	bool m_isOpened;
    	bool m_isDevVersion;
    	std::function<void(const std::string& message)> m_sendToastCallback;
    	NickvisionTagger::Models::MusicFolder m_musicFolder;
    	std::function<void(bool sendToast)> m_musicFolderUpdatedCallback;
    	std::vector<bool> m_musicFilesSaved;
    	std::function<void()> m_musicFilesSavedUpdatedCallback;
    	std::unordered_map<int, std::shared_ptr<NickvisionTagger::Models::MusicFile>> m_selectedMusicFiles;
    };
}