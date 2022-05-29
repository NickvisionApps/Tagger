#pragma once

#include <string>

namespace NickvisionTagger::Helpers::MediaHelpers
{
    unsigned int stoui(const std::string& str, size_t* idx = 0, int base = 10);
    std::string durationToString(int durationInSeconds);
    std::string fileSizeToString(std::uintmax_t fileSize);
    unsigned int musicBrainzDateToYear(const std::string& date);
}