using System.Collections.Generic;

using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// A validation record that indicates how a record was validated.
    /// </summary>
    public class ValidationRecord
    {
        /// <summary>
        /// The URL of this validation result.
        /// </summary>
        [JsonProperty( "url" )]
        public string Url { get; set; }

        /// <summary>
        /// The hostname that was validated.
        /// </summary>
        [JsonProperty( "hostname" )]
        public string Hostname { get; set; }

        /// <summary>
        /// The port that was used for validation.
        /// </summary>
        [JsonProperty( "port" )]
        public string Port { get; set; }

        /// <summary>
        /// The IP addresses that were resolved by DNS for the hostname.
        /// </summary>
        [JsonProperty( "addressesResolved" )]
        public List<string> AddressesResolved { get; set; }

        /// <summary>
        /// The address that was used to successfully contact our server.
        /// </summary>
        [JsonProperty( "addressUsed" )]
        public string AddressUsed { get; set; }

        /// <summary>
        /// Other IP addresses that were tried.
        /// </summary>
        [JsonProperty( "addressesTried" )]
        public List<string> AddressesTried { get; set; }
    }
}
