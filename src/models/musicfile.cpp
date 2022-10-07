#include "musicfile.hpp"
#include <array>
#include <cstdio>
#include <stdexcept>
#include <taglib/asffile.h>
#include <taglib/flacfile.h>
#include <taglib/mpegfile.h>
#include <taglib/textidentificationframe.h>
#include <taglib/tstring.h>
#include <taglib/wavfile.h>
#include <taglib/vorbisfile.h>
#include "acoustidquery.hpp"
#include "acoustidsubmission.hpp"
#include "musicbrainzrecordingquery.hpp"
#include "tagmap.hpp"
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

MusicFile::MusicFile(const std::filesystem::path& path) : m_path{ path }, m_dotExtension{ m_path.extension() }, m_modificationTimeStamp{ std::filesystem::last_write_time(m_path) }, m_fingerprint{ "" }
{
    if(m_dotExtension == ".mp3")
    {
        TagLib::MPEG::File file{ m_path.c_str() };
        m_title = file.ID3v2Tag(true)->title().to8Bit(true);
        m_artist = file.ID3v2Tag(true)->artist().to8Bit(true);
        m_album = file.ID3v2Tag(true)->album().to8Bit(true);
        m_year = file.ID3v2Tag(true)->year();
        m_track = file.ID3v2Tag(true)->track();
        const TagLib::ID3v2::FrameList& frameAlbumArtist{ file.ID3v2Tag(true)->frameList("TPE2") };
        if (!frameAlbumArtist.isEmpty())
        {
            m_albumArtist = frameAlbumArtist.front()->toString().to8Bit(true);
        }
        m_genre = file.ID3v2Tag(true)->genre().to8Bit(true);
        m_comment = file.ID3v2Tag(true)->comment().to8Bit(true);
        const TagLib::ID3v2::FrameList& frameAlbumArt{ file.ID3v2Tag(true)->frameList("APIC") };
        if (!frameAlbumArt.isEmpty())
        {
            m_albumArt = ((TagLib::ID3v2::AttachedPictureFrame*)frameAlbumArt.front())->picture();
        }
        m_duration = file.audioProperties()->lengthInSeconds();
    }
    else if(m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        TagLib::Ogg::Vorbis::File file{ m_path.c_str() };
        m_title = file.tag()->title().to8Bit(true);
        m_artist = file.tag()->artist().to8Bit(true);
        m_album = file.tag()->album().to8Bit(true);
        m_year = file.tag()->year();
        m_track = file.tag()->track();
        const TagLib::Ogg::FieldListMap& fieldAlbumArtist{ file.tag()->fieldListMap() };
        if (fieldAlbumArtist.contains("ALBUMARTIST"))
        {
            const TagLib::StringList& listAlbumArtist{ fieldAlbumArtist["ALBUMARTIST"] };
            if (!listAlbumArtist.isEmpty())
            {
                m_albumArtist = listAlbumArtist[0].to8Bit(true);
            }
        }
        m_genre = file.tag()->genre().to8Bit(true);
        m_comment = file.tag()->comment().to8Bit(true);
        const TagLib::List<TagLib::FLAC::Picture*>& listAlbumArt{ file.tag()->pictureList() };
        if (!listAlbumArt.isEmpty())
        {
            m_albumArt = listAlbumArt[0]->data();
        }
        m_duration = file.audioProperties()->lengthInSeconds();
    }
    else if(m_dotExtension == ".flac")
    {
        TagLib::FLAC::File file{ m_path.c_str() };
        m_title = file.xiphComment(true)->title().to8Bit(true);
        m_artist = file.xiphComment(true)->artist().to8Bit(true);
        m_album = file.xiphComment(true)->album().to8Bit(true);
        m_year = file.xiphComment(true)->year();
        m_track = file.xiphComment(true)->track();
        const TagLib::Ogg::FieldListMap& fieldAlbumArtist{ file.xiphComment(true)->fieldListMap() };
        if (fieldAlbumArtist.contains("ALBUMARTIST"))
        {
            const TagLib::StringList& listAlbumArtist{ fieldAlbumArtist["ALBUMARTIST"] };
            if (!listAlbumArtist.isEmpty())
            {
                m_albumArtist = listAlbumArtist[0].to8Bit(true);
            }
        }
        m_genre = file.xiphComment(true)->genre().to8Bit(true);
        m_comment = file.xiphComment(true)->comment().to8Bit(true);
        const TagLib::List<TagLib::FLAC::Picture*>& listAlbumArt{ file.xiphComment(true)->pictureList() };
        if (!listAlbumArt.isEmpty())
        {
            m_albumArt = listAlbumArt[0]->data();
        }
        m_duration = file.audioProperties()->lengthInSeconds();
    }
    else if(m_dotExtension == ".wma")
    {
        TagLib::ASF::File file{ m_path.c_str() };
        m_title = file.tag()->title().to8Bit(true);
        m_artist = file.tag()->artist().to8Bit(true);
        m_album = file.tag()->album().to8Bit(true);
        m_year = file.tag()->year();
        m_track = file.tag()->track();
        const TagLib::ASF::AttributeListMap& attributeListMap{ file.tag()->attributeListMap() };
        if (attributeListMap.contains("ALBUMARTIST"))
        {
            const TagLib::ASF::AttributeList& attributeList{ attributeListMap["ALBUMARTIST"] };
            if (!attributeList.isEmpty())
            {
                m_albumArtist = attributeList[0].toString().to8Bit(true);
            }
        }
        m_genre = file.tag()->genre().to8Bit(true);
        m_comment = file.tag()->comment().to8Bit(true);
        if (attributeListMap.contains("WM/Picture"))
        {
            const TagLib::ASF::AttributeList& attributeList{ attributeListMap["WM/Picture"] };
            if (!attributeList.isEmpty())
            {
                m_albumArt = attributeList[0].toPicture().picture();
            }
        }
        m_duration = file.audioProperties()->lengthInSeconds();
    }
    else if(m_dotExtension == ".wav")
    {
        TagLib::RIFF::WAV::File file{ m_path.c_str() };
        m_title = file.ID3v2Tag()->title().to8Bit(true);
        m_artist = file.ID3v2Tag()->artist().to8Bit(true);
        m_album = file.ID3v2Tag()->album().to8Bit(true);
        m_year = file.ID3v2Tag()->year();
        m_track = file.ID3v2Tag()->track();
        const TagLib::ID3v2::FrameList& frameAlbumArtist{ file.ID3v2Tag()->frameList("TPE2") };
        if (!frameAlbumArtist.isEmpty())
        {
            m_albumArtist = frameAlbumArtist.front()->toString().to8Bit(true);
        }
        m_genre = file.ID3v2Tag()->genre().to8Bit(true);
        m_comment = file.ID3v2Tag()->comment().to8Bit(true);
        const TagLib::ID3v2::FrameList& frameAlbumArt{ file.ID3v2Tag()->frameList("APIC") };
        if (!frameAlbumArt.isEmpty())
        {
            m_albumArt = ((TagLib::ID3v2::AttachedPictureFrame*)frameAlbumArt.front())->picture();
        }
        m_duration = file.audioProperties()->lengthInSeconds();
    }
    else
    {
        throw std::invalid_argument("Invalid Path. The path is not a valid music file.");
    }
}

const std::filesystem::path& MusicFile::getPath() const
{
    return m_path;
}

std::string MusicFile::getFilename() const
{
    return m_path.filename().string();
}

void MusicFile::setFilename(const std::string& filename)
{
    std::string newPath{ m_path.parent_path().string() + "/" + filename };
    if (newPath.find(m_dotExtension) == std::string::npos)
    {
        newPath += m_dotExtension;
    }
    if(!std::filesystem::exists(newPath))
    {
        std::filesystem::rename(m_path, newPath);
        m_path = newPath;
    }
}

const std::string& MusicFile::getTitle() const
{
    return m_title;
}

void MusicFile::setTitle(const std::string& title)
{
    m_title = title;
}

const std::string& MusicFile::getArtist() const
{
    return m_artist;
}

void MusicFile::setArtist(const std::string& artist)
{
    m_artist = artist;
}

const std::string& MusicFile::getAlbum() const
{
    return m_album;
}

void MusicFile::setAlbum(const std::string& album)
{
    m_album = album;
}

unsigned int MusicFile::getYear() const
{
    return m_year;
}

void MusicFile::setYear(unsigned int year)
{
    m_year = year;
}

unsigned int MusicFile::getTrack() const
{
    return m_track;
}

void MusicFile::setTrack(unsigned int track)
{
    m_track = track;
}

const std::string& MusicFile::getAlbumArtist() const
{
    return m_albumArtist;
}

void MusicFile::setAlbumArtist(const std::string& albumArtist)
{
    m_albumArtist = albumArtist;
}

const std::string& MusicFile::getGenre() const
{
    return m_genre;
}

void MusicFile::setGenre(const std::string& genre)
{
    m_genre = genre;
}

const std::string& MusicFile::getComment() const
{
    return m_comment;
}

void MusicFile::setComment(const std::string& comment)
{
    m_comment = comment;
}

const TagLib::ByteVector& MusicFile::getAlbumArt() const
{
    return m_albumArt;
}

void MusicFile::setAlbumArt(const TagLib::ByteVector& albumArt)
{
    m_albumArt = albumArt;
}

int MusicFile::getDuration() const
{
    return m_duration;
}

std::string MusicFile::getDurationAsString() const
{
    return MediaHelpers::durationToString(m_duration);
}

std::uintmax_t MusicFile::getFileSize() const
{
    return std::filesystem::file_size(m_path);
}

std::string MusicFile::getFileSizeAsString() const
{
    return MediaHelpers::fileSizeToString(getFileSize());
}

const std::string& MusicFile::getChromaprintFingerprint()
{
    if(m_fingerprint.empty() || m_fingerprint == "ERROR")
    {
        std::string cmd{ "fpcalc \"" + m_path.string() + "\"" };
        std::string output{ "" };
        std::array<char, 128> buffer;
        FILE* pipe{ popen(cmd.c_str(), "r") };
        if(pipe)
        {
            while(!feof(pipe))
            {
                if(fgets(buffer.data(), 128, pipe) != nullptr)
                {
                    output += buffer.data();
                }
            }
            int resultCode{ pclose(pipe) };
            m_fingerprint = resultCode == EXIT_SUCCESS ? output.substr(output.find("FINGERPRINT=") + 12) : "CMD ERROR ";
            m_fingerprint.pop_back();
        }
        else
        {
            m_fingerprint = "PIPE ERROR";
        }
    }
    return m_fingerprint;
}

void MusicFile::saveTag(bool preserveModificationTimeStamp)
{
    if (m_dotExtension == ".mp3")
    {
        TagLib::MPEG::File file{ m_path.c_str() };
        file.ID3v2Tag(true)->setTitle({ m_title, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->setArtist({ m_artist, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->setAlbum({ m_album, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->setYear(m_year);
        file.ID3v2Tag(true)->setTrack(m_track);
        file.ID3v2Tag(true)->removeFrames("TPE2");
        TagLib::ID3v2::TextIdentificationFrame* frameAlbumArtist{ new TagLib::ID3v2::TextIdentificationFrame("TPE2", TagLib::String::UTF8) };
        frameAlbumArtist->setText({ m_albumArtist, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->addFrame(frameAlbumArtist);
        file.ID3v2Tag(true)->setGenre({ m_genre, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->setComment({ m_comment, TagLib::String::Type::UTF8 });
        file.ID3v2Tag(true)->removeFrames("APIC");
        if (!m_albumArt.isEmpty())
        {
            TagLib::ID3v2::AttachedPictureFrame* frameAlbumArt{ new TagLib::ID3v2::AttachedPictureFrame };
            frameAlbumArt->setType(TagLib::ID3v2::AttachedPictureFrame::Type::FrontCover);
            frameAlbumArt->setPicture(m_albumArt);
            file.ID3v2Tag(true)->addFrame(frameAlbumArt);
        }
        file.save(TagLib::MPEG::File::TagTypes::ID3v2);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        TagLib::Ogg::Vorbis::File file{ m_path.c_str() };
        file.tag()->setTitle({ m_title, TagLib::String::Type::UTF8 });
        file.tag()->setArtist({ m_artist, TagLib::String::Type::UTF8 });
        file.tag()->setAlbum({ m_album, TagLib::String::Type::UTF8 });
        file.tag()->setYear(m_year);
        file.tag()->setTrack(m_track);
        file.tag()->addField("ALBUMARTIST", { m_albumArtist, TagLib::String::Type::UTF8 });
        file.tag()->setGenre({ m_genre, TagLib::String::Type::UTF8 });
        file.tag()->setComment({ m_comment, TagLib::String::Type::UTF8 });
        file.tag()->removeAllPictures();
        if (!m_albumArt.isEmpty())
        {
            TagLib::FLAC::Picture* picture{ new TagLib::FLAC::Picture() };
            picture->setType(TagLib::FLAC::Picture::Type::FrontCover);
            picture->setData(m_albumArt);
            file.tag()->addPicture(picture);
        }
        file.save();
    }
    else if (m_dotExtension == ".flac")
    {
        TagLib::FLAC::File file{ m_path.c_str() };
        file.xiphComment(true)->setTitle({ m_title, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->setArtist({ m_artist, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->setAlbum({ m_album, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->setYear(m_year);
        file.xiphComment(true)->setTrack(m_track);
        file.xiphComment(true)->addField("ALBUMARTIST", { m_albumArtist, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->setGenre({ m_genre, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->setComment({ m_comment, TagLib::String::Type::UTF8 });
        file.xiphComment(true)->removeAllPictures();
        if (!m_albumArt.isEmpty())
        {
            TagLib::FLAC::Picture* picture{ new TagLib::FLAC::Picture() };
            picture->setType(TagLib::FLAC::Picture::Type::FrontCover);
            picture->setData(m_albumArt);
            file.xiphComment(true)->addPicture(picture);
        }
        file.save();
    }
    else if (m_dotExtension == ".wma")
    {
        TagLib::ASF::File file{ m_path.c_str() };
        file.tag()->setTitle({ m_title, TagLib::String::Type::UTF8 });
        file.tag()->setArtist({ m_artist, TagLib::String::Type::UTF8 });
        file.tag()->setAlbum({ m_album, TagLib::String::Type::UTF8 });
        file.tag()->setYear(m_year);
        file.tag()->setTrack(m_track);
        file.tag()->removeItem("ALBUMARTIST");
        file.tag()->addAttribute("ALBUMARTIST", { { m_albumArtist, TagLib::String::Type::UTF8 } });
        file.tag()->setGenre({ m_genre, TagLib::String::Type::UTF8 });
        file.tag()->setComment({ m_comment, TagLib::String::Type::UTF8 });
        file.tag()->removeItem("WM/Picture");
        file.tag()->addAttribute("WM/Picture", { m_albumArt });
        file.save();
    }
    else if (m_dotExtension == ".wav")
    {
        TagLib::RIFF::WAV::File file{ m_path.c_str() };
        file.ID3v2Tag()->setTitle({ m_title, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->setArtist({ m_artist, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->setAlbum({ m_album, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->setYear(m_year);
        file.ID3v2Tag()->setTrack(m_track);
        file.ID3v2Tag()->removeFrames("TPE2");
        TagLib::ID3v2::TextIdentificationFrame* frameAlbumArtist{ new TagLib::ID3v2::TextIdentificationFrame("TPE2", TagLib::String::UTF8) };
        frameAlbumArtist->setText({ m_albumArtist, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->addFrame(frameAlbumArtist);
        file.ID3v2Tag()->setGenre({ m_genre, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->setComment({ m_comment, TagLib::String::Type::UTF8 });
        file.ID3v2Tag()->removeFrames("APIC");
        if (!m_albumArt.isEmpty())
        {
            TagLib::ID3v2::AttachedPictureFrame* frameAlbumArt{ new TagLib::ID3v2::AttachedPictureFrame };
            frameAlbumArt->setType(TagLib::ID3v2::AttachedPictureFrame::Type::FrontCover);
            frameAlbumArt->setPicture(m_albumArt);
            file.ID3v2Tag()->addFrame(frameAlbumArt);
        }
        file.save(TagLib::RIFF::WAV::File::TagTypes::ID3v2);
    }
    if (preserveModificationTimeStamp)
    {
        std::filesystem::last_write_time(m_path, m_modificationTimeStamp);
    }
    else
    {
        m_modificationTimeStamp = std::filesystem::last_write_time(m_path);
    }
}

void MusicFile::removeTag(bool preserveModificationTimeStamp)
{
    m_title = "";
    m_artist = "";
    m_album = "";
    m_year = 0;
    m_track = 0;
    m_albumArtist = "";
    m_genre = "";
    m_comment = "";
    m_albumArt = TagLib::ByteVector();
    saveTag(preserveModificationTimeStamp);
}

bool MusicFile::filenameToTag(const std::string& formatString)
{
    if (formatString == "%artist%- %title%")
    {
        std::size_t dashIndex{ getFilename().find("- ") };
        if (dashIndex == std::string::npos)
        {
            return false;
        }
        m_artist = getFilename().substr(0, dashIndex);
        m_title = getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (getArtist().size() + 2));
    }
    else if (formatString == "%title%- %artist%")
    {
        std::size_t dashIndex{ getFilename().find("- ") };
        if (dashIndex == std::string::npos)
        {
            return false;
        }
        m_title = getFilename().substr(0, dashIndex);
        m_artist = getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (getTitle().size() + 2));
    }
    else if (formatString == "%track%- %title%")
    {
        std::size_t dashIndex{ getFilename().find("- ") };
        if (dashIndex == std::string::npos)
        {
            return false;
        }
        std::string track{ getFilename().substr(0, dashIndex) };
        try
        {
            m_track = MediaHelpers::stoui(track);
        }
        catch (...)
        {
            setTrack(0);
        }
        m_title = getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (track.size() + 2));
    }
    else if (formatString == "%title%")
    {
        m_title = getFilename().substr(0, getFilename().find(m_path.extension().string()));
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
        if (m_artist.empty() || m_title.empty())
        {
            return false;
        }
        setFilename(m_artist + "- " + m_title + m_path.extension().string());
    }
    else if (formatString == "%title%- %artist%")
    {
        if (m_title.empty() || m_artist.empty())
        {
            return false;
        }
        setFilename(m_title + "- " + m_artist + m_path.extension().string());
    }
    else if (formatString == "%track%- %title%")
    {
        if (m_title.empty())
        {
            return false;
        }
        setFilename(std::to_string(m_track) + "- " + m_title + m_path.extension().string());
    }
    else if (formatString == "%title%")
    {
        if (m_title.empty())
        {
            return false;
        }
        setFilename(m_title + m_path.extension().string());
    }
    else
    {
        return false;
    }
    return true;
}

bool MusicFile::downloadMusicBrainzMetadata(const std::string& acoustIdClientKey, bool overwriteTagWithMusicBrainz)
{
    AcoustIdQuery acoustIdQuery{ acoustIdClientKey, getDuration(), getChromaprintFingerprint() };
    if(acoustIdQuery.lookup())
    {
        MusicBrainzRecordingQuery musicBrainzQuery{ acoustIdQuery.getRecordingId() };
        if(musicBrainzQuery.lookup())
        {
            if(overwriteTagWithMusicBrainz || m_title.empty())
            {
                m_title = musicBrainzQuery.getTitle();
            }
            if(overwriteTagWithMusicBrainz || m_artist.empty())
            {
                m_artist = musicBrainzQuery.getArtist();
            }
            if(overwriteTagWithMusicBrainz || m_album.empty())
            {
                m_album = musicBrainzQuery.getAlbum();
            }
            if(overwriteTagWithMusicBrainz || m_year == 0)
            {
                m_year = musicBrainzQuery.getYear();
            }
            if(overwriteTagWithMusicBrainz || m_albumArtist.empty())
            {
                m_albumArtist = musicBrainzQuery.getAlbumArtist();
            }
            if(overwriteTagWithMusicBrainz || m_genre.empty())
            {
                m_genre = musicBrainzQuery.getGenre();
            }
            if(overwriteTagWithMusicBrainz || m_albumArt.isEmpty())
            {
                m_albumArt = musicBrainzQuery.getAlbumArt();
            }
            return true;
        }
    }
    return false;
}

bool MusicFile::submitToAcoustId(const std::string& acoustIdClientAPIKey, const std::string& acoustIdUserAPIKey, const std::string& musicBrainzRecordingId)
{
    AcoustIdSubmission submission{ acoustIdClientAPIKey, acoustIdUserAPIKey, getDuration(), getChromaprintFingerprint() };
    if(musicBrainzRecordingId.empty())
    {
        TagMap tagMap;
        tagMap.setTitle(m_title);
        tagMap.setArtist(m_artist);
        tagMap.setAlbum(m_album);
        tagMap.setYear(std::to_string(m_year));
        tagMap.setTrack(std::to_string(m_track));
        tagMap.setAlbumArtist(m_albumArtist);
        return submission.submitTagMetadata(tagMap);
    }
    return submission.submitMusicBrainzRecordingId(musicBrainzRecordingId);
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



