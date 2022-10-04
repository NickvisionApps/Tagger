#include "acoustidsubmission.hpp"

using namespace NickvisionTagger::Models;

AcoustIdSubmission::AcoustIdSubmission(const std::string& clientAPIKey, const std::string& userAPIKey, int duration, const std::string& fingerprint) : m_lookupUrl{ "https://api.acoustid.org/v2/submit?client=" + clientAPIKey + "&user=" + userAPIKey + "&wait=3&&duration.0=" + std::to_string(duration) + "&fingerprint.0=" + fingerprint }
{

}

bool AcoustIdSubmission::submitMusicBrainzRecordingId(const std::string& musicBrainzRecordingId)
{
    m_lookupUrl += "&mbid.0=" + musicBrainzRecordingId;
    return submit();
}

bool AcoustIdSubmission::submitTagMetadata(const std::unordered_map<std::string, std::string>& tagMap)
{
    if(!tagMap.at("title").empty())
    {
        m_lookupUrl += "&title.0=" + tagMap.at("title");
    }
    if(!tagMap.at("artist").empty())
    {
        m_lookupUrl += "&artist.0=" + tagMap.at("artist");
    }
    if(!tagMap.at("album").empty())
    {
        m_lookupUrl += "&album.0=" + tagMap.at("album");
    }
    if(tagMap.at("year") != "0")
    {
        m_lookupUrl += "&year.0=" + tagMap.at("year");
    }
    if(tagMap.at("track") != "0")
    {
        m_lookupUrl += "&track.0=" + tagMap.at("track");
    }
    if(!tagMap.at("albumArtist").empty())
    {
        m_lookupUrl += "&albumArtist.0=" + tagMap.at("albumArtist");
    }
    return submit();
}

bool AcoustIdSubmission::submit()
{
    return true;
} 
