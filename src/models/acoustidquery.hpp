#pragma once

#include <string>

namespace NickvisionTagger::Models
{
    enum class AcoustIdQueryStatus
    {
    	Error = 0,
    	OK
    };

    class AcoustIdQuery
    {
    public:
    	AcoustIdQuery(int duration, const std::string& fingerprint);
		AcoustIdQueryStatus getStatus() const;
		AcoustIdQueryStatus lookup();

    private:
    	static int m_requestCount;
    	std::string m_lookupUrl;
		AcoustIdQueryStatus m_status;
    };
}