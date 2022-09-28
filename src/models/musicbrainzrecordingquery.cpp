#include "musicbrainzrecordingquery.hpp"
#include <sstream>
#include <thread>
#include <curlpp/cURLpp.hpp>
#include <curlpp/Easy.hpp>
#include <curlpp/Options.hpp>
#include <json/json.h>

using namespace NickvisionTagger::Models;

int MusicBrainzRecordingQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> MusicBrainzRecordingQuery::m_lastRequestTime = std::chrono::system_clock::now();

MusicBrainzRecordingQuery::MusicBrainzRecordingQuery(const std::string& recordingId) : m_status{ MusicBrainzRecordingQueryStatus::Error }
{
    std::stringstream builder;
    builder << "https://musicbrainz.org/ws/2/recording/" << recordingId << "?";
    builder << "inc=" << "artists+releases" << "&";
    builder << "fmt=" << "json";
    m_lookupUrl = builder.str();
}

MusicBrainzRecordingQueryStatus MusicBrainzRecordingQuery::getStatus() const
{
    return m_status;
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
        m_status = MusicBrainzRecordingQueryStatus::CurlError;
        return m_status;
    }
    m_requestCount++;
    m_lastRequestTime = std::chrono::system_clock::now();
    //Parse Response
    Json::Value json;
    response >> json;
    std::cout << json;
    return m_status;
}


