using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The certificate material produced by a renewal, round-tripped between the
    /// renew and install steps of the renewal flow.
    /// </summary>
    public class CertificateDataBag
    {
        /// <summary>
        /// Gets or sets the identifier of the certificate configuration object.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the PEM encoded private key.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the list of one or more PEM encoded certificates.
        /// </summary>
        public List<string> Certificates { get; set; }

        /// <summary>
        /// Gets or sets the hash identifier of the primary certificate.
        /// </summary>
        public string Hash { get; set; }
    }
}
