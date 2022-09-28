#pragma once

#include <chrono>
#include <string>

namespace NickvisionTagger::Models
{
    enum class AcoustIdQueryStatus
    {
    	OK,
    	Error,
    	CurlError,
    	AcoustIdError,
		JsonError,
		NoResult
    };

    class AcoustIdQuery
    {
    public:
    	AcoustIdQuery(int duration, const std::string& fingerprint);
		AcoustIdQueryStatus getStatus() const;
		std::string getRecordingId() const;
		AcoustIdQueryStatus lookup();

    private:
    	static int m_requestCount;
		static std::chrono::time_point<std::chrono::system_clock> m_lastRequestTime;
    	std::string m_lookupUrl;
		AcoustIdQueryStatus m_status;
		std::string m_recordingId;
    };
}