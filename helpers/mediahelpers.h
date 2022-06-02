#pragma once

#include <string>
#include <filesystem>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Helpers::MediaHelpers
{
    unsigned int stoui(const std::string& str, size_t* idx = 0, int base = 10);
    std::string durationToString(int durationInSeconds);
    std::string fileSizeToString(std::uintmax_t fileSize);
    TagLib::ByteVector byteVectorFromFile(const std::filesystem::path& path);
}