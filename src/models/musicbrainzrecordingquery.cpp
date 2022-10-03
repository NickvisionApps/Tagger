#include "musicbrainzrecordingquery.hpp"
#include <thread>
#include <json/json.h>
#include "musicbrainzreleasequery.hpp"
#include "../helpers/curlhelpers.hpp"
#include "../helpers/jsonhelpers.hpp"
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int MusicBrainzRecordingQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> MusicBrainzRecordingQuery::m_lastRequestTime = std::chrono::system_clock::now();

MusicBrainzRecordingQuery::MusicBrainzRecordingQuery(const std::string& recordingId) : m_lookupUrl{ "https://musicbrainz.org/ws/2/recording/" + recordingId + "?inc=artists+releases+genres&fmt=json" }, m_status{ MusicBrainzRecordingQueryStatus::MusicBrainzError }, m_title{ "" }, m_artist{ "" }, m_album{ "" }, m_year{ 0 }, m_albumArtist{ "" }, m_genre{ "" }
{

}

MusicBrainzRecordingQueryStatus MusicBrainzRecordingQuery::getStatus() const
{
    return m_status;
}

const std::string& MusicBrainzRecordingQuery::getTitle() const
{
    return m_title;
}

const std::string& MusicBrainzRecordingQuery::getArtist() const
{
    return m_artist;
}

const std::string& MusicBrainzRecordingQuery::getAlbum() const
{
    return m_album;
}

unsigned int MusicBrainzRecordingQuery::getYear() const
{
    return m_year;
}

const std::string& MusicBrainzRecordingQuery::getAlbumArtist() const
{
    return m_albumArtist;
}

const std::string& MusicBrainzRecordingQuery::getGenre() const
{
    return m_genre;
}

const TagLib::ByteVector& MusicBrainzRecordingQuery::getAlbumArt() const
{
    return m_albumArt;
}

MusicBrainzRecordingQueryStatus MusicBrainzRecordingQuery::lookup()
{
    //MusicBrainz has rate limit of 50 requests/second
    if(m_requestCount == 50)
    {
        if(std::chrono::system_clock::now() - m_lastRequestTime <= std::chrono::seconds(1))
        {
            std::this_thread::sleep_for(std::chrono::seconds(1));
        }
        m_requestCount = 0;
    }
    //Get Json Response from Lookup
    std::string response{ CurlHelpers::getResponseString(m_lookupUrl, "NickvisionTagger/2022.9.2 ( nlogozzo225@gmail.com )") };
    m_requestCount++;
    m_lastRequestTime = std::chrono::system_clock::now();
    if(response.empty())
    {
        m_status = MusicBrainzRecordingQueryStatus::CurlError;
        return m_status;
    }
    //Parse Response
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    if(!jsonRoot["error"].isNull())
    {
        m_status = MusicBrainzRecordingQueryStatus::MusicBrainzError;
        return m_status;
    }
    //Get Title
    m_title = jsonRoot.get("title", "").asString();
    //Get Artist
    const Json::Value& jsonFirstArtist{ jsonRoot["artist-credit"][0] };
    if(!jsonFirstArtist.isNull())
    {
        m_artist = jsonFirstArtist.get("name", "").asString();
    }
    //Get Album
    const Json::Value& jsonFirstRelease{ jsonRoot["releases"][0] };
    if(!jsonFirstRelease.isNull())
    {
        MusicBrainzReleaseQuery releaseQuery{ jsonFirstRelease.get("id", "").asString() };
        if(releaseQuery.lookup() == MusicBrainzReleaseQueryStatus::OK)
        {
            m_album = releaseQuery.getTitle();
            m_albumArtist = releaseQuery.getArtist();
            m_albumArt = releaseQuery.getAlbumArt();
        }
    }
    //Get Year
    try
    {
        m_year = MediaHelpers::stoui(jsonRoot.get("first-release-date", "").asString().substr(0, 4));
    }
    catch(...) {  }
    //Get Genre
    const Json::Value& jsonFirstGenre{ jsonRoot["genres"][0] };
    if(!jsonFirstGenre.isNull())
    {
        m_genre = jsonFirstGenre.get("name", "").asString();
    }
    //Done
    m_status = MusicBrainzRecordingQueryStatus::OK;
    return m_status;
}
