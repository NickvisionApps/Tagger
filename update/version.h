#pragma once

#include <string>

namespace NickvisionTagger::Update
{
    class Version
    {
    public:
        Version(const std::string& version);
        std::string toString() const;
        bool operator==(const Version& toCompare) const;
        bool operator!=(const Version& toCompare) const;
        bool operator<(const Version& toCompare) const;
        bool operator>(const Version& toCompare) const;

    private:
        int m_major;
        int m_minor;
        int m_build;
    };
}
