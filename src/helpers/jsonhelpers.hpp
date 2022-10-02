#pragma once

#include <string>
#include <json/json.h>

namespace NickvisionTagger::Helpers::JsonHelpers
{
	/**
	 * Parses a Json::Value from an std::string
	 *
	 * @param s The string to parse
	 * @returns The Json::Value from the string
	 */
	Json::Value getValueFromString(const std::string& s);
}