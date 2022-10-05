#pragma once

#include <chrono>
#include <string>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Models
{
	/**
	 * A model representing a release query from MusicBrainz
	 */
    class MusicBrainzReleaseQuery
    {
    public:
    	/**
    	 * Constructs a MusicBrainzReleaseQuery
    	 *
    	 * @param releaseId The MusicBrainz release id
    	 */
    	MusicBrainzReleaseQuery(const std::string& release);
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
		std::string m_releaseId;
    	std::string m_lookupUrl;
    	std::string m_lookupUrlAlbumArt;
		std::string m_title;
		std::string m_artist;
		TagLib::ByteVector m_albumArt;
    };
}