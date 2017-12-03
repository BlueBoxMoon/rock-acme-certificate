using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// An error response from the server, defined by RFC7807.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Contains the type of error that occurred.
        /// </summary>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// A generally user-friendly message about what the error was.
        /// </summary>
        [JsonProperty( "detail" )]
        public string Detail { get; set; }

        /// <summary>
        /// HTTP Status code for this error.
        /// </summary>
        [JsonProperty( "status" )]
        public int Status { get; set; }
    }
}
