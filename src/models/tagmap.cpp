#include "tagmap.hpp"

using namespace NickvisionTagger::Models;

TagMap::TagMap() : m_filename{ "" }, m_title{ "" }, m_artist{ "" }, m_album{ "" }, m_year{ "" }, m_track{ "" }, m_albumArtist{ "" }, m_genre{ "" }, m_comment{ "" }, m_albumArt{ "" }, m_duration{ "" }, m_fingerprint{ "" }, m_fileSize{ "" }
{

}

const std::string& TagMap::getFilename() const
{
    return m_filename;
}

void TagMap::setFilename(const std::string& filename)
{
    m_filename = filename;
}

const std::string& TagMap::getTitle() const
{
    return m_title;
}

void TagMap::setTitle(const std::string& title)
{
    m_title = title;
}

const std::string& TagMap::getArtist() const
{
    return m_artist;
}

void TagMap::setArtist(const std::string& artist)
{
    m_artist = artist;
}

const std::string& TagMap::getAlbum() const
{
    return m_album;
}

void TagMap::setAlbum(const std::string& album)
{
    m_album = album;
}

const std::string& TagMap::getYear() const
{
    return m_year;
}

void TagMap::setYear(const std::string& year)
{
    m_year = year;
}

const std::string& TagMap::getTrack() const
{
    return m_track;
}

void TagMap::setTrack(const std::string& track)
{
    m_track = track;
}

const std::string& TagMap::getAlbumArtist() const
{
    return m_albumArtist;
}

void TagMap::setAlbumArtist(const std::string& albumArtist)
{
    m_albumArtist = albumArtist;
}

const std::string& TagMap::getGenre() const
{
    return m_genre;
}

void TagMap::setGenre(const std::string& genre)
{
    m_genre = genre;
}

const std::string& TagMap::getComment() const
{
    return m_comment;
}

void TagMap::setComment(const std::string& comment)
{
    m_comment = comment;
}

const std::string& TagMap::getAlbumArt() const
{
    return m_albumArt;
}

void TagMap::setAlbumArt(const std::string& albumArt)
{
    m_albumArt = albumArt;
}

const std::string& TagMap::getDuration() const
{
    return m_duration;
}

void TagMap::setDuration(const std::string& duration)
{
    m_duration = duration;
}

const std::string& TagMap::getFingerprint() const
{
    return m_fingerprint;
}

void TagMap::setFingerprint(const std::string& fingerprint)
{
    m_fingerprint = fingerprint;
}

const std::string& TagMap::getFileSize() const
{
    return m_fileSize;
}

void TagMap::setFileSize(const std::string& fileSize)
{
    m_fileSize = fileSize;
}



