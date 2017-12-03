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
        [JsonProperty( "new-reg" )]
        public string NewReg { get; set; }

        /// <summary>
        /// The endpoint to use when changing the private key of an existing account.
        /// </summary>
        [JsonProperty( "key-change" )]
        public string KeyChange { get; set; }

        /// <summary>
        /// The endpoint to use when requesting authorization of a domain.
        /// </summary>
        [JsonProperty( "new-authz" )]
        public string NewAuthz { get; set; }

        /// <summary>
        /// The endpoint to use when requesting a certificate be issued.
        /// </summary>
        [JsonProperty( "new-cert" )]
        public string NewCert { get; set; }

        /// <summary>
        /// The endpoint to use when requesting that a certificate be revoked for some reason.
        /// </summary>
        [JsonProperty( "revoke-cert" )]
        public string RevokeCert { get; set; }

        /// <summary>
        /// Any non-endpoint meta data about this server.
        /// </summary>
        [JsonProperty( "meta" )]
        public DirectoryMeta Meta { get; set; }
    }
}
