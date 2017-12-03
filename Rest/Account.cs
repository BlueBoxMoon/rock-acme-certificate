using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// Identifies a single account object in the system.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Unique identifier for the account.
        /// </summary>
        [JsonProperty( "id" )]
        public int Id { get; set; }

        /// <summary>
        /// The JWK object that is tied to this account.
        /// </summary>
        [JsonProperty( "key" )]
        public JWK.JsonWebKey Key { get; set; }

        /// <summary>
        /// A list of contacts that are associated with this account.
        /// </summary>
        [JsonProperty( "contact" )]
        public List<string> Contact { get; set; }

        /// <summary>
        /// The agreement URL that was agreed to upon creation of the account.
        /// </summary>
        [JsonProperty( "agreement" )]
        public string Agreement { get; set; }

        /// <summary>
        /// The IP address that created the account.
        /// </summary>
        [JsonProperty( "initialIp" )]
        public string InitialIp { get; set; }

        /// <summary>
        /// When the account was created.
        /// </summary>
        [JsonProperty( "createdAt" )]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The status of this account pf type AccountStatus.
        /// </summary>
        [JsonProperty( "Status" )]
        public string Status { get; set; }
    }
}
