#pragma once

#include <mutex>
#include <string>
#include <filesystem>
#include <memory>
#include <taglib/fileref.h>
#include "mediafiletype.h"

namespace NickvisionTagger::Models
{
    class MusicFile
    {
    public:
        MusicFile(const std::filesystem::path& path, const MediaFileType& fileType);
        const std::filesystem::path& getPath() const;
        int getId() const;
        void setId(int id);
        std::string getFilename() const;
        void setFilename(const std::string& filename);
        std::string getTitle() const;
        void setTitle(const std::string& title);
        std::string getArtist() const;
        void setArtist(const std::string& artist);
        std::string getAlbum() const;
        void setAlbum(const std::string& album);
        unsigned int getYear() const;
        void setYear(unsigned int year);
        unsigned int getTrack() const;
        void setTrack(unsigned int track);
        std::string getGenre() const;
        void setGenre(const std::string& genre);
        std::string getComment() const;
        void setComment(const std::string& comment);
        int getDuration() const;
        std::string getDurationAsString() const;
        std::uintmax_t getFileSize() const;
        std::string getFileSizeAsString() const;
        void saveTag();
        void removeTag();
        bool filenameToTag(const std::string& formatString);
        bool tagToFilename(const std::string& formatString);
        bool downloadMetadataFromInternet();
        bool operator<(const MusicFile& toCompare) const;
        bool operator>(const MusicFile& toCompare) const;
        bool operator==(const MusicFile& toCompare) const;
        bool operator!=(const MusicFile& toCompare) const;

    private:
        mutable std::mutex m_mutex;
        std::filesystem::path m_path;
        MediaFileType m_fileType;
        std::shared_ptr<TagLib::FileRef> m_file;
        int m_id;
    };
}