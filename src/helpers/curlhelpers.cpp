#include "curlhelpers.hpp"
#include <fstream>
#include <sstream>
#include <curlpp/Easy.hpp>
#include <curlpp/Options.hpp>

using namespace NickvisionTagger::Helpers;

bool CurlHelpers::downloadFile(const std::string& url, const std::string& savePath, const std::string& userAgent)
{
    std::ofstream file{ savePath };
    if(file.is_open())
    {
        cURLpp::Easy handle;
        handle.setOpt(cURLpp::Options::Url(url));
        handle.setOpt(cURLpp::Options::FollowLocation(true));
        handle.setOpt(cURLpp::Options::WriteStream(&file));
        if(!userAgent.empty())
        {
            handle.setOpt(cURLpp::Options::UserAgent(userAgent));
        }
        try
        {
            handle.perform();
        }
        catch(...)
        {
            return false;
        }
        return true;
    }
    return false;
}

std::string CurlHelpers::getResponseString(const std::string& url, const std::string& userAgent)
{
    std::stringstream response;
    cURLpp::Easy handle;
    handle.setOpt(cURLpp::Options::Url(url));
    handle.setOpt(cURLpp::Options::FollowLocation(true));
    handle.setOpt(cURLpp::Options::HttpGet(true));
    handle.setOpt(cURLpp::Options::WriteStream(&response));
    if(!userAgent.empty())
    {
        handle.setOpt(cURLpp::Options::UserAgent(userAgent));
    }
    try
    {
        handle.perform();
    }
    catch(...)
    {
        return "";
    }
    return response.str();
}
