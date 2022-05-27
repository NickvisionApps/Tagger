#pragma once

#include <string>

namespace NickvisionTagger::Helpers::CurlHelpers
{
    bool downloadFile(const std::string& url, const std::string& path);
}