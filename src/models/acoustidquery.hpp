#pragma once

#include <chrono>
#include <string>
#include "appinfo.hpp"

namespace NickvisionTagger::Models
{
	/**
	 * A model representing a query from AcoustId
	 */
    class AcoustIdQuery
    {
    public:
    	/**
    	 * Constructs an AcoustIdQuery
    	 *
    	 * @param clientAPIKey The AcoustId client api key
    	 * @param duration The duration of a song in seconds
    	 * @param fingerprint The chromaprint fingerprint of a song
    	 */
    	AcoustIdQuery(const std::string& clientAPIKey, int duration, const std::string& fingerprint);
		/**
		 * Gets the recording id from the query
		 *
		 * @returns The recording id from the query
		 */
		const std::string& getRecordingId() const;
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
		std::string m_recordingId;
    };
}