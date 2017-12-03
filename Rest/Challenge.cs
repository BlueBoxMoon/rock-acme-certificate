using System.Collections.Generic;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// A challenge identifies the status of a request for validation on a domain
    /// name using a specific validation type.
    /// </summary>
    public class Challenge
    {
        /// <summary>
        /// The type of challenge this is (e.g. "http-01").
        /// </summary>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// The status of the challenge of type ChallengeStatus.
        /// </summary>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// The URI for interacting with this specific challenge.
        /// </summary>
        [JsonProperty( "uri" )]
        public string Uri { get; set; }

        /// <summary>
        /// The server-token provided for this challenge.
        /// </summary>
        [JsonProperty( "token" )]
        public string Token { get; set; }

        /// <summary>
        /// The user-provided authorization for this challenge.
        /// </summary>
        [JsonProperty( "keyAuthorization" )]
        public string KeyAuthorization { get; set; }

        /// <summary>
        /// ValidationRecords that indicate what was specifically validated.
        /// </summary>
        [JsonProperty( "validationRecord" )]
        public List<ValidationRecord> ValidationRecord { get; set; }

        /// <summary>
        /// Contains any error that may have occurred while trying to validate.
        /// </summary>
        [JsonProperty( "error" )]
        public Error Error { get; set; }
    }
}
