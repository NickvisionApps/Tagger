#pragma once

#include <string>

namespace NickvisionTagger::Helpers::CurlHelpers
{
	/**
	 * Downloads a file from the internet
	 *
	 * @param url The url of the file
	 * @param savePath The path of where to save the downloaded file
	 * @param userAgent The UserAgent to use for curl
	 * @returns True if the download was successful, else false
	 */
	bool downloadFile(const std::string& url, const std::string& savePath, const std::string& userAgent = "");
	/**
	 * Gets a response string from a get request from the internet
	 *
	 * @param url The url of the get request
	 * @param userAgent The UserAgent to use for curl
	 * @returns The response string from the get request. An empty string if there was an error
	 */
	std::string getResponseString(const std::string& url, const std::string& userAgent = "");
}