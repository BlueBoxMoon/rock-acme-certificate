using System.Collections.Generic;

namespace com.blueboxmoon.AcmeCertificate.ViewModels
{
    /// <summary>
    /// The full state of the Acme Certificate Detail block, covering both the
    /// read-only detail view and the edit form, plus the IIS configuration warnings.
    /// </summary>
    public class AcmeCertificateDetailBag
    {
        /// <summary>
        /// Gets or sets the identity key of the certificate group, or empty when new.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a new (unsaved) certificate.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the URL of the parent page, used when cancelling or after deletion.
        /// </summary>
        public string ParentPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the certificate.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the old certificate is removed after renewal.
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

        /// <summary>
        /// Gets or sets the last renewed date, formatted for display.
        /// </summary>
        public string LastRenewed { get; set; }

        /// <summary>
        /// Gets or sets the expiration date, formatted for display.
        /// </summary>
        public string Expires { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account operates in offline mode.
        /// When offline, IIS bindings are not required and certificates are provided
        /// for manual installation.
        /// </summary>
        public bool OfflineMode { get; set; }

        #region IIS Configuration Warnings

        /// <summary>
        /// Gets or sets a value indicating whether the IIS Http Redirect module warning is shown.
        /// </summary>
        public bool ShowRedirectModuleWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the site-redirect warning is shown.
        /// </summary>
        public bool ShowSiteRedirectWarning { get; set; }

        /// <summary>
        /// Gets or sets the names of the sites that need redirect rules configured.
        /// </summary>
        public List<string> SiteRedirectNames { get; set; }

        /// <summary>
        /// Gets or sets the target URL that Acme Challenge requests are redirected to.
        /// </summary>
        public string TargetRedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets an error message produced while enabling IIS features, if any.
        /// </summary>
        public string IisErrorMessage { get; set; }

        #endregion
    }
}
