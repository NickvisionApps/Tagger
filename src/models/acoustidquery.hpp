#pragma once

#include <chrono>
#include <string>

namespace NickvisionTagger::Models
{
	/**
	 * Statuses for the AcoustIdQuery
	 */
    enum class AcoustIdQueryStatus
    {
    	OK,
    	CurlError,
    	AcoustIdError,
		NoResult
    };

	/**
	 * A model representing a query from AcoustId
	 */
    class AcoustIdQuery
    {
    public:
    	/**
    	 * Constructs an AcoustIdQuery
    	 *
    	 * @param duration The duration of a song in seconds
    	 * @param fingerprint The chromaprint fingerprint of a song
    	 */
    	AcoustIdQuery(int duration, const std::string& fingerprint);
    	/**
    	 * Gets the status of the query
    	 *
    	 * @returns The status of the query
    	 */
		AcoustIdQueryStatus getStatus() const;
		/**
		 * Gets the recording id from the query
		 *
		 * @returns The recording id from the query
		 */
		const std::string& getRecordingId() const;
		/**
		 * Runs the query
		 *
		 * @returns The status of the query
		 */
		AcoustIdQueryStatus lookup();

    private:
    	static int m_requestCount;
		static std::chrono::time_point<std::chrono::system_clock> m_lastRequestTime;
    	std::string m_lookupUrl;
		AcoustIdQueryStatus m_status;
		std::string m_recordingId;
    };
}