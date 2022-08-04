#pragma once

#include <filesystem>
#include <string>
#include <taglib/tbytevector.h>

/// <summary>
/// Functions for working with media files
/// </summary>
namespace NickvisionTagger::Helpers::MediaHelpers
{
    /// <summary>
    /// Converts a string to an unsigned int
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <param name="idx">size_t*</param>
    /// <param name="base">int</param>
    /// <returns>The unsigned int version of the string. Throws std::out_of_range exception if unable to convert</returns>
    unsigned int stoui(const std::string& str, size_t* idx = 0, int base = 10);
    /// <summary>
    /// Converts a duration to a human-readable string
    /// </summary>
    /// <param name="durationInSeconds">The duration in seconds</param>
    /// <returns>A human-readable version of the duration in the format: "hh:mm:ss"</returns>
    std::string durationToString(int durationInSeconds);
    /// <summary>
    /// Converts a file size to a human-readable string
    /// </summary>
    /// <param name="fileSize">The file size</param>
    /// <returns>A human-readable version of the file size</returns>
    std::string fileSizeToString(std::uintmax_t fileSize);
    /// <summary>
    /// Creates a TagLib::ByteVector from a picture file
    /// </summary>
    /// <param name="path">The path to the picture file</param>
    /// <returns>The TagLib::ByteVector of the picture file</returns>
    TagLib::ByteVector getByteVectorFromFile(const std::filesystem::path& path);
}

