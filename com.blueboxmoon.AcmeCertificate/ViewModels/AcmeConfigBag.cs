namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The account configuration state shown by the Acme Config block.
    /// </summary>
    public class AcmeConfigBag
    {
        /// <summary>
        /// Gets or sets the e-mail address the account was registered with.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account uses the testing server.
        /// </summary>
        public bool TestMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IIS changes are skipped and the
        /// certificate is provided for manual installation.
        /// </summary>
        public bool OfflineMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an account has been registered.
        /// </summary>
        public bool IsRegistered { get; set; }
    }
}
