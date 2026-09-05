using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using com.blueboxmoon.AcmeCertificate.ViewModels;

namespace com.blueboxmoon.AcmeCertificate.Blocks
{
    /// <summary>
    /// Configures a certificate, including its domains, IIS bindings, and renewal.
    /// </summary>
    [DisplayName( "Acme Certificate Detail" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Configures a certificate." )]
    [IconCssClass( "fa fa-certificate" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.ACME_CERTIFICATE_DETAIL )]
    [Rock.SystemGuid.BlockTypeGuid( SystemGuid.BlockType.ACME_CERTIFICATE_DETAIL_OBSIDIAN )]
    public class AcmeCertificateDetail : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string Id = "Id";
        }

        private static class AttributeKey
        {
            public const string RedirectOverride = "RedirectOverride";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => RequestContext.ResolveRockUrl( "~/Plugins/com_blueboxmoon/AcmeCertificate/acmeCertificateDetail.obs" );

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );

                return BuildDetailBag( group, rockContext );
            }
        }

        /// <summary>
        /// Gets the certificate group identified by the page parameter, or null when adding a new one.
        /// </summary>
        private Group GetCurrentGroup( RockContext rockContext )
        {
            var idParam = RequestContext.GetPageParameter( PageParameterKey.Id );

            if ( string.IsNullOrEmpty( idParam ) || idParam == "0" )
            {
                return null;
            }

            return new GroupService( rockContext ).Get( idParam, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Builds the full detail/edit/IIS state for the specified certificate group.
        /// </summary>
        private AcmeCertificateDetailBag BuildDetailBag( Group group, RockContext rockContext )
        {
            var account = AcmeHelper.LoadAccountData();

            var bag = new AcmeCertificateDetailBag
            {
                OfflineMode = account.OfflineMode,
                ParentPageUrl = this.GetParentPageUrl(),
                Domains = new List<string>(),
                Bindings = new List<BindingBag>(),
                SiteRedirectNames = new List<string>()
            };

            if ( group == null )
            {
                bag.IsNew = true;
                bag.IdKey = string.Empty;
                bag.Name = string.Empty;
                bag.RemoveOldCertificate = false;

                return bag;
            }

            group.LoadAttributes( rockContext );

            bag.IsNew = false;
            bag.IdKey = group.IdKey;
            bag.Name = group.Name;
            bag.RemoveOldCertificate = group.GetAttributeValue( "RemoveOldCertificate" ).AsBoolean( false );
            bag.Domains = group.GetAttributeValue( "Domains" ).SplitDelimitedValues().ToList();
            bag.Bindings = ParseBindings( group.GetAttributeValue( "Bindings" ) );
            bag.LastRenewed = group.GetAttributeValue( "LastRenewed" );
            bag.Expires = group.GetAttributeValue( "Expires" );

            ApplyIisState( bag, rockContext );

            return bag;
        }

        /// <summary>
        /// Parses the pipe-delimited bindings attribute value into a list of bags.
        /// </summary>
        private List<BindingBag> ParseBindings( string value )
        {
            return ( value ?? string.Empty )
                .Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( b => new BindingData( b ) )
                .Select( ToBindingBag )
                .ToList();
        }

        private static BindingBag ToBindingBag( BindingData b )
        {
            return new BindingBag
            {
                Site = b.Site,
                IpAddress = b.IPAddress,
                Port = b.Port,
                Domain = b.Domain
            };
        }

        private static BindingData ToBindingData( BindingBag b )
        {
            return new BindingData
            {
                Site = b.Site,
                IPAddress = b.IpAddress,
                Port = b.Port,
                Domain = b.Domain
            };
        }

        private static CertificateDataBag ToCertificateDataBag( CertificateData c )
        {
            return new CertificateDataBag
            {
                Id = c.Id,
                PrivateKey = c.PrivateKey,
                Certificates = c.Certificates,
                Hash = c.Hash
            };
        }

        private static CertificateData ToCertificateData( CertificateDataBag c )
        {
            return new CertificateData
            {
                Id = c.Id,
                PrivateKey = c.PrivateKey,
                Certificates = c.Certificates,
                Hash = c.Hash
            };
        }

        /// <summary>
        /// Gets the URL that Acme Challenge requests should be redirected to for verification.
        /// </summary>
        private string GetRedirectUrl()
        {
            var url = GetAttributeValue( AttributeKey.RedirectOverride );

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = string.Format( "{0}.well-known/acme-challenge/", GlobalAttributesCache.Value( "PublicApplicationRoot" ) );
            }

            return url;
        }

        /// <summary>
        /// Performs the IIS configuration checks and records any warnings on the bag.
        /// </summary>
        private void ApplyIisState( AcmeCertificateDetailBag bag, RockContext rockContext )
        {
            var targetUrl = GetRedirectUrl();
            var groupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.ACME_CERTIFICATES ).Id;
            var bindings = new List<BindingData>();
            var siteNames = new List<string>();
            var currentSiteName = System.Web.Hosting.HostingEnvironment.SiteName;

            var groups = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );

            //
            // Determine if we have any certificates that edit bindings of a site other than the Rock site.
            //
            foreach ( var group in groups )
            {
                group.LoadAttributes( rockContext );

                bindings.AddRange( group.GetAttributeValue( "Bindings" )
                    .Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( b => new BindingData( b ) )
                    .Where( b => b.Site != currentSiteName ) );

                siteNames.AddRange( bindings.Where( b => !AcmeHelper.IsIISSiteRedirectEnabled( b.Site, targetUrl ) ).Select( b => b.Site ) );
            }

            bag.TargetRedirectUrl = targetUrl;

            //
            // If we have non-Rock sites to configure, ensure that the Http Redirect module has been installed.
            //
            if ( bindings.Any() && !AcmeHelper.IsHttpRedirectModuleEnabled() )
            {
                bag.ShowRedirectModuleWarning = true;
                bag.ShowSiteRedirectWarning = false;
            }
            else
            {
                bag.ShowRedirectModuleWarning = false;

                //
                // If the redirect module has been installed but we have sites that need to be
                // configured, then present a notice about those sites.
                //
                if ( siteNames.Any() )
                {
                    bag.ShowSiteRedirectWarning = true;
                    bag.SiteRedirectNames = siteNames.Distinct().ToList();
                }
                else
                {
                    bag.ShowSiteRedirectWarning = false;
                }
            }
        }

        #endregion Methods

        #region Edit Block Actions

        /// <summary>
        /// Gets the sites and IP addresses available when editing a binding.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetBindingOptions()
        {
            var sites = new List<string>();
            var ipAddresses = new List<string>();

            try
            {
                sites = AcmeHelper.GetSites().ToList();
            }
            catch { /* Intentionally left blank */ }

            try
            {
                ipAddresses = AcmeHelper.GetIPv4Addresses().ToList();
            }
            catch { /* Intentionally left blank */ }

            return ActionOk( new BindingOptionsBag
            {
                Sites = sites,
                IpAddresses = ipAddresses
            } );
        }

        /// <summary>
        /// Recommends a domain and binding configuration based on the current IIS bindings.
        /// </summary>
        /// <param name="current">The domains and bindings currently entered in the editor.</param>
        [BlockAction]
        public BlockActionResult RecommendConfiguration( AcmeCertificateEditDataBag current )
        {
            using ( var rockContext = new RockContext() )
            {
                var allBindings = AcmeHelper.GetExistingBindings( null );
                string siteName = System.Web.Hosting.HostingEnvironment.SiteName;
                var siteBindings = allBindings.Where( b => siteName.Equals( b.Site, StringComparison.CurrentCultureIgnoreCase ) ).ToList();

                //
                // Get all the configured domain names in Rock. Skip any domains with "/" in case
                // they configured poorly.
                //
                var domainNames = ( current.Domains ?? new List<string>() ).ToList();
                var newDomainNames = new SiteDomainService( rockContext ).Queryable()
                    .Select( d => d.Domain )
                    .Where( d => !d.Contains( "/" ) )
                    .Where( d => !d.Equals( "localhost", StringComparison.CurrentCultureIgnoreCase ) )
                    .ToList();
                domainNames.AddRange( newDomainNames.Where( d => !domainNames.Any( a => a.ToLower() == d.ToLower() ) ) );

                //
                // Check if we have a default binding (i.e. blank domain).
                //
                bool hasDefaultBinding = siteBindings.Any( b => string.IsNullOrWhiteSpace( b.Domain ) );

                //
                // Make a list of all the site bindings on port 80 without a 443 binding.
                //
                var needSecureBindings = siteBindings.Where( b => b.Port == 80 )
                    .Where( b => !siteBindings.Any( a => a.Site == b.Site && a.IPAddress == b.IPAddress && a.Domain == b.Domain && a.Port == 443 ) )
                    .ToList();

                //
                // Add new 443 bindings for any of those port 80-only bindings.
                //
                foreach ( var binding in needSecureBindings )
                {
                    siteBindings.Add( new BindingData
                    {
                        Site = binding.Site,
                        IPAddress = binding.IPAddress,
                        Domain = binding.Domain,
                        Port = 443
                    } );
                }

                //
                // Add SSL bindings for any domains that we don't have bindings for.
                //
                if ( !hasDefaultBinding )
                {
                    foreach ( var domain in domainNames )
                    {
                        if ( siteBindings.Any( b => domain.Equals( b.Domain, StringComparison.CurrentCultureIgnoreCase ) ) )
                        {
                            continue;
                        }

                        var newBinding = new BindingData
                        {
                            Site = siteName,
                            Domain = domain,
                            Port = 443
                        };

                        var firstExistingBinding = siteBindings.FirstOrDefault();
                        newBinding.IPAddress = firstExistingBinding != null ? firstExistingBinding.IPAddress : string.Empty;

                        //
                        // Make sure we would not be generating a binding that already exists in another site.
                        //
                        if ( allBindings.Any( b => b.IPAddress == newBinding.IPAddress && b.Port == newBinding.Port && newBinding.Domain.Equals( b.Domain, StringComparison.CurrentCultureIgnoreCase ) ) )
                        {
                            continue;
                        }

                        siteBindings.Add( newBinding );
                    }
                }

                //
                // Filter down to just the 443 bindings, thats all we care about.
                //
                siteBindings = siteBindings.Where( b => b.Port == 443 ).ToList();

                //
                // Merge with the bindings already entered in the editor.
                //
                var currentBindings = ( current.Bindings ?? new List<BindingBag>() ).Select( ToBindingData ).ToList();
                var newBindings = siteBindings.Where( b => !currentBindings.Any( a =>
                    ( a.Site ?? string.Empty ).ToLower() == ( b.Site ?? string.Empty ).ToLower() &&
                    ( a.Domain ?? string.Empty ).ToLower() == ( b.Domain ?? string.Empty ).ToLower() &&
                    a.IPAddress == b.IPAddress &&
                    a.Port == b.Port ) );
                currentBindings.AddRange( newBindings );

                return ActionOk( new AcmeCertificateEditDataBag
                {
                    Domains = domainNames,
                    Bindings = currentBindings.Select( ToBindingBag ).ToList()
                } );
            }
        }

        /// <summary>
        /// Saves the certificate configuration.
        /// </summary>
        /// <param name="bag">The certificate data to save.</param>
        [BlockAction]
        public BlockActionResult Save( AcmeCertificateSaveBag bag )
        {
            var domains = ( bag.Domains ?? new List<string>() )
                .Where( d => !string.IsNullOrWhiteSpace( d ) )
                .ToList();

            //
            // Verify we have at least one domain name entered.
            //
            if ( domains.Count == 0 )
            {
                return ActionBadRequest( "You must enter at least one domain to validate." );
            }

            var bindings = bag.Bindings ?? new List<BindingBag>();

            //
            // Verify we have at least one binding configured, unless we are in offline mode.
            //
            if ( bindings.Count == 0 && !AcmeHelper.LoadAccountData().OfflineMode )
            {
                return ActionBadRequest( "You must add at least one IIS binding." );
            }

            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var group = GetCurrentGroup( rockContext );

                if ( group == null )
                {
                    group = new Group
                    {
                        GroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.ACME_CERTIFICATES ).Id
                    };

                    groupService.Add( group );
                }

                group.LoadAttributes( rockContext );

                group.Name = bag.Name;
                group.SetAttributeValue( "RemoveOldCertificate", bag.RemoveOldCertificate.ToString() );
                group.SetAttributeValue( "Domains", string.Join( "|", domains ) );
                group.SetAttributeValue( "Bindings", string.Join( "|", bindings.Select( b => ToBindingData( b ).ToString() ) ) );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    group.SaveAttributeValues( rockContext );
                } );

                return ActionOk( BuildDetailBag( group, rockContext ) );
            }
        }

        /// <summary>
        /// Deletes the certificate configuration.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeleteCertificate()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var group = GetCurrentGroup( rockContext );

                if ( group == null )
                {
                    return ActionBadRequest( "Certificate not found." );
                }

                groupService.Delete( group );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        #endregion Edit Block Actions

        #region IIS Block Actions

        /// <summary>
        /// Enables the IIS Http Redirect module.
        /// </summary>
        [BlockAction]
        public BlockActionResult EnableRedirectModule()
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );

                string error = null;

                if ( !AcmeHelper.EnableIISHttpRedirectModule() )
                {
                    error = "Failed to enable the IIS Http Redirect module. Rock may not have enough permissions to perform this task. Please manually enable the IIS Http Redirect module.";
                }

                var bag = BuildDetailBag( group, rockContext );
                bag.IisErrorMessage = error;

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Enables the required site redirects to forward Acme Challenge requests to Rock.
        /// </summary>
        [BlockAction]
        public BlockActionResult EnableSiteRedirects()
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );
                var targetUrl = GetRedirectUrl();

                //
                // Recompute the sites that still need to be configured.
                //
                var stateBag = new AcmeCertificateDetailBag { SiteRedirectNames = new List<string>() };
                ApplyIisState( stateBag, rockContext );
                var siteNames = stateBag.SiteRedirectNames ?? new List<string>();

                var errors = new List<string>();

                foreach ( var siteName in siteNames )
                {
                    try
                    {
                        AcmeHelper.EnableIISSiteRedirect( siteName, targetUrl );
                    }
                    catch ( Exception ex )
                    {
                        errors.Add( ex.Message );
                    }
                }

                var bag = BuildDetailBag( group, rockContext );

                if ( errors.Any() )
                {
                    bag.IisErrorMessage = string.Format( "Failed to enable the redirect on one or more sites. This may be due to insufficient permissions to make modifications to the other sites. <ul><li>{0}</li></ul>",
                        string.Join( "</li><li>", errors ) );
                }

                return ActionOk( bag );
            }
        }

        #endregion IIS Block Actions

        #region Renewal Block Actions

        /// <summary>
        /// Gets the hex-encoded hash of the currently installed certificate.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetCertificateHash()
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );

                if ( group == null )
                {
                    return ActionBadRequest( "Certificate not found." );
                }

                group.LoadAttributes( rockContext );

                var hash64 = group.GetAttributeValue( "CertificateHash" );

                if ( string.IsNullOrWhiteSpace( hash64 ) )
                {
                    return ActionOk( hash64 ?? string.Empty );
                }

                var hex = string.Join( string.Empty, Convert.FromBase64String( hash64 ).Select( c => string.Format( "{0:X2}", c ) ) );

                return ActionOk( hex );
            }
        }

        /// <summary>
        /// Renews the certificate, optionally using a custom CSR.
        /// </summary>
        /// <param name="csr">An optional custom PEM-encoded certificate signing request.</param>
        [BlockAction]
        public BlockActionResult RenewCertificate( string csr = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );

                if ( group == null )
                {
                    return ActionBadRequest( "Certificate not found." );
                }

                int certificateId = group.Id;
                string errorMessage;
                CertificateData certificateData;

                if ( !string.IsNullOrWhiteSpace( csr ) )
                {
                    string csrText = csr
                        .Replace( "-----BEGIN CERTIFICATE REQUEST-----", string.Empty )
                        .Replace( "-----END CERTIFICATE REQUEST-----", string.Empty )
                        .Trim();
                    byte[] csrData = Convert.FromBase64String( csrText );
                    certificateData = AcmeHelper.RenewCsrRequest( certificateId, csrData, out errorMessage );
                }
                else
                {
                    certificateData = AcmeHelper.RenewCertificate( certificateId, out errorMessage );
                }

                if ( !string.IsNullOrWhiteSpace( errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( ToCertificateDataBag( certificateData ) );
            }
        }

        /// <summary>
        /// Installs the renewed certificate and configures IIS.
        /// </summary>
        /// <param name="certificate">The certificate material returned from the renew step.</param>
        [BlockAction]
        public BlockActionResult InstallCertificate( CertificateDataBag certificate )
        {
            AcmeHelper.InstallCertificateData( ToCertificateData( certificate ), true );

            return ActionOk();
        }

        /// <summary>
        /// Verifies that a certificate is correctly installed and bound in IIS.
        /// </summary>
        /// <param name="certificateHash">The hash of the certificate to verify.</param>
        [BlockAction]
        public BlockActionResult VerifyCertificateInstalled( string certificateHash )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = GetCurrentGroup( rockContext );

                if ( group == null )
                {
                    return ActionBadRequest( "Certificate not found." );
                }

                return ActionOk( AcmeHelper.VerifyCertificateBindings( group.Id, certificateHash ) );
            }
        }

        /// <summary>
        /// Removes an old certificate from the certificate store by its hex-encoded hash.
        /// </summary>
        /// <param name="certificateHash">The hex-encoded hash of the certificate to remove.</param>
        [BlockAction]
        public BlockActionResult DeleteCertificateHash( string certificateHash )
        {
            byte[] hash = new byte[certificateHash.Length / 2];

            for ( int i = 0; i < certificateHash.Length; i += 2 )
            {
                hash[i / 2] = Convert.ToByte( certificateHash.Substring( i, 2 ), 16 );
            }

            AcmeHelper.RemoveCertificate( hash );

            return ActionOk();
        }

        #endregion Renewal Block Actions
    }
}
