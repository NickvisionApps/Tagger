#include "acoustidquery.hpp"
#include <sstream>
#include <thread>
#include <curlpp/cURLpp.hpp>
#include <curlpp/Easy.hpp>
#include <curlpp/Options.hpp>
#include <json/json.h>

using namespace NickvisionTagger::Models;

int AcoustIdQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> AcoustIdQuery::m_lastRequestTime = std::chrono::system_clock::now();

AcoustIdQuery::AcoustIdQuery(int duration, const std::string& fingerprint) : m_status{ AcoustIdQueryStatus::Error }, m_recordingId{ "" }
{
    std::stringstream builder;
    builder << "https://api.acoustid.org/v2/lookup?";
    builder << "client=" << "Lz9ENGSGsX" << "&";
    builder << "duration=" << duration << "&";
    builder << "meta=" << "recordingids" << "&";
    builder << "fingerprint=" << fingerprint;
    m_lookupUrl = builder.str();
}

AcoustIdQueryStatus AcoustIdQuery::getStatus() const
{
    return m_status;
}

std::string AcoustIdQuery::getRecordingId() const
{
    return m_status == AcoustIdQueryStatus::OK ? m_recordingId : "";
}

AcoustIdQueryStatus AcoustIdQuery::lookup()
{
    //AcoustId has rate limit of 3 requests/second
    if(m_requestCount == 3)
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
    try
    {
        handle.perform();
    }
    catch(...)
    {
        m_status = AcoustIdQueryStatus::CurlError;
        return m_status;
    }
    m_requestCount++;
    m_lastRequestTime = std::chrono::system_clock::now();
    //Parse Response
    Json::Value json;
    response >> json;
    try
    {
    	m_status = json.get("status", "error").asString() == "ok" ? AcoustIdQueryStatus::OK : AcoustIdQueryStatus::AcoustIdError;
    }
    catch(...)
    {
        m_status = AcoustIdQueryStatus::JsonError;
    	return m_status;
    }
    const Json::Value& bestResult{ json["results"][0] };
    if(bestResult.isNull())
    {
        m_status = AcoustIdQueryStatus::NoResult;
        return m_status;
    }
    else
    {
        const Json::Value& firstRecording{ bestResult["recordings"][0] };
        if(firstRecording.isNull())
        {
            m_status = AcoustIdQueryStatus::NoResult;
            return m_status;
        }
        else
        {
            m_recordingId = firstRecording.get("id", "").asString();
            if(m_recordingId.empty())
            {
                m_status = AcoustIdQueryStatus::NoResult;
            }
        }
    }
    return m_status;
}


