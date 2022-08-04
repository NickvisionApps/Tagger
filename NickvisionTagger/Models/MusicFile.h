#pragma once

#include <filesystem>
#include <memory>
#include <string>
#include <taglib/asffile.h>
#include <taglib/flacfile.h>
#include <taglib/mpegfile.h>
#include <taglib/tbytevector.h>
#include <taglib/wavfile.h>
#include <taglib/vorbisfile.h>
#include "MediaFileType.h"

namespace NickvisionTagger::Models
{
    /// <summary>
    /// A model of a music file
    /// </summary>
    class MusicFile
    {
    public:
        /// <summary>
        /// Constructs a MusicFile
        /// </summary>
        /// <param name="path">The path to the music file</param>
        /// <param name="fileType">The type of the music file</param>
        MusicFile(const std::filesystem::path& path, const MediaFileType& fileType);
        /// <summary>
        /// Gets the path of the music file
        /// </summary>
        /// <returns>The path of the music file</returns>
        const std::filesystem::path& getPath() const;
        /// <summary>
        /// Gets the filename of the music file
        /// </summary>
        /// <returns>The filename of the music file</returns>
        std::string getFilename() const;
        /// <summary>
        /// Sets the filename of the music file
        /// </summary>
        /// <param name="filename">The new filename</param>
        void setFilename(const std::string& filename);
        /// <summary>
        /// Gets the title of the music file
        /// </summary>
        /// <returns>The title of the music file</returns>
        std::string getTitle() const;
        /// <summary>
        /// Sets the title of the music file
        /// </summary>
        /// <param name="title">The new title</param>
        void setTitle(const std::string& title);
        /// <summary>
        /// Gets the artist of the music file
        /// </summary>
        /// <returns>The artist of the music file</returns>
        std::string getArtist() const;
        /// <summary>
        /// Sets the artist of the music file
        /// </summary>
        /// <param name="artist">The new artist</param>
        void setArtist(const std::string& artist);
        /// <summary>
        /// Gets the album of the music file
        /// </summary>
        /// <returns>The album of the music file</returns>
        std::string getAlbum() const;
        /// <summary>
        /// Sets the album of the music file
        /// </summary>
        /// <param name="album">The new album</param>
        void setAlbum(const std::string& album);
        /// <summary>
        /// Gets the year of the music file
        /// </summary>
        /// <returns>The year of the music file</returns>
        unsigned int getYear() const;
        /// <summary>
        /// Sets the year of the music file
        /// </summary>
        /// <param name="year">The new year</param>
        void setYear(unsigned int year);
        /// <summary>
        /// Gets the track of the music file
        /// </summary>
        /// <returns>The track of the music file</returns>
        unsigned int getTrack() const;
        /// <summary>
        /// Sets the track of the music file
        /// </summary>
        /// <param name="track">The new track</param>
        void setTrack(unsigned int track);
        /// <summary>
        /// Gets the album artist of the music file
        /// </summary>
        /// <returns>The album artist of the music file</returns>
        std::string getAlbumArtist() const;
        /// <summary>
        /// Sets the album artist of the music file
        /// </summary>
        /// <param name="albumArtist">The new album artist</param>
        void setAlbumArtist(const std::string& albumArtist);
        /// <summary>
        /// Gets the genre of the music file
        /// </summary>
        /// <returns>The genre of the music file</returns>
        std::string getGenre() const;
        /// <summary>
        /// Sets the genre of the music file
        /// </summary>
        /// <param name="genre">The new genre</param>
        void setGenre(const std::string& genre);
        /// <summary>
        /// Gets the comment of the music file
        /// </summary>
        /// <returns>The comment of the music file</returns>
        std::string getComment() const;
        /// <summary>
        /// Sets the comment of the music file
        /// </summary>
        /// <param name="comment">The new comment</param>
        void setComment(const std::string& comment);
        /// <summary>
        /// Gets the album art of the music file as a TagLib::ByteVector
        /// </summary>
        /// <returns>The TagLib::ByteVector representation of the album art of the music file</returns>
        TagLib::ByteVector getAlbumArt() const;
        /// <summary>
        /// Sets the album art of the music file
        /// </summary>
        /// <param name="albumArt">The TagLib::ByteVector representation of the new album art</param>
        void setAlbumArt(const TagLib::ByteVector& albumArt);
        /// <summary>
        /// Gets the duration of the music file in seconds
        /// </summary>
        /// <returns>The duration of the music file in seconds</returns>
        int getDuration() const;
        /// <summary>
        /// Gets a human-readable string representation of the duration of the music file
        /// </summary>
        /// <returns>A human-readable string representation of the duration of the music file in the format: "hh:mm:ss"</returns>
        std::string getDurationAsString() const;
        /// <summary>
        /// Gets the file size of the music file in bytes
        /// </summary>
        /// <returns>The file size of the music file in bytes</returns>
        std::uintmax_t getFileSize() const;
        /// <summary>
        /// Gets a human-readable string representation of the file size of the music file
        /// </summary>
        /// <returns>A human-readable string representation of the file size of the music file</returns>
        std::string getFileSizeAsString() const;
        /// <summary>
        /// Saves the tag of the music file
        /// </summary>
        void saveTag();
        /// <summary>
        /// Removes the tag of the music file
        /// </summary>
        void removeTag();
        /// <summary>
        /// Uses the music file's filename to fill in tag information based on the format string
        /// </summary>
        /// <param name="formatString">The format string</param>
        /// <returns>True if the operation was successful, else false</returns>
        bool filenameToTag(const std::string& formatString);
        /// <summary>
        /// Uses the music file's tag to set the file's filename in the format of the format string
        /// </summary>
        /// <param name="formatString">The format string</param>
        /// <returns>True if the operation was successful, else false</returns>
        bool tagToFilename(const std::string& formatString);
        /// <summary>
        /// Compares this.filename to toCompare.filename via less-than
        /// </summary>
        /// <param name="toCompare">The MusicFile to compare</param>
        /// <returns>True if this.filename < toCompare.filename, else false</returns>
        bool operator<(const MusicFile& toCompare) const;
        /// <summary>
        /// Compares this.filename to toCompare.filename via greater-than
        /// </summary>
        /// <param name="toCompare">The MusicFile to compare</param>
        /// <returns>True if this.filename > toCompare.filename, else false</returns>
        bool operator>(const MusicFile& toCompare) const;
        /// <summary>
        /// Compares this.path to toCompare.path via equals
        /// </summary>
        /// <param name="toCompare">The MusicFile to compare</param>
        /// <returns>True if this.path == toCompare.path, else false</returns>
        bool operator==(const MusicFile& toCompare) const;
        /// <summary>
        /// Compares this.path to toCompare.path via not equals
        /// </summary>
        /// <param name="toCompare">The MusicFile to compare</param>
        /// <returns>True if this.path != toCompare.path, else false</returns>
        bool operator!=(const MusicFile& toCompare) const;

    private:
        std::filesystem::path m_path;
        MediaFileType m_fileType;
        std::shared_ptr<TagLib::MPEG::File> m_fileMP3;
        std::shared_ptr<TagLib::Ogg::Vorbis::File> m_fileOGG;
        std::shared_ptr<TagLib::FLAC::File> m_fileFLAC;
        std::shared_ptr<TagLib::ASF::File> m_fileWMA;
        std::shared_ptr<TagLib::RIFF::WAV::File> m_fileWAV;
    };
}