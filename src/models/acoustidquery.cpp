#include "acoustidquery.hpp"
#include <chrono>
#include <thread>
#include <json/json.h>
#include "../helpers/curlhelpers.hpp"

using namespace NickvisionTagger::Helpers;
using namespace NickvisionTagger::Models;

int AcoustIdQuery::m_requestCount = 0;

AcoustIdQuery::AcoustIdQuery(const std::string& lookupUrl) : m_lookupUrl{ lookupUrl }, m_status{ AcoustIdQueryStatus::Error }
{

}

AcoustIdQueryStatus AcoustIdQuery::getStatus() const
{
    return m_status;
}

AcoustIdQueryStatus AcoustIdQuery::lookup()
{
    if(m_requestCount == 3) //AcoustId has rate limit of 3 requests/second
    {
        std::this_thread::sleep_for(std::chrono::seconds(1));
        m_requestCount = 0;
    }
    std::string response{ CurlHelpers::getResponse(m_lookupUrl) };
    m_requestCount++;
    if(!response.empty())
    {
        Json::Value json{ response };
        m_status = AcoustIdQueryStatus::OK;
    }
    return m_status;
}
