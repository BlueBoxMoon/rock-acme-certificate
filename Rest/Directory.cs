using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// The directory data that identifies various endpoints in this server.
    /// </summary>
    public class Directory
    {
        /// <summary>
        /// The endpoint to use for new registrations.
        /// </summary>
        [JsonProperty( "newAccount" )]
        public string NewAccount { get; set; }

        /// <summary>
        /// The endpoint to use when changing the private key of an existing account.
        /// </summary>
        [JsonProperty( "keyChange" )]
        public string KeyChange { get; set; }

        /// <summary>
        /// The endpoint to use when requesting a new Nonce.
        /// </summary>
        [JsonProperty( "newNonce" )]
        public string NewNonce { get; set; }

        /// <summary>
        /// The endpoint to use when requesting a certificate be issued.
        /// </summary>
        [JsonProperty( "newOrder" )]
        public string NewOrder { get; set; }

        /// <summary>
        /// The endpoint to use when requesting that a certificate be revoked for some reason.
        /// </summary>
        [JsonProperty( "revokeCert" )]
        public string RevokeCert { get; set; }

        /// <summary>
        /// Any non-endpoint meta data about this server.
        /// </summary>
        [JsonProperty( "meta" )]
        public DirectoryMeta Meta { get; set; }
    }
}
