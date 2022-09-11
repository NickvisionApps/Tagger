#include "mainwindowcontroller.hpp"
#include <chrono>
#include <filesystem>
#include <thread>

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Models;

MainWindowController::MainWindowController(AppInfo& appInfo, Configuration& configuration) : m_appInfo{ appInfo }, m_configuration{ configuration }, m_isOpened{ false }, m_folderPath{ "No Folder Opened" }
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
            m_sendNotificationCallback("Welcome", "Application has been opened for the first time!");
            m_configuration.setIsFirstTimeOpen(false);
            m_configuration.save();
        }
        m_isOpened = true;
    }
}

std::string MainWindowController::getFolderPath() const
{
    return m_folderPath == "No Folder Opened" ? "" : m_folderPath;
}

bool MainWindowController::getIsFolderValid() const
{
    return std::filesystem::exists(m_folderPath);
}

void MainWindowController::registerFolderChangedCallback(const std::function<void()>& callback)
{
    m_folderChangedCallback = callback;
}

bool MainWindowController::openFolder(const std::string& folderPath)
{
    m_folderPath = folderPath;
    bool isValid = getIsFolderValid();
    if(!isValid)
    {
        m_folderPath = "No Folder Opened";
    }
    m_folderChangedCallback();
    m_sendToastCallback("Folder Opened: " + m_folderPath);
    return isValid;
}  

void MainWindowController::closeFolder()
{
    m_folderPath = "No Folder Opened";
    m_folderChangedCallback();
    m_sendToastCallback("Folder closed successfully.");
}
