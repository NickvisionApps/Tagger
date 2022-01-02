#include "musicfile.h"
#include <stdexcept>
#include "../helpers/mediahelpers.h"

namespace NickvisionTagger::Models
{
    using namespace NickvisionTagger::Helpers;

    MusicFile::MusicFile(const std::filesystem::path& path) : m_path(path), m_file(std::make_shared<TagLib::FileRef>(m_path.c_str()))
    {

    }

    const std::filesystem::path& MusicFile::getPath() const
    {
        return m_path;
    }

    std::string MusicFile::getFilename() const
    {
        return m_path.filename();
    }

    void MusicFile::setFilename(const std::string& filename)
    {
        std::string newPath = m_path.parent_path().string() + "/" + filename;
        if(newPath.find(getDotExtension()) == std::string::npos)
        {
            newPath += getDotExtension();
        }
        m_file.reset();
        std::filesystem::rename(m_path, newPath);
        m_path = newPath;
        m_file = std::make_shared<TagLib::FileRef>(m_path.c_str());
    }

    std::string MusicFile::getDotExtension() const
    {
        return m_path.extension();
    }

    std::string MusicFile::getTitle() const
    {
        return m_file->tag()->title().toCString();
    }

    void MusicFile::setTitle(const std::string& title)
    {
        m_file->tag()->setTitle(title);
    }

    std::string MusicFile::getArtist() const
    {
        return m_file->tag()->artist().toCString();
    }

    void MusicFile::setArtist(const std::string& artist)
    {
        m_file->tag()->setArtist(artist);
    }

    std::string MusicFile::getAlbum() const
    {
        return m_file->tag()->album().toCString();
    }

    void MusicFile::setAlbum(const std::string& album)
    {
        m_file->tag()->setAlbum(album);
    }

    unsigned int MusicFile::getYear() const
    {
        return m_file->tag()->year();
    }

    void MusicFile::setYear(unsigned int year)
    {
        m_file->tag()->setYear(year);
    }

    unsigned int MusicFile::getTrack() const
    {
        return m_file->tag()->track();
    }

    void MusicFile::setTrack(unsigned int track)
    {
        m_file->tag()->setTrack(track);
    }

    std::string MusicFile::getGenre() const
    {
        return m_file->tag()->genre().toCString();
    }

    void MusicFile::setGenre(const std::string& genre)
    {
        m_file->tag()->setGenre(genre);
    }

    std::string MusicFile::getComment() const
    {
        return m_file->tag()->comment().toCString();
    }

    void MusicFile::setComment(const std::string& comment)
    {
        m_file->tag()->setComment(comment);
    }

    int MusicFile::getDuration() const
    {
        return m_file->audioProperties()->lengthInSeconds();
    }

    std::string MusicFile::getDurationAsString() const
    {
        return MediaHelpers::durationToString(getDuration());
    }

    std::uintmax_t MusicFile::getFileSize() const
    {
        return std::filesystem::file_size(m_path);
    }

    std::string MusicFile::getFileSizeAsString() const
    {
        return MediaHelpers::fileSizeToString(getFileSize());
    }

    void MusicFile::saveTag()
    {
        m_file->save();
    }

    void MusicFile::removeTag()
    {
        setTitle("");
        setArtist("");
        setAlbum("");
        setYear(0);
        setTrack(0);
        setGenre("");
        setComment("");
        m_file->save();
    }

    void MusicFile::filenameToTag(const std::string& formatString)
    {
        if (formatString == "%artist%- %title%")
        {
            std::size_t dashIndex = getFilename().find("- ");
            if(dashIndex == std::string::npos)
            {
                throw std::invalid_argument("Invalid Filename. No dash was found.");
            }
            setArtist(getFilename().substr(0, dashIndex));
            setTitle(getFilename().substr(dashIndex + 2, getFilename().find(getDotExtension()) - (getArtist().size() + 2)));
            saveTag();
        }
        else if (formatString == "%title%- %artist%")
        {
            std::size_t dashIndex = getFilename().find("- ");
            if(dashIndex == std::string::npos)
            {
                throw std::invalid_argument("Invalid Filename. No dash was found.");
            }
            setTitle(getFilename().substr(0, dashIndex));
            setArtist(getFilename().substr(dashIndex + 2, getFilename().find(getDotExtension()) - (getTitle().size() + 2)));
            saveTag();
        }
        else if (formatString == "%title%")
        {
            setTitle(getFilename().substr(0, getFilename().find(getDotExtension())));
            saveTag();
        }
        else
        {
            throw std::invalid_argument("Invalid format string.");
        }
    }

    void MusicFile::tagToFilename(const std::string& formatString)
    {
        if (formatString == "%artist%- %title%")
        {
            if(getArtist().empty() || getArtist().empty())
            {
                throw std::invalid_argument("Invalid Tag. Artist and/or title are empty.");
            }
            setFilename(getArtist() + "- " + getTitle() + getDotExtension());
        }
        else if (formatString == "%title%- %artist%")
        {
            if(getTitle().empty() || getArtist().empty())
            {
                throw std::invalid_argument("Invalid Tag. Title and/or artist are empty.");
            }
            setFilename(getTitle() + "- " + getArtist() + getDotExtension());
        }
        else if (formatString == "%title%")
        {
            if(getTitle().empty())
            {
                throw std::invalid_argument("Invalid Tag. Title is empty.");
            }
            setFilename(getTitle() + getDotExtension());
        }
        else
        {
            throw std::invalid_argument("Invalid format string.");
        }
    }

    bool MusicFile::operator<(const MusicFile& toCompare) const
    {
        return getFilename() < toCompare.getFilename();
    }

    bool MusicFile::operator>(const MusicFile& toCompare) const
    {
        return getFilename() > toCompare.getFilename();
    }

    bool MusicFile::operator==(const MusicFile& toCompare) const
    {
        return m_path == toCompare.m_path;
    }

    bool MusicFile::operator!=(const MusicFile& toCompare) const
    {
        return m_path != toCompare.m_path;
    }
}
