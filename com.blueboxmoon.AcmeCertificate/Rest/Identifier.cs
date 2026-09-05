using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// A generic identifier that is used to identify various objects.
    /// </summary>
    public class Identifier
    {
        /// <summary>
        /// The type of object this is identifying (e.g. "dns").
        /// </summary>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// The value being identified (e.g. "contoso.com").
        /// </summary>
        [JsonProperty( "value" )]
        public string Value { get; set; }
    }
}
