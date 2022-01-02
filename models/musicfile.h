#ifndef MUSICFILE_H
#define MUSICFILE_H

#include <string>
#include <filesystem>
#include <memory>
#include <taglib/fileref.h>

namespace NickvisionTagger::Models
{
    class MusicFile
    {
    public:
        MusicFile(const std::string& path);
        const std::filesystem::path& getPath() const;
        std::string getFilename() const;
        void setFilename(const std::string& filename);
        std::string getDotExtension() const;
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
        void filenameToTag(const std::string& formatString);
        void tagToFilename(const std::string& formatString);
        bool operator<(const MusicFile& toCompare) const;
        bool operator>(const MusicFile& toCompare) const;
        bool operator==(const MusicFile& toCompare) const;
        bool operator!=(const MusicFile& toCompare) const;

    private:
        std::filesystem::path m_path;
        std::shared_ptr<TagLib::FileRef> m_file;
    };
}

#endif // MUSICFILE_H
