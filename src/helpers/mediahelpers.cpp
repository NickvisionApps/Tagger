#include "mediahelpers.hpp"
#include <climits>
#include <cmath>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <vector>

using namespace NickvisionTagger::Helpers;

unsigned int MediaHelpers::stoui(const std::string& str, size_t* idx, int base)
{
    unsigned long ui{ std::stoul(str, idx, base) };
    if (ui > UINT_MAX)
    {
        return UINT_MAX;
    }
    return ui;
}

std::string MediaHelpers::durationToString(int durationInSeconds)
{
    std::stringstream builder;
    int seconds{ durationInSeconds % 60 };
    durationInSeconds /= 60;
    int minutes{ durationInSeconds % 60 };
    int hours{ durationInSeconds / 60 };
    builder << std::setw(2) << std::setfill('0') << hours << ":";
    builder << std::setw(2) << std::setfill('0') << minutes << ":";
    builder << std::setw(2) << std::setfill('0') << seconds;
    return builder.str();
}

std::string MediaHelpers::fileSizeToString(std::uintmax_t fileSize)
{
    std::vector<std::string> sizes{ "B", "KB", "MB", "GB", "TB" };
    int index{ 0 };
    std::stringstream builder;
    while (fileSize >= 1024 && index < 4)
    {
        index++;
        fileSize /= 1024;
    }
    builder << std::ceil(fileSize * 100.0) / 100.0 << " " << sizes[index];
    return builder.str();
}

TagLib::ByteVector MediaHelpers::byteVectorFromFile(const std::filesystem::path& path)
{
    std::ifstream file{ path, std::ios::binary };
    std::stringstream builder;
    builder << file.rdbuf();
    std::string data{ builder.str() };
    return TagLib::ByteVector::fromCString(data.c_str(), data.size());
}
