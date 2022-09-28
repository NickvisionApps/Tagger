#pragma once

#include <chrono>
#include <string>

namespace NickvisionTagger::Models
{
    enum class MusicBrainzRecordingQueryStatus
    {
    	OK,
    	Error,
    	CurlError,
    	MusicBrainzError,
		JsonError,
		NoResult
    };

    class MusicBrainzRecordingQuery
    {
    public:
    	MusicBrainzRecordingQuery(const std::string& recordingId);
		MusicBrainzRecordingQueryStatus getStatus() const;
		MusicBrainzRecordingQueryStatus lookup();

    private:
    	static int m_requestCount;
		static std::chrono::time_point<std::chrono::system_clock> m_lastRequestTime;
    	std::string m_lookupUrl;
		MusicBrainzRecordingQueryStatus m_status;
    };
}