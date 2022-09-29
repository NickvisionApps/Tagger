#include "musicfile.hpp"
#include <array>
#include <cstdio>
#include <sstream>
#include <stdexcept>
#include <taglib/textidentificationframe.h>
#include "acoustidquery.hpp"
#include "musicbrainzrecordingquery.hpp"
#include "../helpers/mediahelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

MusicFile::MusicFile(const std::filesystem::path& path) : m_path{ path }, m_dotExtension{ m_path.extension() }, m_modificationTimeStamp{ std::filesystem::last_write_time(m_path) }, m_fingerprint{ }
{
    if(m_dotExtension == ".mp3")
    {
        m_fileMP3 = std::make_shared<TagLib::MPEG::File>(m_path.c_str());
    }
    else if(m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG = std::make_shared<TagLib::Ogg::Vorbis::File>(m_path.c_str());
    }
    else if(m_dotExtension == ".flac")
    {
        m_fileFLAC = std::make_shared<TagLib::FLAC::File>(m_path.c_str());
    }
    else if(m_dotExtension == ".wma")
    {
        m_fileWMA = std::make_shared<TagLib::ASF::File>(m_path.c_str());
    }
    else if(m_dotExtension == ".wav")
    {
        m_fileWAV = std::make_shared<TagLib::RIFF::WAV::File>(m_path.c_str());
    }
    else
    {
        throw std::invalid_argument("Invalid Path. The path is not a valid music file.");
    }
}

MusicFile::~MusicFile()
{
    m_fileMP3.reset();
    m_fileOGG.reset();
    m_fileFLAC.reset();
    m_fileWMA.reset();
    m_fileWAV.reset();
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
        m_fileMP3.reset();
        m_fileOGG.reset();
        m_fileFLAC.reset();
        m_fileWMA.reset();
        m_fileWAV.reset();
        std::filesystem::rename(m_path, newPath);
        m_path = newPath;
        if(m_dotExtension == ".mp3")
        {
            m_fileMP3 = std::make_shared<TagLib::MPEG::File>(m_path.c_str());
        }
        else if(m_dotExtension == ".ogg" || m_dotExtension == ".opus")
        {
            m_fileOGG = std::make_shared<TagLib::Ogg::Vorbis::File>(m_path.c_str());
        }
        else if(m_dotExtension == ".flac")
        {
            m_fileFLAC = std::make_shared<TagLib::FLAC::File>(m_path.c_str());
        }
        else if(m_dotExtension == ".wma")
        {
            m_fileWMA = std::make_shared<TagLib::ASF::File>(m_path.c_str());
        }
        else if(m_dotExtension == ".wav")
        {
            m_fileWAV = std::make_shared<TagLib::RIFF::WAV::File>(m_path.c_str());
        }
    }
}

std::string MusicFile::getTitle() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->title().to8Bit(true);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->title().to8Bit(true);
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->title().to8Bit(true);
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->title().to8Bit(true);
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->title().to8Bit(true);
    }
    return "";
}

void MusicFile::setTitle(const std::string& title)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setTitle(title);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setTitle(title);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setTitle(title);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setTitle(title);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setTitle(title);
    }
}

std::string MusicFile::getArtist() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->artist().to8Bit(true);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->artist().to8Bit(true);
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->artist().to8Bit(true);
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->artist().to8Bit(true);
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->artist().to8Bit(true);
    }
    return "";
}

void MusicFile::setArtist(const std::string& artist)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setArtist(artist);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setArtist(artist);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setArtist(artist);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setArtist(artist);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setArtist(artist);
    }
}

std::string MusicFile::getAlbum() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->album().to8Bit(true);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->album().to8Bit(true);
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->album().to8Bit(true);
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->album().to8Bit(true);
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->album().to8Bit(true);
    }
    return "";
}

void MusicFile::setAlbum(const std::string& album)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setAlbum(album);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setAlbum(album);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setAlbum(album);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setAlbum(album);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setAlbum(album);
    }
}

unsigned int MusicFile::getYear() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->year();
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->year();
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->year();
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->year();
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->year();
    }
    return 0;
}

void MusicFile::setYear(unsigned int year)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setYear(year);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setYear(year);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setYear(year);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setYear(year);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setYear(year);
    }
}

unsigned int MusicFile::getTrack() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->track();
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->track();
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->track();
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->track();
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->track();
    }
    return 0;
}

void MusicFile::setTrack(unsigned int track)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setTrack(track);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setTrack(track);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setTrack(track);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setTrack(track);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setTrack(track);
    }
}

std::string MusicFile::getAlbumArtist() const
{
    if (m_dotExtension == ".mp3")
    {
        const TagLib::ID3v2::FrameList& frameList{ m_fileMP3->ID3v2Tag(true)->frameList("TPE2") };
        if (!frameList.isEmpty())
        {
            return frameList.front()->toString().to8Bit(true);
        }
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        const TagLib::Ogg::FieldListMap& fieldListMap{ m_fileOGG->tag()->fieldListMap() };
        if (fieldListMap.contains("ALBUMARTIST"))
        {
            const TagLib::StringList& stringList{ fieldListMap["ALBUMARTIST"] };
            if (!stringList.isEmpty())
            {
                return stringList[0].to8Bit(true);
            }
        }
    }
    else if (m_dotExtension == ".flac")
    {
        const TagLib::Ogg::FieldListMap& fieldListMap{ m_fileFLAC->xiphComment(true)->fieldListMap() };
        if (fieldListMap.contains("ALBUMARTIST"))
        {
            const TagLib::StringList& stringList{ fieldListMap["ALBUMARTIST"] };
            if (!stringList.isEmpty())
            {
                return stringList[0].to8Bit(true);
            }
        }
    }
    else if (m_dotExtension == ".wma")
    {
        const TagLib::ASF::AttributeListMap& attributeListMap{ m_fileWMA->tag()->attributeListMap() };
        if (attributeListMap.contains("ALBUMARTIST"))
        {
            const TagLib::ASF::AttributeList& attributeList{ attributeListMap["ALBUMARTIST"] };
            if (!attributeList.isEmpty())
            {
                return attributeList[0].toString().to8Bit(true);
            }
        }
    }
    else if (m_dotExtension == ".wav")
    {
        const TagLib::ID3v2::FrameList& frameList{ m_fileWAV->ID3v2Tag()->frameList("TPE2") };
        if (!frameList.isEmpty())
        {
            return frameList.front()->toString().to8Bit(true);
        }
    }
    return "";
}

void MusicFile::setAlbumArtist(const std::string& albumArtist)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->removeFrames("TPE2");
        TagLib::ID3v2::TextIdentificationFrame* textFrame{ new TagLib::ID3v2::TextIdentificationFrame("TPE2", TagLib::String::UTF8) };
        textFrame->setText(albumArtist);
        m_fileMP3->ID3v2Tag(true)->addFrame(textFrame);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->addField("ALBUMARTIST", albumArtist);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->addField("ALBUMARTIST", albumArtist);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->removeItem("ALBUMARTIST");
        m_fileWMA->tag()->addAttribute("ALBUMARTIST", { albumArtist });
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->removeFrames("TPE2");
        TagLib::ID3v2::TextIdentificationFrame* textFrame{ new TagLib::ID3v2::TextIdentificationFrame("TPE2", TagLib::String::UTF8) };
        textFrame->setText(albumArtist);
        m_fileWAV->ID3v2Tag()->addFrame(textFrame);
    }
}

std::string MusicFile::getGenre() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->genre().to8Bit(true);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->genre().to8Bit(true);
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->genre().to8Bit(true);
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->genre().to8Bit(true);
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->genre().to8Bit(true);
    }
    return "";
}

void MusicFile::setGenre(const std::string& genre)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setGenre(genre);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setGenre(genre);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setGenre(genre);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setGenre(genre);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setGenre(genre);
    }
}

std::string MusicFile::getComment() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->ID3v2Tag(true)->comment().to8Bit(true);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->tag()->comment().to8Bit(true);
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->xiphComment(true)->comment().to8Bit(true);
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->tag()->comment().to8Bit(true);
    }
    else if (m_dotExtension == ".wav")
    {
        return m_fileWAV->ID3v2Tag()->comment().to8Bit(true);
    }
    return "";
}

void MusicFile::setComment(const std::string& comment)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->setComment(comment);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->setComment(comment);
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->setComment(comment);
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->setComment(comment);
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->setComment(comment);
    }
}

TagLib::ByteVector MusicFile::getAlbumArt() const
{
    if (m_dotExtension == ".mp3")
    {
        const TagLib::ID3v2::FrameList& frameList{ m_fileMP3->ID3v2Tag(true)->frameList("APIC") };
        if (!frameList.isEmpty())
        {
            return ((TagLib::ID3v2::AttachedPictureFrame*)frameList.front())->picture();
        }
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        const TagLib::List<TagLib::FLAC::Picture*>& pictureList{ m_fileOGG->tag()->pictureList() };
        if (!pictureList.isEmpty())
        {
            return pictureList[0]->data();
        }
    }
    else if (m_dotExtension == ".flac")
    {
        const TagLib::List<TagLib::FLAC::Picture*>& pictureList{ m_fileFLAC->xiphComment(true)->pictureList() };
        if (!pictureList.isEmpty())
        {
            return pictureList[0]->data();
        }
    }
    else if (m_dotExtension == ".wma")
    {
        const TagLib::ASF::AttributeListMap& attributeListMap{ m_fileWMA->tag()->attributeListMap() };
        if (attributeListMap.contains("WM/Picture"))
        {
            const TagLib::ASF::AttributeList& attributeList{ attributeListMap["WM/Picture"] };
            if (!attributeList.isEmpty())
            {
                return attributeList[0].toPicture().picture();
            }
        }
    }
    else if (m_dotExtension == ".wav")
    {
        const TagLib::ID3v2::FrameList& frameList{ m_fileWAV->ID3v2Tag()->frameList("APIC") };
        if (!frameList.isEmpty())
        {
            return ((TagLib::ID3v2::AttachedPictureFrame*)frameList.front())->picture();
        }
    }
    return {};
}

void MusicFile::setAlbumArt(const TagLib::ByteVector& albumArt)
{
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->ID3v2Tag(true)->removeFrames("APIC");
        if (!albumArt.isEmpty())
        {
            TagLib::ID3v2::AttachedPictureFrame* pictureFrame{ new TagLib::ID3v2::AttachedPictureFrame };
            pictureFrame->setType(TagLib::ID3v2::AttachedPictureFrame::Type::FrontCover);
            pictureFrame->setPicture(albumArt);
            m_fileMP3->ID3v2Tag(true)->addFrame(pictureFrame);
        }
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->tag()->removeAllPictures();
        if (!albumArt.isEmpty())
        {
            TagLib::FLAC::Picture* picture{ new TagLib::FLAC::Picture() };
            picture->setType(TagLib::FLAC::Picture::Type::FrontCover);
            picture->setData(albumArt);
            m_fileOGG->tag()->addPicture(picture);
        }
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->xiphComment(true)->removeAllPictures();
        if (!albumArt.isEmpty())
        {
            TagLib::FLAC::Picture* picture{ new TagLib::FLAC::Picture() };
            picture->setType(TagLib::FLAC::Picture::Type::FrontCover);
            picture->setData(albumArt);
            m_fileFLAC->xiphComment(true)->addPicture(picture);
        }
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->tag()->removeItem("WM/Picture");
        m_fileWMA->tag()->addAttribute("WM/Picture", { albumArt });
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->ID3v2Tag()->removeFrames("APIC");
        if (!albumArt.isEmpty())
        {
            TagLib::ID3v2::AttachedPictureFrame* pictureFrame{ new TagLib::ID3v2::AttachedPictureFrame };
            pictureFrame->setType(TagLib::ID3v2::AttachedPictureFrame::Type::FrontCover);
            pictureFrame->setPicture(albumArt);
            m_fileWAV->ID3v2Tag()->addFrame(pictureFrame);
        }
    }
}

int MusicFile::getDuration() const
{
    if (m_dotExtension == ".mp3")
    {
        return m_fileMP3->audioProperties()->lengthInSeconds();
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        return m_fileOGG->audioProperties()->lengthInSeconds();
    }
    else if (m_dotExtension == ".flac")
    {
        return m_fileFLAC->audioProperties()->lengthInSeconds();
    }
    else if (m_dotExtension == ".wma")
    {
        return m_fileWMA->audioProperties()->lengthInSeconds();
    }
    else if (m_dotExtension == ".wav")
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
        m_fileMP3->save(TagLib::MPEG::File::TagTypes::ID3v2);
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        m_fileOGG->save();
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->save();
    }
    else if (m_dotExtension == ".wma")
    {
        m_fileWMA->save();
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->save(TagLib::RIFF::WAV::File::TagTypes::ID3v2);
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
    if (m_dotExtension == ".mp3")
    {
        m_fileMP3->strip();
    }
    else if (m_dotExtension == ".ogg" || m_dotExtension == ".opus")
    {
        setTitle("");
        setArtist("");
        setAlbum("");
        setYear(0);
        setTrack(0);
        setAlbumArtist("");
        setGenre("");
        setComment("");
        setAlbumArt({});
    }
    else if (m_dotExtension == ".flac")
    {
        m_fileFLAC->strip();
    }
    else if (m_dotExtension == ".wma")
    {
        setTitle("");
        setArtist("");
        setAlbum("");
        setYear(0);
        setTrack(0);
        setAlbumArtist("");
        setGenre("");
        setComment("");
        setAlbumArt({});
    }
    else if (m_dotExtension == ".wav")
    {
        m_fileWAV->strip();
    }
    saveTag(preserveModificationTimeStamp);
}

bool MusicFile::filenameToTag(const std::string& formatString, bool preserveModificationTimeStamp)
{
    if (formatString == "%artist%- %title%")
    {
        std::size_t dashIndex{ getFilename().find("- ") };
        if (dashIndex == std::string::npos)
        {
            return false;
        }
        setArtist(getFilename().substr(0, dashIndex));
        setTitle(getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (getArtist().size() + 2)));
    }
    else if (formatString == "%title%- %artist%")
    {
        std::size_t dashIndex{ getFilename().find("- ") };
        if (dashIndex == std::string::npos)
        {
            return false;
        }
        setTitle(getFilename().substr(0, dashIndex));
        setArtist(getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (getTitle().size() + 2)));
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
            setTrack(MediaHelpers::stoui(track));
        }
        catch (...)
        {
            setTrack(0);
        }
        setTitle(getFilename().substr(dashIndex + 2, getFilename().find(m_path.extension().string()) - (track.size() + 2)));
    }
    else if (formatString == "%title%")
    {
        setTitle(getFilename().substr(0, getFilename().find(m_path.extension().string())));
    }
    else
    {
        return false;
    }
    saveTag(preserveModificationTimeStamp);
    return true;
}

bool MusicFile::tagToFilename(const std::string& formatString)
{
    if (formatString == "%artist%- %title%")
    {
        if (getArtist().empty() || getTitle().empty())
        {
            return false;
        }
        setFilename(getArtist() + "- " + getTitle() + m_path.extension().string());
    }
    else if (formatString == "%title%- %artist%")
    {
        if (getTitle().empty() || getArtist().empty())
        {
            return false;
        }
        setFilename(getTitle() + "- " + getArtist() + m_path.extension().string());
    }
    else if (formatString == "%track%- %title%")
    {
        if (getTitle().empty())
        {
            return false;
        }
        setFilename(std::to_string(getTrack()) + "- " + getTitle() + m_path.extension().string());
    }
    else if (formatString == "%title%")
    {
        if (getTitle().empty())
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

bool MusicFile::downloadMusicBrainzMetadata(bool preserveModificationTimeStamp)
{
    AcoustIdQuery acoustIdQuery{ getDuration(), getChromaprintFingerprint() };
    if(acoustIdQuery.lookup() == AcoustIdQueryStatus::OK)
    {
        MusicBrainzRecordingQuery musicBrainzQuery{ acoustIdQuery.getRecordingId() };
        if(musicBrainzQuery.lookup() == MusicBrainzRecordingQueryStatus::OK)
        {
            setTitle(musicBrainzQuery.getTitle());
            setArtist(musicBrainzQuery.getArtist());
            setAlbum(musicBrainzQuery.getAlbum());
            setYear(musicBrainzQuery.getYear());
            setAlbumArtist(musicBrainzQuery.getAlbumArtist());
            saveTag(preserveModificationTimeStamp);
            return true;
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

