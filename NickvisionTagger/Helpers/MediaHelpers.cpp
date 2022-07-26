#include "MediaHelpers.h"
#include <climits>
#include <cmath>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <stdexcept>
#include <vector>

namespace NickvisionTagger::Helpers
{
    unsigned int MediaHelpers::stoui(const std::string& str, size_t* idx, int base)
    {
        unsigned long ui = std::stoul(str, idx, base);
        if (ui > UINT_MAX)
        {
            throw std::out_of_range(str);
        }
        return ui;
    }

    std::string MediaHelpers::durationToString(int durationInSeconds)
    {
        std::stringstream builder;
        int seconds = durationInSeconds % 60;
        durationInSeconds /= 60;
        int minutes = durationInSeconds % 60;
        int hours = durationInSeconds / 60;
        builder << std::setw(2) << std::setfill('0') << hours << ":";
        builder << std::setw(2) << std::setfill('0') << minutes << ":";
        builder << std::setw(2) << std::setfill('0') << seconds;
        return builder.str();
    }

    std::string MediaHelpers::fileSizeToString(std::uintmax_t fileSize)
    {
        std::vector<std::string> sizes{ "B", "KB", "MB", "GB", "TB" };
        double length = fileSize;
        std::size_t index = 0;
        std::stringstream builder;
        while (length >= 1024 && index < 4)
        {
            index++;
            length /= 1024;
        }
        builder << std::ceil(length * 100.0) / 100.0 << " " << sizes[index];
        return builder.str();
    }

    TagLib::ByteVector MediaHelpers::byteVectorFromFile(const std::filesystem::path& path)
    {
        std::ifstream pictureFile{ path, std::ios::binary };
        std::stringstream builder;
        builder << pictureFile.rdbuf();
        std::string data{ builder.str() };
        return TagLib::ByteVector::fromCString(data.c_str(), data.size());
    }
}