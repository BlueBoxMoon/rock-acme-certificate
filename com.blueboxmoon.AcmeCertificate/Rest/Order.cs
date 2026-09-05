using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// An order identifies the overall status of a request for validation on a set of domains.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The identifiers to be authorized.
        /// </summary>
        [JsonProperty( "identifiers" )]
        public List<Identifier> Identifiers { get; set; }

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
        /// Gets or sets the not before date.
        /// </summary>
        public DateTime? NotBefore { get; set; }

        /// <summary>
        /// Gets or sets the not after date.
        /// </summary>
        public DateTime? NotAfter { get; set; }

        /// <summary>
        /// A list of authorizations to be completed.
        /// </summary>
        [JsonProperty( "authorizations" )]
        public List<string> Authorizations { get; set; }

        /// <summary>
        /// Gets or sets the finalize endpoint.
        /// </summary>
        [JsonProperty( "finalize" )]
        public string Finalize { get; set; }

        /// <summary>
        /// Gets or sets the certificate endpoint.
        /// </summary>
        public string Certificate { get; set; }
    }
}
