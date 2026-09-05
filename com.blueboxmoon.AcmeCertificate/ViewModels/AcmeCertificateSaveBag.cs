using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The data submitted when saving a certificate configuration.
    /// </summary>
    public class AcmeCertificateSaveBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the certificate.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the old certificate should be
        /// removed from the certificate store after renewal.
        /// </summary>
        public bool RemoveOldCertificate { get; set; }

        /// <summary>
        /// Gets or sets the domain names associated with the certificate.
        /// </summary>
        public List<string> Domains { get; set; }

        /// <summary>
        /// Gets or sets the IIS bindings associated with the certificate.
        /// </summary>
        public List<BindingBag> Bindings { get; set; }
    }
}
