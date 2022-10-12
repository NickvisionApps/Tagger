#include "mainwindowcontroller.hpp"
#include <chrono>
#include <filesystem>
#include <future>
#include <curlpp/cURLpp.hpp>
#include "../helpers/mediahelpers.hpp"
#include "../models/acoustidsubmission.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

MainWindowController::MainWindowController(AppInfo& appInfo, Configuration& configuration) : m_appInfo{ appInfo }, m_configuration{ configuration }, m_isOpened{ false }, m_isDevVersion{ m_appInfo.getVersion().find("-") != std::string::npos }
{

}

const AppInfo& MainWindowController::getAppInfo() const
{
    return m_appInfo;
}

bool MainWindowController::getIsDevVersion() const
{
    return m_isDevVersion;
}

PreferencesDialogController MainWindowController::createPreferencesDialogController() const
{
    return { m_configuration };
}

void MainWindowController::registerSendToastCallback(const std::function<void(const std::string&)>& callback)
{
    m_sendToastCallback = callback;
}

void MainWindowController::startup()
{
    if(!m_isOpened)
    {
        cURLpp::initialize();
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
        m_musicFolderUpdatedCallback(true);
    }
}

const std::filesystem::path& MainWindowController::getMusicFolderPath() const
{
    return m_musicFolder.getParentPath();
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
    m_musicFolderUpdatedCallback(true);
}

void MainWindowController::reloadMusicFolder()
{
    m_musicFolder.reloadMusicFiles();
}

void MainWindowController::saveTags(const TagMap& tagMap)
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        if(tagMap.getFilename() != musicFile->getFilename() && tagMap.getFilename() != "<keep>")
        {
            musicFile->setFilename(tagMap.getFilename());
        }
        if(tagMap.getTitle() != "<keep>")
        {
            musicFile->setTitle(tagMap.getTitle());
        }
        if(tagMap.getArtist() != "<keep>")
        {
            musicFile->setArtist(tagMap.getArtist());
        }
        if(tagMap.getAlbum() != "<keep>")
        {
            musicFile->setAlbum(tagMap.getAlbum());
        }
        if(tagMap.getYear() != "<keep>")
        {
            try
            {
                musicFile->setYear(MediaHelpers::stoui(tagMap.getYear()));
            }
            catch(...) { }
        }
        if(tagMap.getTrack() != "<keep>")
        {
            try
            {
                musicFile->setTrack(MediaHelpers::stoui(tagMap.getTrack()));
            }
            catch(...) { }
        }
        if(tagMap.getAlbumArtist() != "<keep>")
        {
            musicFile->setAlbumArtist(tagMap.getAlbumArtist());
        }
        if(tagMap.getGenre() != "<keep>")
        {
            musicFile->setGenre(tagMap.getGenre());
        }
        if(tagMap.getComment() != "<keep>")
        {
            musicFile->setComment(tagMap.getComment());
        }
        musicFile->saveTag(m_configuration.getPreserveModificationTimeStamp());
    }
    m_sendToastCallback("Tags saved successfully.");
}

void MainWindowController::deleteTags()
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        musicFile->removeTag();
    }
}

void MainWindowController::insertAlbumArt(const std::string& pathToImage)
{
    TagLib::ByteVector byteVector{ MediaHelpers::byteVectorFromFile(pathToImage) };
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        musicFile->setAlbumArt(byteVector);
    }
}

void MainWindowController::removeAlbumArt()
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        musicFile->setAlbumArt({});
    }
}

void MainWindowController::filenameToTag(const std::string& formatString)
{
    int success{ 0 };
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        if(musicFile->filenameToTag(formatString))
        {
            success++;
        }
    }
    m_sendToastCallback("Converted " + std::to_string(success) + " filenames to tags successfully.");
}

void MainWindowController::tagToFilename(const std::string& formatString)
{
    int success{ 0 };
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        if(musicFile->tagToFilename(formatString))
        {
            success++;
        }
    }
    m_sendToastCallback("Converted " + std::to_string(success) + " tags to filenames successfully.");
}

void MainWindowController::downloadMusicBrainzMetadata()
{
    int successful{ 0 };
    for(size_t i = 0; i < m_selectedMusicFiles.size(); i += 3)
    {
        std::vector<std::future<bool>> futures;
        if(i < m_selectedMusicFiles.size())
        {
            const std::shared_ptr<MusicFile>& musicFile{ m_selectedMusicFiles[i] };
            futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getOverwriteTagWithMusicBrainz()); }));
        }
        if(i + 1 < m_selectedMusicFiles.size())
        {
            const std::shared_ptr<MusicFile>& musicFile{ m_selectedMusicFiles[i + 1] };
            futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getOverwriteTagWithMusicBrainz()); }));
        }
        if(i + 2 < m_selectedMusicFiles.size())
        {
            const std::shared_ptr<MusicFile>& musicFile{ m_selectedMusicFiles[i + 2] };
            futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getOverwriteTagWithMusicBrainz()); }));
        }
        size_t done{ 0 };
        while(done != futures.size())
        {
            done = 0;
            for(const std::future<bool>& future : futures)
            {
                std::future_status status{ future.wait_for(std::chrono::milliseconds(100)) };
                if(status == std::future_status::ready)
                {
                    done++;
                }
            }
        }
        for(std::future<bool>& future : futures)
        {
            if(future.get())
            {
                successful++;
            }
        }
    }
    m_sendToastCallback("Download metadata for " + std::to_string(successful) + " files successfully.");
}

void MainWindowController::submitToAcoustId(const std::string& musicBrainzRecordingId)
{
    if(m_selectedMusicFiles.size() == 1)
    {
        const std::shared_ptr<MusicFile> selectedMusicFile{ m_selectedMusicFiles[0] };
        m_sendToastCallback(selectedMusicFile->submitToAcoustId(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getAcoustIdUserAPIKey(), musicBrainzRecordingId) ? "Submitted metadata to AcoustId successfully." : "Unable to submit metadata to AcoustId.");
    }
}

void MainWindowController::registerMusicFolderUpdatedCallback(const std::function<void(bool)>& callback)
{
    m_musicFolderUpdatedCallback = callback;
}

const std::vector<std::shared_ptr<MusicFile>>& MainWindowController::getSelectedMusicFiles() const
{
    return m_selectedMusicFiles;
}

TagMap MainWindowController::getSelectedTagMap() const
{
    TagMap tagMap;
    if(m_selectedMusicFiles.size() == 0)
    {
        tagMap.setFilename("");
        tagMap.setTitle("");
        tagMap.setArtist("");
        tagMap.setAlbum("");
        tagMap.setYear("");
        tagMap.setTrack("");
        tagMap.setAlbumArtist("");
        tagMap.setGenre("");
        tagMap.setComment("");
        tagMap.setDuration("00:00:00");
        tagMap.setFingerprint("");
        tagMap.setFileSize("0 MB");
        tagMap.setAlbumArt("");
    }
    else if(m_selectedMusicFiles.size() == 1)
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles[0] };
        tagMap.setFilename(firstMusicFile->getFilename());
        tagMap.setTitle(firstMusicFile->getTitle());
        tagMap.setArtist(firstMusicFile->getArtist());
        tagMap.setAlbum(firstMusicFile->getAlbum());
        tagMap.setYear(std::to_string(firstMusicFile->getYear()));
        tagMap.setTrack(std::to_string(firstMusicFile->getTrack()));
        tagMap.setAlbumArtist(firstMusicFile->getAlbumArtist());
        tagMap.setGenre(firstMusicFile->getGenre());
        tagMap.setComment(firstMusicFile->getComment());
        tagMap.setDuration(firstMusicFile->getDurationAsString());
        tagMap.setFingerprint(firstMusicFile->getChromaprintFingerprint());
        tagMap.setFileSize(firstMusicFile->getFileSizeAsString());
        tagMap.setAlbumArt(firstMusicFile->getAlbumArt().isEmpty() ? "noArt" : "hasArt");
    }
    else
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles[0] };
        bool haveSameTitle{ true };
        bool haveSameArtist{ true };
        bool haveSameAlbum{ true };
        bool haveSameYear{ true };
        bool haveSameTrack{ true };
        bool haveSameAlbumArtist{ true };
        bool haveSameGenre{ true };
        bool haveSameComment{ true };
        bool haveSameAlbumArt{ true };
        int totalDuration{ 0 };
        std::uintmax_t totalFileSize{ 0 };
        for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
        {
            if (firstMusicFile->getTitle() != musicFile->getTitle())
            {
                haveSameTitle = false;
            }
            if (firstMusicFile->getArtist() != musicFile->getArtist())
            {
                haveSameArtist = false;
            }
            if (firstMusicFile->getAlbum() != musicFile->getAlbum())
            {
                haveSameAlbum = false;
            }
            if (firstMusicFile->getYear() != musicFile->getYear())
            {
                haveSameYear = false;
            }
            if (firstMusicFile->getTrack() != musicFile->getTrack())
            {
                haveSameTrack = false;
            }
            if (firstMusicFile->getAlbumArtist() != musicFile->getAlbumArtist())
            {
                haveSameAlbumArtist = false;
            }
            if (firstMusicFile->getGenre() != musicFile->getGenre())
            {
                haveSameGenre = false;
            }
            if (firstMusicFile->getComment() != musicFile->getComment())
            {
                haveSameComment = false;
            }
            if  (firstMusicFile->getAlbumArt() != musicFile->getAlbumArt())
            {
                haveSameAlbumArt = false;
            }
            totalDuration += musicFile->getDuration();
            totalFileSize += musicFile->getFileSize();
        }
        tagMap.setFilename("<keep>");
        tagMap.setTitle(haveSameTitle ? firstMusicFile->getTitle() : "<keep>");
        tagMap.setArtist(haveSameArtist ? firstMusicFile->getArtist() : "<keep>");
        tagMap.setAlbum(haveSameAlbum ? firstMusicFile->getAlbum() : "<keep>");
        tagMap.setYear(haveSameYear ? std::to_string(firstMusicFile->getYear()) : "<keep>");
        tagMap.setTrack(haveSameTrack ? std::to_string(firstMusicFile->getTrack()) : "<keep>");
        tagMap.setAlbumArtist(haveSameAlbumArtist ? firstMusicFile->getAlbumArtist() : "<keep>");
        tagMap.setGenre(haveSameGenre ? firstMusicFile->getGenre() : "<keep>");
        tagMap.setComment(haveSameComment ? firstMusicFile->getComment() : "<keep>");
        tagMap.setDuration(MediaHelpers::durationToString(totalDuration));
        tagMap.setFingerprint("<keep>");
        tagMap.setFileSize(MediaHelpers::fileSizeToString(totalFileSize));
        if(haveSameAlbumArt)
        {
            tagMap.setAlbumArt(firstMusicFile->getAlbumArt().isEmpty() ? "noArt" : "hasArt");
        }
        else
        {
            tagMap.setAlbumArt("keepArt");
        }
    }
    return tagMap;
}

void MainWindowController::updateSelectedMusicFiles(std::vector<int> indexes)
{
    m_selectedMusicFiles.clear();
    for(int index : indexes)
    {
        m_selectedMusicFiles.push_back(m_musicFolder.getMusicFiles()[index]);
    }
}

bool MainWindowController::checkIfAcoustIdUserAPIKeyValid()
{
    return AcoustIdSubmission::checkIfUserAPIKeyValid(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getAcoustIdUserAPIKey());
}

