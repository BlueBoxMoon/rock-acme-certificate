namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The additional configuration options for the Acme Certificate List block.
    /// </summary>
    public class AcmeCertificateListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether an Acme account has been
        /// registered. When no account exists the certificate list is hidden,
        /// mirroring the behavior of the legacy WebForms block.
        /// </summary>
        public bool IsAccountRegistered { get; set; }
    }
}
