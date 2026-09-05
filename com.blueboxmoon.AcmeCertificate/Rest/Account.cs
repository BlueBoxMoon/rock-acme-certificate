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
        /// The Orders
        /// </summary>
        [JsonProperty( "orders" )]
        public string Orders { get; set; }

        /// <summary>
        /// A list of contacts that are associated with this account.
        /// </summary>
        [JsonProperty( "contact" )]
        public List<string> Contact { get; set; }

        /// <summary>
        /// The status of this account pf type AccountStatus.
        /// </summary>
        [JsonProperty( "Status" )]
        public string Status { get; set; }
    }
}
