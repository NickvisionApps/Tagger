#include "musicbrainzreleasequery.hpp"
#include <fstream>
#include <sstream>
#include <thread>
#include <adwaita.h>
#include <curlpp/Easy.hpp>
#include <curlpp/Infos.hpp>
#include <curlpp/Options.hpp>
#include <json/json.h>
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int MusicBrainzReleaseQuery::m_requestCount = 0;
std::chrono::time_point<std::chrono::system_clock> MusicBrainzReleaseQuery::m_lastRequestTime = std::chrono::system_clock::now();

MusicBrainzReleaseQuery::MusicBrainzReleaseQuery(const std::string& releaseId) : m_releaseId{ releaseId }, m_lookupUrlAlbumArt{ "https://coverartarchive.org/release/" + m_releaseId }, m_status{ MusicBrainzReleaseQueryStatus::MusicBrainzError }, m_title{ "" }, m_artist{ "" }
{
    std::stringstream builder;
    builder << "https://musicbrainz.org/ws/2/release/" << m_releaseId << "?";
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

const TagLib::ByteVector& MusicBrainzReleaseQuery::getAlbumArt() const
{
    return m_albumArt;
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
    //Get Album Art
    response.str("");
    handle.setOpt(cURLpp::Options::Url(m_lookupUrlAlbumArt));
    handle.setOpt(cURLpp::Options::WriteStream(&response));
    try
    {
        handle.perform();
    }
    catch(...)
    {
        m_status = MusicBrainzReleaseQueryStatus::CurlError;
        return m_status;
    }
    //Download Album Art Image Url
    if(response.str().substr(0, 1) == "{")
    {
        Json::Value jsonAlbumArt;
        response >> jsonAlbumArt;
        const Json::Value& jsonFirstAlbumArt{ jsonAlbumArt["images"][0] };
        if(!jsonFirstAlbumArt.isNull())
        {
            std::string albumArtLink{ jsonFirstAlbumArt.get("image", "").asString() };
            std::string pathAlbumArt{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionTagger/" + m_releaseId + ".jpg" };
            std::ofstream fileAlbumArt{ pathAlbumArt };
            if(!fileAlbumArt.is_open())
            {
                m_status = MusicBrainzReleaseQueryStatus::FileError;
                return m_status;
            }
            handle.setOpt(cURLpp::Options::Url(albumArtLink));
            handle.setOpt(cURLpp::Options::WriteStream(&fileAlbumArt));
            try
            {
                handle.perform();
            }
            catch(...)
            {
                m_status = MusicBrainzReleaseQueryStatus::CurlError;
                return m_status;
            }
            fileAlbumArt.close();
            //Extract Album Art
            m_albumArt = MediaHelpers::byteVectorFromFile(pathAlbumArt);
        }
    }
    //Done
    m_status = MusicBrainzReleaseQueryStatus::OK;
    return m_status;
}



