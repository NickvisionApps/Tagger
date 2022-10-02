#include "mainwindowcontroller.hpp"
#include <chrono>
#include <filesystem>
#include <future>
#include <curlpp/cURLpp.hpp>
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
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
    m_musicFolderUpdatedCallback(true);
}

void MainWindowController::reloadMusicFolder()
{
    m_musicFolder.reloadMusicFiles();
}

void MainWindowController::saveTags(const std::unordered_map<std::string, std::string>& tagMap)
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        if(tagMap.at("filename") != musicFile->getFilename() && tagMap.at("filename") != "<keep>")
        {
            musicFile->setFilename(tagMap.at("filename"));
        }
        if(tagMap.at("title") != "<keep>")
        {
            musicFile->setTitle(tagMap.at("title"));
        }
        if(tagMap.at("artist") != "<keep>")
        {
            musicFile->setArtist(tagMap.at("artist"));
        }
        if(tagMap.at("album") != "<keep>")
        {
            musicFile->setAlbum(tagMap.at("album"));
        }
        if(tagMap.at("year") != "<keep>")
        {
            try
            {
                musicFile->setYear(MediaHelpers::stoui(tagMap.at("year")));
            }
            catch(...) { }
        }
        if(tagMap.at("track") != "<keep>")
        {
            try
            {
                musicFile->setTrack(MediaHelpers::stoui(tagMap.at("track")));
            }
            catch(...) { }
        }
        if(tagMap.at("albumArtist") != "<keep>")
        {
            musicFile->setAlbumArtist(tagMap.at("albumArtist"));
        }
        if(tagMap.at("genre") != "<keep>")
        {
            musicFile->setGenre(tagMap.at("genre"));
        }
        if(tagMap.at("comment") != "<keep>")
        {
            musicFile->setComment(tagMap.at("comment"));
        }
        musicFile->saveTag(m_configuration.getPreserveModificationTimeStamp());
    }
    m_sendToastCallback("Tags saved successfully.");
}

void MainWindowController::deleteTags()
{
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        musicFile->removeTag(m_configuration.getPreserveModificationTimeStamp());
    }
    m_sendToastCallback("Tags removed successfully.");
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
    //Start async
    std::vector<std::future<bool>> futures;
    for(const std::shared_ptr<MusicFile>& musicFile : m_selectedMusicFiles)
    {
        futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_configuration.getOverwriteTagWithMusicBrainz()); }));
    }
    //Check futures until done
    size_t done{ 0 };
    while(done != futures.size())
    {
        done = 0;
        for(const std::future<bool>& future : futures)
        {
            if(future.wait_for(std::chrono::milliseconds(100)) == std::future_status::ready)
            {
                done++;
            }
        }
    }
    //Determine number of successful futures
    for(std::future<bool>& future : futures)
    {
        if(future.get())
        {
            successful++;
        }
    }
    m_sendToastCallback("Download metadata for " + std::to_string(successful) + " files successfully.");
}

void MainWindowController::registerMusicFolderUpdatedCallback(const std::function<void(bool)>& callback)
{
    m_musicFolderUpdatedCallback = callback;
}

const std::vector<std::shared_ptr<MusicFile>>& MainWindowController::getSelectedMusicFiles() const
{
    return m_selectedMusicFiles;
}

std::unordered_map<std::string, std::string> MainWindowController::getSelectedTagMap() const
{
    std::unordered_map<std::string, std::string> tagMap;
    if(m_selectedMusicFiles.size() == 0)
    {
        tagMap.insert({ "filename", "" });
        tagMap.insert({ "title", "" });
        tagMap.insert({ "artist", "" });
        tagMap.insert({ "album", "" });
        tagMap.insert({ "year", "" });
        tagMap.insert({ "track", "" });
        tagMap.insert({ "albumArtist", "" });
        tagMap.insert({ "genre", "" });
        tagMap.insert({ "comment", "" });
        tagMap.insert({ "duration", "00:00:00" });
        tagMap.insert({ "fingerprint", "" });
        tagMap.insert({ "fileSize", "0 MB" });
        tagMap.insert({ "albumArt", "" });
    }
    else if(m_selectedMusicFiles.size() == 1)
    {
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles[0] };
        tagMap.insert({ "filename", firstMusicFile->getFilename() });
        tagMap.insert({ "title", firstMusicFile->getTitle() });
        tagMap.insert({ "artist", firstMusicFile->getArtist() });
        tagMap.insert({ "album", firstMusicFile->getAlbum() });
        tagMap.insert({ "year", std::to_string(firstMusicFile->getYear()) });
        tagMap.insert({ "track", std::to_string(firstMusicFile->getTrack()) });
        tagMap.insert({ "albumArtist", firstMusicFile->getAlbumArtist() });
        tagMap.insert({ "genre", firstMusicFile->getGenre() });
        tagMap.insert({ "comment", firstMusicFile->getComment() });
        tagMap.insert({ "duration", firstMusicFile->getDurationAsString() });
        tagMap.insert({ "fingerprint", firstMusicFile->getChromaprintFingerprint() });
        tagMap.insert({ "fileSize", firstMusicFile->getFileSizeAsString() });
        tagMap.insert({ "albumArt",  firstMusicFile->getAlbumArt().isEmpty() ? "noArt" : "hasArt" });
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
        tagMap.insert({ "filename", "<keep>" });
        tagMap.insert({ "title", haveSameTitle ? firstMusicFile->getTitle() : "<keep>" });
        tagMap.insert({ "artist", haveSameArtist ? firstMusicFile->getArtist() : "<keep>" });
        tagMap.insert({ "album", haveSameAlbum ? firstMusicFile->getAlbum() : "<keep>" });
        tagMap.insert({ "year", haveSameYear ? std::to_string(firstMusicFile->getYear()) : "<keep>" });
        tagMap.insert({ "track", haveSameTrack ? std::to_string(firstMusicFile->getTrack()) : "<keep>" });
        tagMap.insert({ "albumArtist", haveSameAlbumArtist ? firstMusicFile->getAlbumArtist() : "<keep>" });
        tagMap.insert({ "genre", haveSameGenre ? firstMusicFile->getGenre() : "<keep>" });
        tagMap.insert({ "comment", haveSameComment ? firstMusicFile->getComment() : "<keep>" });
        tagMap.insert({ "duration", MediaHelpers::durationToString(totalDuration) });
        tagMap.insert({ "fingerprint", "<keep>" });
        tagMap.insert({ "fileSize", MediaHelpers::fileSizeToString(totalFileSize) });
        if(haveSameAlbumArt)
        {
            tagMap.insert({ "albumArt", firstMusicFile->getAlbumArt().isEmpty() ? "noArt" : "hasArt" });
        }
        else
        {
            tagMap.insert({ "albumArt", "keepArt" });
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

