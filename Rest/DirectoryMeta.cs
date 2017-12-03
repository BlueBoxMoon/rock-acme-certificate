using Newtonsoft.Json;

namespace com.blueboxmoon.AcmeCertificate.Rest
{
    /// <summary>
    /// Meta-data that is returned with the Directory response.
    /// </summary>
    public class DirectoryMeta
    {
        /// <summary>
        /// The TOS that the user must agree to in order to create a new account.
        /// </summary>
        [JsonProperty( "terms-of-service" )]
        public string TermsOfService { get; set; }
    }
}
