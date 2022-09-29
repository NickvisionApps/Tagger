#pragma once

#include <chrono>
#include <string>

namespace NickvisionTagger::Models
{
	/**
	 * Statuses for the MusicBrainzReleaseQuery
	 */
    enum class MusicBrainzReleaseQueryStatus
    {
    	OK,
    	CurlError,
    	MusicBrainzError,
		NoResult
    };

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
    	 * Gets the status of the query
    	 *
    	 * @returns The status of the query
    	 */
		MusicBrainzReleaseQueryStatus getStatus() const;
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
		 * Runs the query
		 *
		 * @returns The status of the query
		 */
		MusicBrainzReleaseQueryStatus lookup();

    private:
    	static int m_requestCount;
		static std::chrono::time_point<std::chrono::system_clock> m_lastRequestTime;
    	std::string m_lookupUrl;
		MusicBrainzReleaseQueryStatus m_status;
		std::string m_title;
		std::string m_artist;
    };
}