#include "acoustidquery.hpp"
#include <thread>
#include <json/json.h>
#include "../helpers/curlhelpers.hpp"
#include "../helpers/jsonhelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int AcoustIdQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> AcoustIdQuery::m_lastRequestTime = std::chrono::system_clock::now();

AcoustIdQuery::AcoustIdQuery(const std::string& clientAPIKey, int duration, const std::string& fingerprint) : m_lookupUrl{ "https://api.acoustid.org/v2/lookup?client=" + clientAPIKey + "&duration="  + std::to_string(duration) + "&meta=recordings&fingerprint=" + fingerprint }, m_recordingId{ "" }
{

}

const std::string& AcoustIdQuery::getRecordingId() const
{
    return m_recordingId;
}

bool AcoustIdQuery::lookup()
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
        return false;
    }
    //Parse Response
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    if(jsonRoot.get("status", "error").asString() != "ok")
    {
    	return false;
    }
    //Get First Result
    Json::Value& jsonFirstResult{ jsonRoot["results"][0] };
    if(jsonFirstResult.isNull())
    {
        return false;
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
        return false;
    }
    m_recordingId = jsonBestRecording.get("id", "").asString();
    //Done
    return true;
}
