#pragma once

#include <string>
#include <unordered_map>

namespace NickvisionTagger::Models
{
	class AcoustIdSubmission
	{
	public:
		AcoustIdSubmission(const std::string& clientAPIKey, const std::string& userAPIKey, int duration, const std::string& fingerprint);
		static bool checkIfUserAPIKeyValid(const std::string& clientAPIKey, const std::string& userAPIKey);
		bool submitMusicBrainzRecordingId(const std::string& musicBrainzRecordingId);
		bool submitTagMetadata(const std::unordered_map<std::string, std::string>& tagMap);

	private:
		std::string m_lookupUrl;
		bool submit();

	};
}