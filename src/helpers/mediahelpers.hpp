#pragma once

#include <string>
#include <filesystem>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Helpers::MediaHelpers
{
    /**
     * Converts a string to an unsigned int
     *
     * @param str The string to convert
     * @param idx size_t*
     * @param base int
     * @returns The unsigned int version of the string
     */
    unsigned int stoui(const std::string& str, size_t* idx = nullptr, int base = 10);
    /**
     * Converts a duration to a human-readable string (hh:mm:ss)
     *
     * @param durationInSections The duration to convert in seconds
     * @returns A human-readable version of the duration
     */
    std::string durationToString(int durationInSeconds);
    /**
     * Converts a file size to a human-readable string (0 MB)
     *
     * @param fileSize The file size in bytes
     * @returns A human-readable version of the file size
     */
    std::string fileSizeToString(std::uintmax_t fileSize);
    /**
     * Creates a TagLib::ByteVector from an image on disk
     *
     * @param path The path to the image file
     * @returns The TagLib::ByteVector for the provided file
     */
    TagLib::ByteVector byteVectorFromFile(const std::filesystem::path& path);
}