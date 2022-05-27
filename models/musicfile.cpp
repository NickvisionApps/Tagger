#include "musicfile.h"
#include <stdexcept>
#include <taglib/id3v2tag.h>
#include <taglib/xiphcomment.h>
#include <taglib/attachedpictureframe.h>
#include <taglib/flacpicture.h>
#include "../helpers/mediahelpers.h"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

MusicFile::MusicFile(const std::filesystem::path& path, const MediaFileType& fileType) : m_path{path}, m_fileType{fileType}
{
    if(!fileType.isAudio())
    {
        throw std::invalid_argument("Invalid Path. The path is not a music file.");
    }
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3 = std::make_shared<TagLib::MPEG::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG = std::make_shared<TagLib::Ogg::Opus::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC = std::make_shared<TagLib::FLAC::File>(m_path.c_str());
    }
}

const std::filesystem::path& MusicFile::getPath() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_path;
}

std::string MusicFile::getFilename() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    return m_path.filename();
}

void MusicFile::setFilename(const std::string& filename)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    std::string newPath{m_path.parent_path().string() + "/" + filename};
    if(newPath.find(m_path.extension()) == std::string::npos)
    {
        newPath += m_path.extension();
    }
    m_fileMP3.reset();
    m_fileOGG.reset();
    m_fileFLAC.reset();
    std::filesystem::rename(m_path, newPath);
    m_path = newPath;
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3 = std::make_shared<TagLib::MPEG::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG = std::make_shared<TagLib::Ogg::Opus::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC = std::make_shared<TagLib::FLAC::File>(m_path.c_str());
    }
}

std::string MusicFile::getTitle() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->title().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->title().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->title().toCString();
    }
    return "";
}

void MusicFile::setTitle(const std::string& title)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setTitle(title);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setTitle(title);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setTitle(title);
    }
}

std::string MusicFile::getArtist() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->artist().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->artist().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->artist().toCString();
    }
    return "";
}

void MusicFile::setArtist(const std::string& artist)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setArtist(artist);
    }
}

std::string MusicFile::getAlbum() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->album().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->album().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->album().toCString();
    }
    return "";
}

void MusicFile::setAlbum(const std::string& album)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setAlbum(album);
    }
}

unsigned int MusicFile::getYear() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->year();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->year();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->year();
    }
    return 0;
}

void MusicFile::setYear(unsigned int year)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setYear(year);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setYear(year);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setYear(year);
    }
}

unsigned int MusicFile::getTrack() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->track();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->track();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->track();
    }
    return 0;
}

void MusicFile::setTrack(unsigned int track)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setTrack(track);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setTrack(track);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setTrack(track);
    }
}

std::string MusicFile::getGenre() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->genre().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->genre().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->genre().toCString();
    }
    return "";
}

void MusicFile::setGenre(const std::string& genre)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setGenre(genre);
    }
}

std::string MusicFile::getComment() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->comment().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->comment().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->comment().toCString();
    }
    return "";
}

void MusicFile::setComment(const std::string& comment)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->setComment(comment);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setComment(comment);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->setComment(comment);
    }
}

TagLib::ByteVector MusicFile::getAlbumArt() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag()->frameList()[0]->render();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->pictureList()[0]->data();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->ID3v2Tag()->frameList()[0]->render();
    }
    return {};
}

void MusicFile::setAlbumArt(const TagLib::ByteVector& albumArt)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag()->addFrame(new TagLib::ID3v2::AttachedPictureFrame(albumArt));
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->addPicture(new TagLib::FLAC::Picture(albumArt));
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->ID3v2Tag()->addFrame(new TagLib::ID3v2::AttachedPictureFrame(albumArt));
    }
}

int MusicFile::getDuration() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->audioProperties()->lengthInSeconds();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->audioProperties()->lengthInSeconds();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->audioProperties()->lengthInSeconds();
    }
    return 0;
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
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->save();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->save();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->save();
    }
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