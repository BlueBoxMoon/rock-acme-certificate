using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// The JWK message structure that will be sent to the server.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The protected data. This contains the data that identifies us.
        /// </summary>
        [JsonProperty( "protected" )]
        public object Protected { get; set; }

        /// <summary>
        /// The payload of the message. This contains the data of the request itself.
        /// </summary>
        [JsonProperty( "payload" )]
        public object Payload { get; set; }

        /// <summary>
        /// The signature data. This provides verification that neither Protected nor
        /// Payload were modified.
        /// </summary>
        [JsonProperty( "signature" )]
        public string Signature { get; set; }
    }
}
