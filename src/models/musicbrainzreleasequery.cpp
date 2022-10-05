#include "musicbrainzreleasequery.hpp"
#include <fstream>
#include <thread>
#include <adwaita.h>
#include <json/json.h>
#include "../helpers/curlhelpers.hpp"
#include "../helpers/jsonhelpers.hpp"
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int MusicBrainzReleaseQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> MusicBrainzReleaseQuery::m_lastRequestTime = std::chrono::system_clock::now();

MusicBrainzReleaseQuery::MusicBrainzReleaseQuery(const std::string& releaseId) : m_releaseId{ releaseId }, m_lookupUrl{ "https://musicbrainz.org/ws/2/release/" + m_releaseId + "?inc=artists&fmt=json" }, m_lookupUrlAlbumArt{ "https://coverartarchive.org/release/" + m_releaseId }, m_title{ "" }, m_artist{ "" }
{

}

const std::string& MusicBrainzReleaseQuery::getTitle() const
{
    return m_title;
}

const std::string& MusicBrainzReleaseQuery::getArtist() const
{
    return m_artist;
}

const TagLib::ByteVector& MusicBrainzReleaseQuery::getAlbumArt() const
{
    return m_albumArt;
}

bool MusicBrainzReleaseQuery::lookup()
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
        return false;
    }
    //Parse Response
    Json::Value jsonRoot{ JsonHelpers::getValueFromString(response) };
    if(!jsonRoot["error"].isNull())
    {
        return false;
    }
    //Get Title
    m_title = jsonRoot.get("title", "").asString();
    //Get Artist
    const Json::Value& jsonFirstArtist{ jsonRoot["artist-credit"][0] };
    if(!jsonFirstArtist.isNull())
    {
        m_artist = jsonFirstArtist.get("name", "").asString();
    }
    //Get Album Art
    const Json::Value& jsonCoverArt{ jsonRoot["cover-art-archive"] };
    if(jsonCoverArt.get("count", 0).asInt() > 0)
    {
        response = CurlHelpers::getResponseString(m_lookupUrlAlbumArt);
        if(response.empty())
        {
            return false;
        }
        if(response.substr(0, 1) == "{")
        {
            Json::Value jsonAlbumArt{ JsonHelpers::getValueFromString(response) };
            const Json::Value& jsonFirstAlbumArt{ jsonAlbumArt["images"][0] };
            if(!jsonFirstAlbumArt.isNull())
            {
                std::string albumArtLink{ jsonFirstAlbumArt.get("image", "").asString() };
                std::string pathAlbumArt{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionTagger/" + m_releaseId + ".jpg" };
                if(CurlHelpers::downloadFile(albumArtLink, pathAlbumArt))
                {
                    m_albumArt = MediaHelpers::byteVectorFromFile(pathAlbumArt);
                    std::filesystem::remove(pathAlbumArt);
                }
            }
        }
    }
    //Done
    return true;
}



