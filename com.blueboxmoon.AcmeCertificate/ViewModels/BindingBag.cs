namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// Describes a single IIS binding associated with a certificate.
    /// </summary>
    public class BindingBag
    {
        /// <summary>
        /// Gets or sets the IIS site name.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the IP address, or empty for all addresses.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the host/domain name, or empty for all domains.
        /// </summary>
        public string Domain { get; set; }
    }
}
