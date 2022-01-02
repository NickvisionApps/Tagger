#ifndef MEDIAHELPERS_H
#define MEDIAHELPERS_H

#include <string>

namespace NickvisionTagger::Helpers::MediaHelpers
{
    std::string durationToString(int durationInSeconds);
    std::string fileSizeToString(std::uintmax_t fileSize);
}

#endif // MEDIAHELPERS_H
