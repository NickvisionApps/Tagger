#include "jsonhelpers.hpp"
#include <json/reader.h>

using namespace NickvisionTagger::Helpers;

Json::Value JsonHelpers::getValueFromString(const std::string& s)
{
    Json::CharReaderBuilder builder;
    Json::CharReader* reader{ builder.newCharReader() };
    Json::Value value;
    reader->parse(s.c_str(), s.c_str() + s.size(), &value, nullptr);
    delete reader;
    return value;
}
