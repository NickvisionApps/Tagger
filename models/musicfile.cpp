#include "musicfile.h"
#include <stdexcept>
#include "../helpers/mediahelpers.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

MusicFile::MusicFile(const std::filesystem::path& path, const MediaFileType& fileType) : m_path{path}, m_fileType{fileType}, m_file(std::make_shared<TagLib::FileRef>(m_path.c_str())), m_id(0)
{
    if(!fileType.isAudio())
    {
        throw std::invalid_argument("Invalid Path. The path is not a music file.");
    }
}

const std::filesystem::path& MusicFile::getPath() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_path;
}

int MusicFile::getId() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_id;
}

void MusicFile::setId(int id)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_id = id;
}

std::string MusicFile::getFilename() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_path.filename();
}

void MusicFile::setFilename(const std::string& filename)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    std::string newPath = m_path.parent_path().string() + "/" + filename;
    if(newPath.find(m_path.extension()) == std::string::npos)
    {
        newPath += m_path.extension();
    }
    m_file.reset();
    std::filesystem::rename(m_path, newPath);
    m_path = newPath;
    m_file = std::make_shared<TagLib::FileRef>(m_path.c_str());
}

std::string MusicFile::getTitle() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->title().toCString();
}

void MusicFile::setTitle(const std::string& title)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setTitle(title);
}

std::string MusicFile::getArtist() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->artist().toCString();
}

void MusicFile::setArtist(const std::string& artist)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setArtist(artist);
}

std::string MusicFile::getAlbum() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->album().toCString();
}

void MusicFile::setAlbum(const std::string& album)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setAlbum(album);
}

unsigned int MusicFile::getYear() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->year();
}

void MusicFile::setYear(unsigned int year)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setYear(year);
}

unsigned int MusicFile::getTrack() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->track();
}

void MusicFile::setTrack(unsigned int track)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setTrack(track);
}

std::string MusicFile::getGenre() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->genre().toCString();
}

void MusicFile::setGenre(const std::string& genre)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setGenre(genre);
}

std::string MusicFile::getComment() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->tag()->comment().toCString();
}

void MusicFile::setComment(const std::string& comment)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    m_file->tag()->setComment(comment);
}

int MusicFile::getDuration() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_file->audioProperties()->lengthInSeconds();
}

std::string MusicFile::getDurationAsString() const
{
    return MediaHelpers::durationToString(getDuration());
}

std::uintmax_t MusicFile::getFileSize() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return std::filesystem::file_size(m_path);
}

std::string MusicFile::getFileSizeAsString() const
{
    return MediaHelpers::fileSizeToString(getFileSize());
}

void MusicFile::saveTag()
{
    std::lock_guard<std::mutex> lock{m_mutex};
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
    saveTag();
}

bool MusicFile::filenameToTag(const std::string& formatString)
{
    if (formatString == "%artist%- %title%")
    {
        std::size_t dashIndex = getFilename().find("- ");
        if(dashIndex == std::string::npos)
        {
            return false;
        }
        setArtist(getFilename().substr(0, dashIndex));
        setTitle(getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension()) - (getArtist().size() + 2)));
        saveTag();
    }
    else if (formatString == "%title%- %artist%")
    {
        std::size_t dashIndex = getFilename().find("- ");
        if(dashIndex == std::string::npos)
        {
            return false;
        }
        setTitle(getFilename().substr(0, dashIndex));
        setArtist(getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension()) - (getTitle().size() + 2)));
        saveTag();
    }
    else if (formatString == "%title%")
    {
        setTitle(getFilename().substr(0, getFilename().find(m_path.extension())));
        saveTag();
    }
    else
    {
        return false;
    }
    return true;
}

bool MusicFile::tagToFilename(const std::string& formatString)
{
    if (formatString == "%artist%- %title%")
    {
        if(getArtist().empty() || getTitle().empty())
        {
            return false;
        }
        setFilename(getArtist() + "- " + getTitle() + m_path.extension().string());
    }
    else if (formatString == "%title%- %artist%")
    {
        if(getTitle().empty() || getArtist().empty())
        {
            return false;
        }
        setFilename(getTitle() + "- " + getArtist() + m_path.extension().string());
    }
    else if (formatString == "%title%")
    {
        if(getTitle().empty())
        {
            return false;
        }
        setFilename(getTitle() + m_path.extension().string());
    }
    else
    {
        return false;
    }
    return true;
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
    return getPath() == toCompare.getPath();
}

bool MusicFile::operator!=(const MusicFile& toCompare) const
{
    return getPath() != toCompare.getPath();
}