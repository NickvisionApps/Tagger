#pragma once

#include <string>

namespace NickvisionTagger::Helpers::CurlHelpers
{
    /**
     * Downloads a file from the internet
     *
     * @param url The url of the file to download
     * @param path The path of where to save the file
     *
     * @returns True if download successful, else false
     */
    bool downloadFile(const std::string& url, const std::string& path);
}