using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// An Authorization identifies the overall status of a request for validation on a domain
    /// name.
    /// </summary>
    public class Authorization
    {
        /// <summary>
        /// The identifier to be authorized.
        /// </summary>
        [JsonProperty( "identifier" )]
        public Identifier Identifier { get; set; }

        /// <summary>
        /// The status of the authorization request, of type AuthorizationStatus.
        /// </summary>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// When this authorization will expire.
        /// </summary>
        [JsonProperty( "expires" )]
        public DateTime? Expires { get; set; }

        /// <summary>
        /// A list of challenges to be completed.
        /// </summary>
        [JsonProperty( "challenges" )]
        public List<Challenge> Challenges { get; set; }
    }
}
