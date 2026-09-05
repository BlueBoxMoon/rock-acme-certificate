using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The available choices used when editing an IIS binding.
    /// </summary>
    public class BindingOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of IIS site names.
        /// </summary>
        public List<string> Sites { get; set; }

        /// <summary>
        /// Gets or sets the list of IPv4 addresses available on the server.
        /// </summary>
        public List<string> IpAddresses { get; set; }
    }
}
