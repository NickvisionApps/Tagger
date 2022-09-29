#include "musicbrainzreleasequery.hpp"
#include <sstream>
#include <thread>
#include <curlpp/cURLpp.hpp>
#include <curlpp/Easy.hpp>
#include <curlpp/Options.hpp>
#include <json/json.h>

using namespace NickvisionTagger::Models;

int MusicBrainzReleaseQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> MusicBrainzReleaseQuery::m_lastRequestTime = std::chrono::system_clock::now();

MusicBrainzReleaseQuery::MusicBrainzReleaseQuery(const std::string& releaseId) : m_status{ MusicBrainzReleaseQueryStatus::MusicBrainzError }, m_title{ "" }, m_artist{ "" }
{
    std::stringstream builder;
    builder << "https://musicbrainz.org/ws/2/release/" << releaseId << "?";
    builder << "inc=" << "artists" << "&";
    builder << "fmt=" << "json";
    m_lookupUrl = builder.str();
}

MusicBrainzReleaseQueryStatus MusicBrainzReleaseQuery::getStatus() const
{
    return m_status;
}

const std::string& MusicBrainzReleaseQuery::getTitle() const
{
    return m_title;
}

const std::string& MusicBrainzReleaseQuery::getArtist() const
{
    return m_artist;
}

MusicBrainzReleaseQueryStatus MusicBrainzReleaseQuery::lookup()
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
    std::stringstream response;
    cURLpp::Cleanup cleanup;
    cURLpp::Easy handle;
    handle.setOpt(cURLpp::Options::Url(m_lookupUrl));
    handle.setOpt(cURLpp::Options::FollowLocation(true));
    handle.setOpt(cURLpp::Options::HttpGet(true));
    handle.setOpt(cURLpp::Options::WriteStream(&response));
    handle.setOpt(cURLpp::Options::UserAgent("NickvisionTagger/2022.9.2 ( nlogozzo225@gmail.com )"));
    try
    {
        handle.perform();
    }
    catch(...)
    {
        m_status = MusicBrainzReleaseQueryStatus::CurlError;
        return m_status;
    }
    m_requestCount++;
    m_lastRequestTime = std::chrono::system_clock::now();
    //Parse Response
    Json::Value jsonRoot;
    response >> jsonRoot;
    if(!jsonRoot["error"].isNull())
    {
        m_status = MusicBrainzReleaseQueryStatus::MusicBrainzError;
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
    //Done
    m_status = MusicBrainzReleaseQueryStatus::OK;
    return m_status;
}


