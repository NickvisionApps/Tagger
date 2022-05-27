#pragma once

#include <string>

namespace NickvisionTagger::Helpers::MediaHelpers
{
    std::string durationToString(int durationInSeconds);
    std::string fileSizeToString(std::uintmax_t fileSize);
    unsigned int stoui(const std::string& str, size_t* idx = 0, int base = 10);
}