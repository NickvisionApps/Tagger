#include "acoustidquery.hpp"
#include <sstream>
#include <thread>
#include <json/reader.h>
#include <json/json.h>
#include "../helpers/curlhelpers.hpp"
#include "../helpers/jsonhelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int AcoustIdQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> AcoustIdQuery::m_lastRequestTime = std::chrono::system_clock::now();

AcoustIdQuery::AcoustIdQuery(int duration, const std::string& fingerprint) : m_status{ AcoustIdQueryStatus::AcoustIdError }, m_recordingId{ "" }
{
    std::stringstream builder;
    builder << "https://api.acoustid.org/v2/lookup?";
    builder << "client=" << "Lz9ENGSGsX" << "&";
    builder << "duration=" << duration << "&";
    builder << "meta=" << "recordings+releasegroups" << "&";
    builder << "fingerprint=" << fingerprint;
    m_lookupUrl = builder.str();
}

AcoustIdQueryStatus AcoustIdQuery::getStatus() const
{
    return m_status;
}

const std::string& AcoustIdQuery::getRecordingId() const
{
    return m_recordingId;
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
    std::string response{ CurlHelpers::getResponseString(m_lookupUrl) };
    m_requestCount++;
    m_lastRequestTime = std::chrono::system_clock::now();
    if(response.empty())
    {
        m_status = AcoustIdQueryStatus::CurlError;
        return m_status;
    }
    //Parse Response
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    m_status = jsonRoot.get("status", "error").asString() == "ok" ? AcoustIdQueryStatus::OK : AcoustIdQueryStatus::AcoustIdError;
    if(m_status == AcoustIdQueryStatus::AcoustIdError)
    {
    	return m_status;
    }
    //Get First Result
    Json::Value& jsonFirstResult{ jsonRoot["results"][0] };
    if(jsonFirstResult.isNull())
    {
        m_status = AcoustIdQueryStatus::NoResult;
        return m_status;
    }
    //Get Best Recording Id
    Json::Value& jsonRecordings{ jsonFirstResult["recordings"] };
    Json::Value& jsonBestRecording{ jsonRecordings[0] };
    for(const Json::Value& recording : jsonRecordings)
    {
        if(recording.get("title", "").asString() != "")
        {
            jsonBestRecording = recording;
            break;
        }
    }
    if(jsonBestRecording.get("title", "").asString() == "")
    {
        m_status = AcoustIdQueryStatus::NoResult;
        return m_status;
    }
    m_recordingId = jsonBestRecording.get("id", "").asString();
    return m_status;
}



