#include "curlhelpers.hpp"
#include <fstream>
#include <sstream>
#include <curlpp/cURLpp.hpp>
#include <curlpp/Easy.hpp>
#include <curlpp/Options.hpp>

using namespace NickvisionTagger::Helpers;

bool CurlHelpers::downloadFile(const std::string& url, const std::string& path)
{
    std::ofstream fileOut{ path };
    if (fileOut.is_open())
    {
        cURLpp::Cleanup cleanup;
        cURLpp::Easy handle;
        handle.setOpt(cURLpp::Options::Url(url));
        handle.setOpt(cURLpp::Options::FollowLocation(true));
        handle.setOpt(cURLpp::Options::WriteStream(&fileOut));
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

std::string CurlHelpers::getResponse(const std::string& url)
{
    std::stringstream response;
    cURLpp::Cleanup cleanup;
    cURLpp::Easy handle;
    handle.setOpt(cURLpp::Options::Url(url));
    handle.setOpt(cURLpp::Options::FollowLocation(true));
    handle.setOpt(cURLpp::Options::HttpGet(true));
    handle.setOpt(cURLpp::Options::WriteStream(&response));
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
