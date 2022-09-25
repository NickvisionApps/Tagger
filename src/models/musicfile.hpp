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

namespace NickvisionTagger::Models
{
    /**
     * A model of a music file
     */
    class MusicFile
    {
    public:
    	/**
    	 * Constructs a MusicFile
    	 *
    	 * @param path The path of the music file
    	 * @throws std::invalid_argument Thrown when the path is an invalid music file
    	 */
    	MusicFile(const std::filesystem::path& path);
    	/**
    	 * Destroys the MusicFile
    	 */
    	~MusicFile();
    	/**
    	 * Gets the path of the music file
    	 *
    	 * @returns The path of the music file
    	 */
    	const std::filesystem::path& getPath() const;
	/**
	 * Gets the filename of the music file (includes the dot extension)
	 *
	 * @returns The filename of the music file
	 */
    	std::string getFilename() const;
    	/**
    	 * Sets the filanem of the music file (appends the dot extension of the previous filename if one is not included)
    	 *
    	 * @param filename The new filename of the music file
    	 */
    	void setFilename(const std::string& filename);
    	/**
    	 * Gets the title of the music file
    	 *
    	 * @returns The title of the music file
    	 */
    	std::string getTitle() const;
    	/**
    	 * Sets the title of the music file
    	 *
    	 * @param title The new title of the music file
    	 */
    	void setTitle(const std::string& title);
	/**
	 * Gets the artist of the music file
	 *
	 * @returns The artist of the music file
	 */
    	std::string getArtist() const;
    	/**
    	 * Sets the artist of the music file
    	 *
    	 * @param artist The new artist of the music file
    	 */
    	void setArtist(const std::string& artist);
    	/**
    	 * Gets the album of the music file
    	 *
    	 * @returns The album of the music file
    	 */
    	std::string getAlbum() const;
    	/**
    	 * Sets the album of the music file
    	 *
    	 * @param album The new album of the music file
    	 */
    	void setAlbum(const std::string& album);
    	/**
    	 * Gets the year of the music file
    	 *
    	 * @returns The year of the music file
    	 */
    	unsigned int getYear() const;
    	/**
    	 * Sets the year of the music file
    	 *
    	 * @param year The new year of the music file
    	 */
    	void setYear(unsigned int year);
    	/**
    	 * Gets the track of the music file
    	 *
    	 * @returns The track of the music file
    	 */
    	unsigned int getTrack() const;
    	/**
    	 * Sets the track of the music file
    	 *
    	 * @param track The new track of the music file
    	 */
    	void setTrack(unsigned int track);
    	/**
    	 * Gets the album artist of the music file
    	 *
    	 * @returns The album artist of the music file
    	 */
    	std::string getAlbumArtist() const;
    	/**
    	 * Sets the album artist of the music file
    	 *
    	 * @param albumArtist The new album artist of the music file
    	 */
    	void setAlbumArtist(const std::string& albumArtist);
    	/**
    	 * Gets the genre of the music file
    	 *
    	 * @returns The genre of the music file
    	 */
    	std::string getGenre() const;
    	/**
    	 * Sets the genre of the music file
    	 *
    	 * @param genre The new genre of the music file
    	 */
    	void setGenre(const std::string& genre);
    	/**
    	 * Gets the comment of the music file
    	 *
    	 * @returns The comment of the music file
    	 */
    	std::string getComment() const;
    	/**
    	 * Sets the comment of the music file
    	 *
    	 * @param comment The new comment of the music file
    	 */
    	void setComment(const std::string& comment);
    	/**
    	 * Gets the album art of the music file (as TagLib::ByteVector)
    	 *
    	 * @returns The album art of the music file
    	 */
	TagLib::ByteVector getAlbumArt() const;
	/**
	 * Sets the album art of the music file
	 *
	 * @param albumArt The new album art of the music file
	 */
	void setAlbumArt(const TagLib::ByteVector& albumArt);
	/**
	 * Gets the duration of the music file (in seconds)
	 *
	 * @returns The duration of the music file
	 */
	int getDuration() const;
	/**
	 * Gets the duration of the music file as a human-readable string (hh::mm::ss)
	 *
	 * @returns The duration of the music file as a human-readable string
	 */
	std::string getDurationAsString() const;
	/**
	 * Gets the file size of the music file (in bytes)
	 *
	 * @returns The file size of the music file
	 */
	std::uintmax_t getFileSize() const;
	/**
	 * Gets the file size of the music file as a human-readable string (0 MB)
	 *
	 * @returns The file size of the music file as a human-readable string
	 */
	std::string getFileSizeAsString() const;
	/**
	 * Gets the chromaprint fingerprint for the music file
	 *
	 * @returns The chromaprint fingerprint for the music file
	 */
	const std::string& getChromaprintFingerprint();
	/**
	 * Gets the AcoustId lookup url for the music file
	 *
	 * @returns The AcoustId lookup url for the music file
	 */
	std::string getAcoustIdLookupUrl();
	/**
	 * Saves the tag of the music file
	 *
	 * @param preserveModificationTimeStamp Set true to preserve the modification time stamp of the file, else false
	 */
	void saveTag(bool preserveModificationTimeStamp);
	/**
	 * Removes the tag of the music file
	 *
	 * @param preserveModificationTimeStamp Set true to preserve the modification time stamp of the file, else false
	 */
	void removeTag(bool preserveModificationTimeStamp);
	/**
	 * Uses the music file's filename to fill in tag information based on the format string
	 *
	 * @param formatString The format string
	 * @param preserveModificationTimeStamp Set true to preserve the modification time stamp of the file, else false
	 * @returns True if the operation was successful, else false
	 */
	bool filenameToTag(const std::string& formatString, bool preserveModificationTimeStamp);
	/**
	 * Uses the music file's tag to set the file's filename in the format of the format string
	 *
	 * @param formatString The format string
	 * @returns True if the operation was successful, else false
	 */
	bool tagToFilename(const std::string& formatString);
	/**
	 * Compares this.filename to toCompare.filename via less-than
	 *
	 * @param toCompare The MusicFile to compare
	 * @returns True if this.filename < toCompare.filename, else false
	 */
	bool operator<(const MusicFile& toCompare) const;
	/**
	 * Compares this.filename to toCompare.filename via greater-than
	 *
	 * @param toCompare The MusicFile to compare
	 * @returns True if this.filename > toCompare.filename, else false
	 */
	bool operator>(const MusicFile& toCompare) const;
	/**
	 * Compares this.path to toCompare.path via equals
	 *
	 * @param toCompare The MusicFile to compare
	 * @returns True if this.path == toCompare.path, else false
	 */
	bool operator==(const MusicFile& toCompare) const;
	/**
	 * Compares this.path to toCompare.path via not equals
	 *
	 * @param toCompare The MusicFile to compare
	 * @returns True if this.path != toCompare.path, else false
	 */
	bool operator!=(const MusicFile& toCompare) const;

    private:
	std::filesystem::path m_path;
	std::string m_dotExtension;
        std::filesystem::file_time_type m_modificationTimeStamp;
        std::shared_ptr<TagLib::MPEG::File> m_fileMP3;
        std::shared_ptr<TagLib::Ogg::Vorbis::File> m_fileOGG;
        std::shared_ptr<TagLib::FLAC::File> m_fileFLAC;
        std::shared_ptr<TagLib::ASF::File> m_fileWMA;
        std::shared_ptr<TagLib::RIFF::WAV::File> m_fileWAV;
        std::string m_fingerprint;
    };
}