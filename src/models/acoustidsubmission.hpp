#pragma once

#include <string>
#include "tagmap.hpp"

namespace NickvisionTagger::Models
{
	/**
	 * A model representing a submission to AcoustId
	 */
	class AcoustIdSubmission
	{
	public:
		/**
		 * Constructs an AcoustIdSubmission
		 *
		 * @param clientAPIKey The AcoustId client api key
		 * @param userAPIKey The AcoudtId user api key
    	 * @param duration The duration of a song in seconds
    	 * @param fingerprint The chromaprint fingerprint of a song
		 */
		AcoustIdSubmission(const std::string& clientAPIKey, const std::string& userAPIKey, int duration, const std::string& fingerprint);
		/**
		 * Checks if an AcoustId user api key is valid
		 *
		 * @param clientAPIKey The AcoustId client api key
		 * @param userAPIKey The AcoudtId user api key
		 * @returns True if valid, else false
		 */
		static bool checkIfUserAPIKeyValid(const std::string& clientAPIKey, const std::string& userAPIKey);
		/**
		 * Submits a fingerprint to AcoustId associated with a MusicBrainzRecordingId
		 *
		 * @param musicBrainzRecordingId The MusicBrainz recording id
		 * @returns True if the submission was successful, else false
		 */
		bool submitMusicBrainzRecordingId(const std::string& musicBrainzRecordingId);
		/**
		 * Submits a fingerprint to AcoustId associated with tag metadata
		 *
		 * @param tagMap The TagMap
		 * @returns True if the submission was successful, else false
		 */
		bool submitTagMetadata(const TagMap& tagMap);

	private:
		std::string m_lookupUrl;
		std::string m_clientAPIKey;
		/**
		 * Submits a fingerprint to AcoustId
		 *
		 * @returns True if the submission was successful, else false
		 */
		bool submit();

	};
}