using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The editable domains and bindings for a certificate. Used as both the
    /// request and response for the recommend-configuration action.
    /// </summary>
    public class AcmeCertificateEditDataBag
    {
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
