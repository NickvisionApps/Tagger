#include "mainwindowcontroller.hpp"
#include <algorithm>
#include <chrono>
#include <filesystem>
#include <future>
#include <iterator>
#include <curlpp/cURLpp.hpp>
#include "../helpers/mediahelpers.hpp"
#include "../models/acoustidsubmission.hpp"

using namespace NickvisionTagger::Controllers;
using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

std::vector<std::string> split(const std::string& s, const std::string& delim)
{
    std::vector<std::string> result;
    size_t last{ 0 };
    size_t next{ s.find(delim) };
    while(next != std::string::npos)
    {
        result.push_back(s.substr(last, next - last));
        last = next + delim.length();
        next = s.find(delim, last);
    }
    result.push_back(s.substr(last));
    return result;
}

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

bool MainWindowController::getCanClose() const
{
    for(bool saved : m_musicFilesSaved)
    {
        if(!saved)
        {
            return false;
        }
    }
    return true;
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

void MainWindowController::registerMusicFolderUpdatedCallback(const std::function<void(bool)>& callback)
{
    m_musicFolderUpdatedCallback = callback;
}

const std::vector<bool>& MainWindowController::getMusicFilesSaved() const
{
    return m_musicFilesSaved;
}

void MainWindowController::registerMusicFilesSavedUpdatedCallback(const std::function<void()>& callback)
{
    m_musicFilesSavedUpdatedCallback = callback;
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
    m_musicFilesSaved.clear();
    m_musicFolder.reloadMusicFiles();
    for(size_t i = 0; i < m_musicFolder.getMusicFiles().size(); i++)
    {
        m_musicFilesSaved.push_back(true);
    }
}

void MainWindowController::updateTags(const TagMap& tagMap)
{
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        bool updated{ false };
        if(tagMap.getFilename() != pair.second->getFilename() && tagMap.getFilename() != "<keep>")
        {
            pair.second->setFilename(tagMap.getFilename());
            updated = true;
        }
        if(tagMap.getTitle() != pair.second->getTitle() && tagMap.getTitle() != "<keep>")
        {
            pair.second->setTitle(tagMap.getTitle());
            updated = true;
        }
        if(tagMap.getArtist() != pair.second->getArtist() && tagMap.getArtist() != "<keep>")
        {
            pair.second->setArtist(tagMap.getArtist());
            updated = true;
        }
        if(tagMap.getAlbum() != pair.second->getAlbum() && tagMap.getAlbum() != "<keep>")
        {
            pair.second->setAlbum(tagMap.getAlbum());
            updated = true;
        }
        if(tagMap.getYear() != std::to_string(pair.second->getYear()) && tagMap.getYear() != "<keep>")
        {
            try
            {
                pair.second->setYear(MediaHelpers::stoui(tagMap.getYear()));
                updated = true;
            }
            catch(...) { }
        }
        if(tagMap.getTrack() != std::to_string(pair.second->getTrack()) && tagMap.getTrack() != "<keep>")
        {
            try
            {
                pair.second->setTrack(MediaHelpers::stoui(tagMap.getTrack()));
                updated = true;
            }
            catch(...) { }
        }
        if(tagMap.getAlbumArtist() != pair.second->getAlbumArtist() && tagMap.getAlbumArtist() != "<keep>")
        {
            pair.second->setAlbumArtist(tagMap.getAlbumArtist());
            updated = true;
        }
        if(tagMap.getGenre() != pair.second->getGenre() && tagMap.getGenre() != "<keep>")
        {
            pair.second->setGenre(tagMap.getGenre());
            updated = true;
        }
        if(tagMap.getComment() != pair.second->getComment() && tagMap.getComment() != "<keep>")
        {
            pair.second->setComment(tagMap.getComment());
            updated = true;
        }
        m_musicFilesSaved[pair.first] = !updated;
    }
    m_musicFilesSavedUpdatedCallback();
}

void MainWindowController::saveTags()
{
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        pair.second->saveTag(m_configuration.getPreserveModificationTimeStamp());
        m_musicFilesSaved[pair.first] = true;
    }
    m_musicFilesSavedUpdatedCallback();
    m_sendToastCallback("Tags saved successfully.");
}

void MainWindowController::discardUnappliedChanges()
{
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        pair.second->loadFromDisk();
        m_musicFilesSaved[pair.first] = true;
    }
    m_musicFilesSavedUpdatedCallback();
}

void MainWindowController::deleteTags()
{
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        pair.second->removeTag();
        m_musicFilesSaved[pair.first] = false;
    }
    m_musicFilesSavedUpdatedCallback();
}

void MainWindowController::insertAlbumArt(const std::string& pathToImage)
{
    TagLib::ByteVector byteVector{ MediaHelpers::byteVectorFromFile(pathToImage) };
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        pair.second->setAlbumArt(byteVector);
        m_musicFilesSaved[pair.first] = false;
    }
    m_musicFilesSavedUpdatedCallback();
}

void MainWindowController::removeAlbumArt()
{
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        pair.second->setAlbumArt({});
        m_musicFilesSaved[pair.first] = false;
    }
    m_musicFilesSavedUpdatedCallback();
}

void MainWindowController::filenameToTag(const std::string& formatString)
{
    int success{ 0 };
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        if(pair.second->filenameToTag(formatString))
        {
            success++;
            m_musicFilesSaved[pair.first] = false;
        }
    }
    m_musicFilesSavedUpdatedCallback();
    m_sendToastCallback("Converted " + std::to_string(success) + " filenames to tags successfully.");
}

void MainWindowController::tagToFilename(const std::string& formatString)
{
    int success{ 0 };
    for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
    {
        if(pair.second->tagToFilename(formatString))
        {
            success++;
            m_musicFilesSaved[pair.first] = false;
        }
    }
    m_musicFilesSavedUpdatedCallback();
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
            const std::shared_ptr<MusicFile>& musicFile{ std::next(m_selectedMusicFiles.begin(), i)->second };
            futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getOverwriteTagWithMusicBrainz()); }));
        }
        if(i + 1 < m_selectedMusicFiles.size())
        {
            const std::shared_ptr<MusicFile>& musicFile{ std::next(m_selectedMusicFiles.begin(), i + 1)->second };
            futures.push_back(std::async(std::launch::async, [&, musicFile]() -> bool { return musicFile->downloadMusicBrainzMetadata(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getOverwriteTagWithMusicBrainz()); }));
        }
        if(i + 2 < m_selectedMusicFiles.size())
        {
            const std::shared_ptr<MusicFile>& musicFile{ std::next(m_selectedMusicFiles.begin(), i + 2)->second };
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
        size_t j{ i };
        for(std::future<bool>& future : futures)
        {
            if(j >= i + 3)
            {
                j = i;
            }
            if(future.get())
            {
                successful++;
                m_musicFilesSaved[std::next(m_selectedMusicFiles.begin(), j)->first] = false;
            }
            j++;
        }
    }
    m_musicFilesSavedUpdatedCallback();
    m_sendToastCallback("Download metadata for " + std::to_string(successful) + " files successfully.");
}

bool MainWindowController::checkIfAcoustIdUserAPIKeyValid()
{
    return AcoustIdSubmission::checkIfUserAPIKeyValid(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getAcoustIdUserAPIKey());
}

void MainWindowController::submitToAcoustId(const std::string& musicBrainzRecordingId)
{
    if(m_selectedMusicFiles.size() == 1)
    {
        const std::shared_ptr<MusicFile> selectedMusicFile{ m_selectedMusicFiles.begin()->second };
        m_sendToastCallback(selectedMusicFile->submitToAcoustId(m_appInfo.getAcoustIdClientAPIKey(), m_configuration.getAcoustIdUserAPIKey(), musicBrainzRecordingId) ? "Submitted metadata to AcoustId successfully." : "Unable to submit metadata to AcoustId.");
    }
}

bool MainWindowController::checkIfAdvancedSearchStringValid(const std::string& search)
{
    //!prop1="value1";prop2="value2"
    if(search.substr(0, 1) != "!")
    {
        return false;
    }
    std::string s{ search.substr(1) };
    if(s.empty())
    {
        return false;
    }
    std::vector<std::string> splitProperties{ split(s, ";") };
    std::vector<std::string> properties{ "filename", "title", "artist", "album", "year", "track", "albumartist", "genre", "comment" };
    for(const std::string& property : splitProperties)
    {
        std::vector<std::string> fields{ split(property, "=") };
        if(fields.size() != 2)
        {
            return false;
        }
        if(std::find(properties.begin(), properties.end(), fields[0]) == properties.end())
        {
            return false;
        }
        if(fields[1].length() <= 1 || fields[1].substr(0, 1) != "\"" || fields[1].substr(fields[1].length() - 1) != "\"")
        {
            return false;
        }
    }
    return true;
}

std::pair<bool, std::vector<std::string>> MainWindowController::advancedSearch(const std::string& search)
{
    if(!checkIfAdvancedSearchStringValid(search))
    {
        return { false, {} };
    }
    std::vector<std::string> matches;
    return { true, matches };
}

size_t MainWindowController::getSelectedMusicFilesCount() const
{
    return m_selectedMusicFiles.size();
}

const std::shared_ptr<NickvisionTagger::Models::MusicFile>& MainWindowController::getFirstSelectedMusicFile() const
{
    return m_selectedMusicFiles.begin()->second;
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
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles.begin()->second };
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
        const std::shared_ptr<MusicFile>& firstMusicFile{ m_selectedMusicFiles.begin()->second };
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
        for(const std::pair<const int, std::shared_ptr<MusicFile>>& pair : m_selectedMusicFiles)
        {
            if (firstMusicFile->getTitle() != pair.second->getTitle())
            {
                haveSameTitle = false;
            }
            if (firstMusicFile->getArtist() != pair.second->getArtist())
            {
                haveSameArtist = false;
            }
            if (firstMusicFile->getAlbum() != pair.second->getAlbum())
            {
                haveSameAlbum = false;
            }
            if (firstMusicFile->getYear() != pair.second->getYear())
            {
                haveSameYear = false;
            }
            if (firstMusicFile->getTrack() != pair.second->getTrack())
            {
                haveSameTrack = false;
            }
            if (firstMusicFile->getAlbumArtist() != pair.second->getAlbumArtist())
            {
                haveSameAlbumArtist = false;
            }
            if (firstMusicFile->getGenre() != pair.second->getGenre())
            {
                haveSameGenre = false;
            }
            if (firstMusicFile->getComment() != pair.second->getComment())
            {
                haveSameComment = false;
            }
            if  (firstMusicFile->getAlbumArt() != pair.second->getAlbumArt())
            {
                haveSameAlbumArt = false;
            }
            totalDuration += pair.second->getDuration();
            totalFileSize += pair.second->getFileSize();
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
        m_selectedMusicFiles.insert({ index, m_musicFolder.getMusicFiles()[index] });
    }
}
