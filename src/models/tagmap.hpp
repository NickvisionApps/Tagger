#pragma once

#include <string>

namespace NickvisionTagger::Models
{
	class TagMap
	{
	public:
		TagMap();
		const std::string& getFilename() const;
		void setFilename(const std::string& filename);
		const std::string& getTitle() const;
		void setTitle(const std::string& title);
		const std::string& getArtist() const;
		void setArtist(const std::string& artist);
		const std::string& getAlbum() const;
		void setAlbum(const std::string& album);
		const std::string& getYear() const;
		void setYear(const std::string& year);
		const std::string& getTrack() const;
		void setTrack(const std::string& track);
		const std::string& getAlbumArtist() const;
		void setAlbumArtist(const std::string& albumArtist);
		const std::string& getGenre() const;
		void setGenre(const std::string& genre);
		const std::string& getComment() const;
		void setComment(const std::string& comment);
		const std::string& getAlbumArt() const;
		void setAlbumArt(const std::string& albumArt);
		const std::string& getDuration() const;
		void setDuration(const std::string& duration);
		const std::string& getFingerprint() const;
		void setFingerprint(const std::string& fingerprint);
		const std::string& getFileSize() const;
		void setFileSize(const std::string& fileSize);

	private:
		std::string m_filename;
		std::string m_title;
		std::string m_artist;
		std::string m_album;
		std::string m_year;
		std::string m_track;
		std::string m_albumArtist;
		std::string m_genre;
		std::string m_comment;
		std::string m_albumArt;
		std::string m_duration;
		std::string m_fingerprint;
		std::string m_fileSize;

	};
}