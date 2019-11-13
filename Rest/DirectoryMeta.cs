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
        [JsonProperty( "termsOfService" )]
        public string TermsOfService { get; set; }

        /// <summary>
        /// Gets or sets the website to direct the user to for more information.
        /// </summary>
        [JsonProperty( "website" )]
        public string Website { get; set; }
    }
}
