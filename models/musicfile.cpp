#include "musicfile.h"
#include <stdexcept>
#include <musicbrainz5/Query.h>
#include <musicbrainz5/Metadata.h>
#include <musicbrainz5/Artist.h>
#include <musicbrainz5/Release.h>
#include <musicbrainz5/ReleaseGroup.h>
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
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA = std::make_shared<TagLib::ASF::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV = std::make_shared<TagLib::RIFF::WAV::File>(m_path.c_str());
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
    std::string newPath = m_path.parent_path().string() + "/" + filename;
    if(newPath.find(m_path.extension()) == std::string::npos)
    {
        newPath += m_path.extension();
    }
    m_fileMP3.reset();
    m_fileOGG.reset();
    m_fileFLAC.reset();
    m_fileWMA.reset();
    m_fileWAV.reset();
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
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA = std::make_shared<TagLib::ASF::File>(m_path.c_str());
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV = std::make_shared<TagLib::RIFF::WAV::File>(m_path.c_str());
    }
}

std::string MusicFile::getTitle() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->title().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->title().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->title().toCString();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->title().toCString();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->title().toCString();
    }
    return "";
}

void MusicFile::setTitle(const std::string& title)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setTitle(title);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setTitle(title);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setTitle(title);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setTitle(title);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setTitle(title);
    }
}

std::string MusicFile::getArtist() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->artist().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->artist().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->artist().toCString();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->artist().toCString();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->artist().toCString();
    }
    return "";
}

void MusicFile::setArtist(const std::string& artist)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setArtist(artist);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setArtist(artist);
    }
}

std::string MusicFile::getAlbum() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->album().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->album().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->album().toCString();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->album().toCString();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->album().toCString();
    }
    return "";
}

void MusicFile::setAlbum(const std::string& album)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setAlbum(album);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setAlbum(album);
    }
}

unsigned int MusicFile::getYear() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->year();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->year();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->year();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->year();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->year();
    }
    return 0;
}

void MusicFile::setYear(unsigned int year)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setYear(year);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setYear(year);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setYear(year);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setYear(year);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setYear(year);
    }
}

unsigned int MusicFile::getTrack() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->track();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->track();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->track();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->track();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->track();
    }
    return 0;
}

void MusicFile::setTrack(unsigned int track)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setTrack(track);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setTrack(track);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setTrack(track);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setTrack(track);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setTrack(track);
    }
}

std::string MusicFile::getGenre() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->genre().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->genre().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->genre().toCString();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->genre().toCString();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->genre().toCString();
    }
    return "";
}

void MusicFile::setGenre(const std::string& genre)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setGenre(genre);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setGenre(genre);
    }
}

std::string MusicFile::getComment() const
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        return m_fileMP3->ID3v2Tag(true)->comment().toCString();
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        return m_fileOGG->tag()->comment().toCString();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        return m_fileFLAC->xiphComment(true)->comment().toCString();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->tag()->comment().toCString();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->ID3v2Tag()->comment().toCString();
    }
    return "";
}

void MusicFile::setComment(const std::string& comment)
{
    std::lock_guard<std::mutex> lock{m_mutex};
    if(m_fileType == MediaFileType::MP3)
    {
        m_fileMP3->ID3v2Tag(true)->setComment(comment);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->tag()->setComment(comment);
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->xiphComment(true)->setComment(comment);
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->tag()->setComment(comment);
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->ID3v2Tag()->setComment(comment);
    }
}

TagLib::ByteVector MusicFile::getAlbumArt() const
{
    std::lock_guard<std::mutex> lock(m_mutex);
    if(m_fileType == MediaFileType::MP3)
    {
        const TagLib::ID3v2::FrameList& frameList = m_fileMP3->ID3v2Tag(true)->frameList("APIC");
        if(!frameList.isEmpty())
        {
            return ((TagLib::ID3v2::AttachedPictureFrame*)frameList.front())->picture();
        }
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        const TagLib::List<TagLib::FLAC::Picture*>& pictureList = m_fileOGG->tag()->pictureList();
        if(!pictureList.isEmpty())
        {
            return pictureList[0]->data();
        }
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        const TagLib::List<TagLib::FLAC::Picture*>& pictureList = m_fileFLAC->pictureList();
        if(!pictureList.isEmpty())
        {
            return pictureList[0]->data();
        }
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        const TagLib::ASF::AttributeListMap& attributeListMap = m_fileWMA->tag()->attributeListMap();
        if(attributeListMap.contains("WM/Picture"))
        {
            const TagLib::ASF::AttributeList& attributeList = attributeListMap["WM/Picture"];
            if(!attributeList.isEmpty())
            {
                return attributeList[0].toPicture().picture();
            }
        }
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        const TagLib::ID3v2::FrameList& frameList = m_fileWAV->ID3v2Tag()->frameList("APIC");
        if(!frameList.isEmpty())
        {
            return ((TagLib::ID3v2::AttachedPictureFrame*)frameList.front())->picture();
        }
    }
    return {};
}

void MusicFile::setAlbumArt(const TagLib::ByteVector& albumArt)
{
    std::lock_guard<std::mutex> lock(m_mutex);
    if(m_fileType == MediaFileType::MP3)
    {
        
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        
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
    else if(m_fileType == MediaFileType::WMA)
    {
        return m_fileWMA->audioProperties()->lengthInSeconds();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        return m_fileWAV->audioProperties()->lengthInSeconds();
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
        m_fileMP3->save(TagLib::MPEG::File::TagTypes::ID3v2);
    }
    else if(m_fileType == MediaFileType::OGG)
    {
        m_fileOGG->save();
    }
    else if(m_fileType == MediaFileType::FLAC)
    {
        m_fileFLAC->save();
    }
    else if(m_fileType == MediaFileType::WMA)
    {
        m_fileWMA->save();
    }
    else if(m_fileType == MediaFileType::WAV)
    {
        m_fileWAV->save(TagLib::RIFF::WAV::File::TagTypes::ID3v2);
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
    setAlbumArt({});
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

bool MusicFile::downloadMusicBrainzMetadata()
{
    MusicBrainz5::CQuery Query{"NickvisionTagger/2022.5.1 ( nlogozzo225@gmail.com )"};
    if(!getTitle().empty() && !getArtist().empty())
    {
        try
        {
            MusicBrainz5::CMetadata metadataRelease{Query.Query("release", "", "", { {"query", "artist:" + getArtist() + " release:" + getTitle()} })};
            if(metadataRelease.ReleaseList() && metadataRelease.ReleaseList()->Item(0))
            {
                MusicBrainz5::CRelease* release{metadataRelease.ReleaseList()->Item(0)};
                setYear(MediaHelpers::musicBrainzDateToYear(release->Date()));
                if(release->ReleaseGroup() && release->ReleaseGroup()->PrimaryType() == "Album")
                {
                    setAlbum(release->ReleaseGroup()->Title());
                }
                saveTag();
                return true;
            }
        }
        catch(...)
        {
            return false;
        }
    }
    return false;
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