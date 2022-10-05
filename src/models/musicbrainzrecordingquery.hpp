#pragma once

#include <chrono>
#include <string>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Models
{
	/**
	 * A model representing a recording query from MusicBrainz
	 */
    class MusicBrainzRecordingQuery
    {
    public:
    	/**
    	 * Constructs a MusicBrainzRecordingQuery
    	 *
    	 * @param recordingId The MusicBrainz recording id
    	 */
    	MusicBrainzRecordingQuery(const std::string& recordingId);
		/**
		 * Gets the title from the query
		 *
		 * @returns The title from the query
		 */
		const std::string& getTitle() const;
		/**
		 * Gets the artist from the query
		 *
		 * @returns The artist from the query
		 */
		const std::string& getArtist() const;
		/**
		 * Gets the album from the query
		 *
		 * @returns The album from the query
		 */
		const std::string& getAlbum() const;
		/**
		 * Gets the year from the query
		 *
		 * @returns The year from the query
		 */
		unsigned int getYear() const;
		/**
		 * Gets the album artist from the query
		 *
		 * @returns The album artist from the query
		 */
		const std::string& getAlbumArtist() const;
		/**
		 * Gets the genre from the query
		 *
		 * @returns The genre from the query
		 */
		const std::string& getGenre() const;
		/**
		 * Gets the album art from the query
		 *
		 * @returns The album art from the query
		 */
		const TagLib::ByteVector& getAlbumArt() const;
		/**
		 * Runs the query
		 *
		 * @returns True if the query was successful, else false
		 */
		bool lookup();

    private:
    	static int m_requestCount;
		static std::chrono::time_point<std::chrono::system_clock> m_lastRequestTime;
    	std::string m_lookupUrl;
		std::string m_title;
		std::string m_artist;
		std::string m_album;
		unsigned int m_year;
		std::string m_albumArtist;
		std::string m_genre;
		TagLib::ByteVector m_albumArt;
    };
}