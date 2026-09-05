namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The data required to present the account registration form in the Acme Config block.
    /// </summary>
    public class AcmeConfigRegistrationBag
    {
        /// <summary>
        /// Gets or sets the e-mail address to pre-fill the registration form with.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the testing server should be used.
        /// </summary>
        public bool TestMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an account already exists. When
        /// true a warning is shown that re-registering breaks existing renewals.
        /// </summary>
        public bool HasExistingAccount { get; set; }

        /// <summary>
        /// Gets or sets the URL of the certificate provider's terms of service.
        /// </summary>
        public string TermsOfServiceUrl { get; set; }
    }
}
