#pragma once

#include <string>

namespace NickvisionTagger::Models
{
    class AcoustIdQuery
    {
    public:
    	AcoustIdQuery(const std::string& lookupUrl);
	const std::string& getStatus() const;
	void lookup();

    private:
    	static int m_requestCount;
    	std::string m_lookupUrl;
	std::string m_status;
    };
}