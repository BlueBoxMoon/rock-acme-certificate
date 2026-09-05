using System;
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
        /// The URL for interacting with this specific challenge.
        /// </summary>
        [JsonProperty( "url" )]
        public string Url { get; set; }

        /// <summary>
        /// The server-token provided for this challenge.
        /// </summary>
        [JsonProperty( "token" )]
        public string Token { get; set; }

        [JsonProperty( "validated" )]
        public DateTime? Validated { get; set; }

        /// <summary>
        /// Contains any error that may have occurred while trying to validate.
        /// </summary>
        [JsonProperty( "error" )]
        public Error Error { get; set; }
    }
}
