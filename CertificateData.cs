using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate
{
    /// <summary>
    /// Defines information about a Certificate that can be used in the REST API.
    /// </summary>
    public class CertificateData
    {
        /// <summary>
        /// The identifier of the certificate configuration object.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The PEM encoded private key.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// A list of one or more PEM encoded certificates.
        /// </summary>
        public List<string> Certificates { get; set; }

        /// <summary>
        /// The hash identifier of the primary certificate.
        /// </summary>
        public string Hash { get; set; }
    }
}
