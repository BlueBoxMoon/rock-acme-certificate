namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// Defines information related to the account that has been registered with the Acme
    /// certificate provider.
    /// </summary>
    public class AccountData
    {
        /// <summary>
        /// The e-mail address that was used to register the account.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the orders URL.
        /// </summary>
        public string OrdersUrl { get; set; }

        /// <summary>
        /// The base64 encoded account private key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// If true, then the staging API is used and will generate fake certificates.
        /// </summary>
        public bool TestMode { get; set; }

        /// <summary>
        /// If true, then no changes to IIS will be made. The generated certificate information
        /// will be provided to the user for manual update.
        /// </summary>
        public bool OfflineMode { get; set; }
    }
}
