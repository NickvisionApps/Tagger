#include "acoustidsubmission.hpp"
#include <json/json.h>
#include "../helpers/curlhelpers.hpp"
#include "../helpers/jsonhelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

AcoustIdSubmission::AcoustIdSubmission(const std::string& clientAPIKey, const std::string& userAPIKey, int duration, const std::string& fingerprint) : m_lookupUrl{ "https://api.acoustid.org/v2/submit?client=" + clientAPIKey + "&user=" + userAPIKey + "&wait=3&&duration.0=" + std::to_string(duration) + "&fingerprint.0=" + fingerprint }
{

}

bool AcoustIdSubmission::checkIfUserAPIKeyValid(const std::string& clientAPIKey, const std::string& userAPIKey)
{
    if(userAPIKey.empty())
    {
        return false;
    }
    std::string checkQueryUrl{ "https://api.acoustid.org/v2/submit?client=" + clientAPIKey + "&user=" + userAPIKey };
    std::string response{ CurlHelpers::getResponseString(checkQueryUrl) };
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    const Json::Value& jsonError{ jsonRoot["error"] };
    if(jsonError.isNull())
    {
        return false;
    }
    int errorCode{ jsonError.get("code", 6).asInt() };
    return errorCode != 6;
}

bool AcoustIdSubmission::submitMusicBrainzRecordingId(const std::string& musicBrainzRecordingId)
{
    m_lookupUrl += "&mbid.0=" + musicBrainzRecordingId;
    return submit();
}

bool AcoustIdSubmission::submitTagMetadata(const TagMap& tagMap)
{
    if(!tagMap.getTitle().empty())
    {
        m_lookupUrl += "&title.0=" + tagMap.getTitle();
    }
    if(!tagMap.getArtist().empty())
    {
        m_lookupUrl += "&artist.0=" + tagMap.getArtist();
    }
    if(!tagMap.getAlbum().empty())
    {
        m_lookupUrl += "&album.0=" + tagMap.getAlbum();
    }
    if(tagMap.getYear() != "0")
    {
        m_lookupUrl += "&year.0=" + tagMap.getYear();
    }
    if(tagMap.getTrack() != "0")
    {
        m_lookupUrl += "&track.0=" + tagMap.getTrack();
    }
    if(!tagMap.getAlbumArtist().empty())
    {
        m_lookupUrl += "&albumArtist.0=" + tagMap.getAlbumArtist();
    }
    return submit();
}

bool AcoustIdSubmission::submit()
{
    std::string response{ CurlHelpers::getResponseString(m_lookupUrl) };
    //Parse Response
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    if(jsonRoot.get("status", "error").asString() != "ok")
    {
    	return false;
    }
    //Get First Submission
    Json::Value& jsonFirstResult{ jsonRoot["submissions"][0] };
    if(jsonFirstResult.isNull())
    {
        return false;
    }
    return true;
} 



