#include "mainwindowcontroller.hpp"
#include <chrono>
#include <filesystem>
#include <thread>

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Models;

MainWindowController::MainWindowController(AppInfo& appInfo, Configuration& configuration) : m_appInfo{ appInfo }, m_configuration{ configuration }, m_isOpened{ false }
{

}

const AppInfo& MainWindowController::getAppInfo() const
{
    return m_appInfo;
}

PreferencesDialogController MainWindowController::createPreferencesDialogController() const
{
    return { m_configuration };
}

void MainWindowController::registerSendToastCallback(const std::function<void(const std::string& message)>& callback)
{
    m_sendToastCallback = callback;
}

void MainWindowController::registerSendNotificationCallback(const std::function<void(const std::string& title, const std::string& message)>& callback)
{
    m_sendNotificationCallback = callback;
}

void MainWindowController::startup()
{
    if(!m_isOpened)
    {
        if(m_configuration.getIsFirstTimeOpen())
        {
            m_configuration.setIsFirstTimeOpen(false);
            m_configuration.save();
        }
        m_musicFolder.setIncludeSubfolders(m_configuration.getIncludeSubfolders());
        if(m_configuration.getRememberLastOpenedFolder())
        {
            openMusicFolder(m_configuration.getLastOpenedFolder());
        }
        m_isOpened = true;
    }
}

void MainWindowController::onConfigurationChanged()
{
    if(m_musicFolder.getIncludeSubfolders() != m_configuration.getIncludeSubfolders())
    {
        m_musicFolder.setIncludeSubfolders(m_configuration.getIncludeSubfolders());
        m_musicFolderUpdatedCallback();
    }
}

const std::filesystem::path& MainWindowController::getMusicFolderPath() const
{
    return m_musicFolder.getParentPath();
}

std::size_t MainWindowController::getMusicFileCount() const
{
    return m_musicFolder.getMusicFiles().size();
}

const std::vector<std::shared_ptr<MusicFile>>& MainWindowController::getMusicFiles() const
{
    return m_musicFolder.getMusicFiles();
}

void MainWindowController::openMusicFolder(const std::string& folderPath)
{
    m_musicFolder.setParentPath(std::filesystem::exists(folderPath) ? folderPath : "");
    if(m_configuration.getRememberLastOpenedFolder())
    {
        m_configuration.setLastOpenedFolder(m_musicFolder.getParentPath());
        m_configuration.save();
    }
    m_musicFolderUpdatedCallback();
}

void MainWindowController::reloadMusicFolder()
{
    m_musicFolder.reloadMusicFiles();
}

void MainWindowController::deleteTags()
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        musicFile->removeTag();
    }
    m_musicFolderUpdatedCallback();
}

void MainWindowController::registerMusicFolderUpdatedCallback(const std::function<void()>& callback)
{
    m_musicFolderUpdatedCallback = callback;
}

const std::vector<std::shared_ptr<MusicFile>>& MainWindowController::getSelectedMusicFiles() const
{
    return m_selectedMusicFiles;
}

void MainWindowController::updateSelectedMusicFiles(std::vector<int> indexes)
{
    m_selectedMusicFiles.clear();
    for(int index : indexes)
    {
        m_selectedMusicFiles.push_back(m_musicFolder.getMusicFiles()[index]);
    }
}
